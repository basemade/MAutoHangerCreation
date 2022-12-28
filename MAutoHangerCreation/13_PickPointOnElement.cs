using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;



namespace MAutoHangerCreation
{
    //透過Reference PickObject(ObjectType.PointOnElement, ISelectionFilter selectionFilter);
    //在滑鼠點擊處創建Instance

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class PickPointOnElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            StringBuilder st = new StringBuilder();

            PipeSelFilter gagaFilter = new PipeSelFilter(doc);
            //XYZ pt = sel.PickPoint(ObjectSnapTypes.Nearest);
            //PickPoint方法也可以獲得點，但要求得在工作平面上才能使用(3D不行)
            Reference selPipePtRef = sel.PickObject(ObjectType.PointOnElement, gagaFilter);
            XYZ pt = selPipePtRef.GlobalPoint;

            //結果：有2個問題待解決
            //PointOnElement得到點位是在模型上的任意位置，非中心線上
            //並且高度不對

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
            Parameter hangerPara = filteredByPara[0].get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            st.AppendLine("經過吊架篩選，找到的是：");
            st.AppendLine(hangerPara.AsString() + "......" + filteredByPara[0].Name);
            MessageBox.Show(st.ToString());
            st.Clear();
            #endregion

            #region 篩選：樓層
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            IList<Element> theLevels = collector2.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements();
            //簡易寫法
            st.AppendLine("此模型的樓層有：");
            foreach (Element elem in theLevels)
            {
                Parameter elemPara = elem.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                st.AppendLine(elemPara.AsString() + "......" + elem.Name);
            }
            st.AppendLine();

            //使用LINQ作為篩選
            st.AppendLine("測試用LINQ語法找出1FL，結果找到：");
            var findlevels = from element in theLevels
                             where element.Name == "1FL"
                             select element;

            //使用LINQ後需要轉型別
            List<Element> levList = findlevels.ToList<Element>();
            Level lev = levList[0] as Level;

            Parameter levPara = lev.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            st.AppendLine(levPara.AsString() + "......" + lev.Name);
            MessageBox.Show(st.ToString());
            st.Clear();
            #endregion

            #region 確認轉型
            FamilySymbol famSym = filteredByPara[0] as FamilySymbol;
            //因為前面已確認symbolList收到的是FamilySymbol，這裡需要做的就是轉型
            Parameter famSymPara = famSym.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            st.AppendLine($"再次檢查轉型是否成功");
            st.AppendLine($"名稱是：{famSym.Name}，型別是：{famSym.GetType()}");
            MessageBox.Show(st.ToString());
            st.Clear();
            #endregion

            using (Transaction transAct = new Transaction(doc))
            {
                transAct.Start("Create standard-alone instance");

                if (!famSym.IsActive)
                {
                    famSym.Activate();
                }

                FamilyInstance famIns = doc.Create.NewFamilyInstance(pt, famSym, lev, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                transAct.Commit();

                st.AppendLine("新增了一個吊架：");
                st.AppendLine($"名稱是：{famIns.Name}，ID是：{famIns.Id}");
                MessageBox.Show(st.ToString());
                st.Clear();
            }


            return Result.Succeeded;
        }



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

                if (eForFil.Category.Id == pipe.Id      ||
                    eForFil.Category.Id == duct.Id      ||
                    eForFil.Category.Id == conduit.Id   ||
                    eForFil.Category.Id == tray.Id )
                    return true;
                else
                    return false;
            }

            public bool AllowReference(Reference r, XYZ p)
            {
                return true;
            }
        }
    }
}

