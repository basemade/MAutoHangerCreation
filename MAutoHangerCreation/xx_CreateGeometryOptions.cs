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
    //本案例利用ElementClassFilter來篩選出FamilySymbol

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CreateGeometryOptions : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            StringBuilder st = new StringBuilder();
            Selection sel = uidoc.Selection;


#region 點選特定實作
            PipeSelFilter gagaFilter = new PipeSelFilter(doc);
            Reference selPipeRef = sel.PickObject(ObjectType.Element, gagaFilter);
            Element elem = doc.GetElement(selPipeRef); 

            Parameter para = elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM);
            st.AppendLine("族群與類型：");
            st.AppendLine(para.AsValueString());
            MessageBox.Show(st.ToString());
            st.Clear();
#endregion

#region 創建幾何選項
            Autodesk.Revit.DB.Options geomOption = uiapp.Application.Create.NewGeometryOptions();
            if (null != geomOption)
            {
                Autodesk.Revit.DB.Options option = uiapp.Application.Create.NewGeometryOptions();
                option.ComputeReferences = true;
                option.DetailLevel = ViewDetailLevel.Fine;
                TaskDialog.Show("Revit", "Geometry Option created successfully.");
            }
#endregion

#region 得到幾何訊息
            GeometryElement geoElem = elem.get_Geometry(geomOption);
            int i = 0;
            int j = 0;
            foreach (GeometryObject geomObj in geoElem)
            {
                Solid geomSolid = geomObj as Solid;
                if (null != geomSolid)
                {
                    foreach (Face geomFace in geomSolid.Faces)
                    {
                        st.AppendLine($"Face {i} 的面積 = {geomFace.Area.ToString()}");
                        i++;
                    }
                    MessageBox.Show(st.ToString());
                    st.Clear();

                    foreach (Edge geomEdge in geomSolid.Edges)
                    {
                        j++;
                    }
                    st.AppendLine($"總共有{j}個邊");
                    MessageBox.Show(st.ToString());
                }
            }

            return Result.Succeeded; 
#endregion
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
                //寫法1：可以運作，但其實不用再透過文件去撈
                //if (eForFil.Category.Id == Category.GetCategory(docDefault, BuiltInCategory.OST_PipeCurves).Id)
                //寫法2：寫法不錯
                //if (eForFil.Category.Id.IntegerValue == BuiltInCategory.OST_PipeCurves.GetHashCode())

                //寫法3：針對多種品類去做篩選
                Category pipe = Category.GetCategory(docDefault, BuiltInCategory.OST_PipeCurves);
                Category duct = Category.GetCategory(docDefault, BuiltInCategory.OST_DuctCurves);
                Category conduit = Category.GetCategory(docDefault, BuiltInCategory.OST_Conduit);
                Category tray = Category.GetCategory(docDefault, BuiltInCategory.OST_CableTray);
                //GetCategory 是Static方法，不能也不用new就可以使用(因為已經占據記憶體了)

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
    }
}

