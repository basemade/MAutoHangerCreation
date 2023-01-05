using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace MAutoHangerCreation
{
    //CHC做法：點選創建吊架->每點一次就會在當處生成一個吊架
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MultiCreate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            while (true)
            {
                try
                {
                    UIApplication uiapp = commandData.Application;
                    UIDocument uidoc = uiapp.ActiveUIDocument;
                    Document doc = uidoc.Document;
                    Selection sel = uidoc.Selection;
                    StringBuilder st = new StringBuilder();

                    #region 篩選：管附件+族群
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    ElementClassFilter filter1 = new ElementClassFilter(typeof(FamilySymbol));
                    ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
                    LogicalAndFilter andFilter = new LogicalAndFilter(filter1, filter2);
                    IList<Element> symbolList = collector.WherePasses(andFilter).ToElements();
                    #endregion

                    #region 篩選：吊架
                    string paraName = "API識別名稱";
                    string targetName = "吊架";
                    List<Element> filteredByPara = new List<Element>();
                    foreach (Element sym in symbolList)
                    {
                        Parameter symPara = sym.LookupParameter(paraName);
                        if (symPara?.AsString().Contains(targetName) == true)
                        {
                            filteredByPara.Add(sym);
                        }
                    }
                    #endregion

                    #region 篩選：樓層
                    FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                    IList<Element> theLevels = collector2.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements();
                    
                    var findlevels = from element in theLevels
                                        where element.Name == "1FL"
                                        select element;

                    List<Element> levList = findlevels.ToList<Element>();
                    Level lev = levList[0] as Level;
                    #endregion

                    #region 確認轉型
                    FamilySymbol famSym = filteredByPara[0] as FamilySymbol;
                    #endregion

               
                    ////////////////////////////////////////////////////////////////////////////////////////////
                    using (Transaction transAct = new Transaction(doc))
                    {
                        transAct.Start("Create standard-alone instance");

                        #region 獲取點的方式 2，step1：PickObject
                        PipeSelFilter gagaFilter = new PipeSelFilter(doc);
                        Reference selPipePtRef = sel.PickObject(ObjectType.PointOnElement, gagaFilter);
                        XYZ pt = selPipePtRef.GlobalPoint;
                        #endregion

                        #region 獲取點的方式 2，step2：找尋在管中心線上的最近點
                        Element turnRefToElem = doc.GetElement(selPipePtRef.ElementId);
                        LocationCurve locaCrv = turnRefToElem.Location as LocationCurve;
                        #endregion

                        #region 獲取點的方式 2，step2：找尋在管中心線上的最近點
                        IntersectionResult projectResult = locaCrv.Curve.Project(pt);
                        XYZ ptClosest = projectResult.XYZPoint;
                        #endregion

                        #region 修正朝向
                        double ptClosestPara = projectResult.Parameter;
                        Transform transform = locaCrv.Curve.ComputeDerivatives(0, true);
                        XYZ dir = transform.BasisX;
                        #endregion

                    ////////////////////////////////////////////////////////////////////////////////////////////

                        #region 創建吊架
                        if (!famSym.IsActive)
                        {
                            famSym.Activate();
                        }

                        Element elemPicked = doc.GetElement(selPipePtRef);
                        FamilyInstance famIns = doc.Create.NewFamilyInstance(ptClosest, famSym, dir, elemPicked, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        #endregion

                        transAct.Commit();
                    }
                }
                catch 
                {
                    break;
                }
            }
            return Result.Succeeded;
        }



        #region 這裡的Filter是for Selection
        public class PipeSelFilter : ISelectionFilter
        {
            Document docDefault = null;
            public PipeSelFilter(Document doc)
            {
                docDefault = doc;
            }

            public bool AllowElement(Element eForFil)
            {

                Category pipe = Category.GetCategory(docDefault, BuiltInCategory.OST_PipeCurves);
                Category duct = Category.GetCategory(docDefault, BuiltInCategory.OST_DuctCurves);
                Category conduit = Category.GetCategory(docDefault, BuiltInCategory.OST_Conduit);
                Category tray = Category.GetCategory(docDefault, BuiltInCategory.OST_CableTray);

                if (eForFil.Category.Id == pipe.Id ||
                    eForFil.Category.Id == duct.Id ||
                    eForFil.Category.Id == conduit.Id ||
                    eForFil.Category.Id == tray.Id)
                    return true;
                else
                    return false;
            }

            public bool AllowReference(Reference r, XYZ p)
            {
                return true;
            }
        }
        #endregion    
    }
}

