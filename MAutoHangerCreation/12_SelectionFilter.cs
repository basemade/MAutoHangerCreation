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
    //建立帶有篩選的pickobject

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class SelectionFilter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            StringBuilder st = new StringBuilder();

            PipeSelFilter gagaFilter = new PipeSelFilter(doc);
            //ISelectionFilter gagaFilter = new PipeSelFilter(doc);
            //目前經驗，這兩種實作做法都不會報錯，推斷可能是與介面的創建方式有關
            //或許 AU所寫的 ISelectionFilter 可以支援?
            //以下案例說明，若沒有明確實作，會報錯：
            //https://learn.microsoft.com/zh-tw/dotnet/csharp/programming-guide/interfaces/how-to-explicitly-implement-interface-members


            Reference selPipeRef = sel.PickObject(ObjectType.Element, gagaFilter);
            Element elem = doc.GetElement(selPipeRef);

            Parameter para = elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM);
            st.AppendLine("族群與類型：");
            st.AppendLine(para.AsValueString());
            MessageBox.Show(st.ToString());
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

