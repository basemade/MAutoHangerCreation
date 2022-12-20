using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;


namespace MAutoHangerCreation
{
    //本案例利用ElementClassFilter來篩選出FamilySymbol

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class FilterByParameter : IExternalCommand{
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements){
            UIApplication uiapp = commandData.Application;
			UIDocument uidoc = uiapp.ActiveUIDocument;
			Document doc = uidoc.Document;
            StringBuilder st = new StringBuilder();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementClassFilter filter1 = new ElementClassFilter(typeof(FamilySymbol));
            ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
            LogicalAndFilter andFilter = new LogicalAndFilter(filter1, filter2);
            IList<Element> elemList = collector.WherePasses(andFilter).ToElements();

            string paraName = "API識別名稱";
            string targetName = "吊架";
            List<Element> filteredByPara = new List<Element>();

            //自建Filter
            foreach (Element elem in elemList) {
                Parameter para = elem.LookupParameter(paraName);
                //在需要存取某物件的屬性之前，我們通常會先檢查該物件是否為 null，以免程式執行時拋出異常（NullReferenceException）
                /*
                if (para != null && para.AsString().Contains(targetName) == true) {
                    filteredByPara.Add(elem);
                }
                */

                //C# 6 新增了 null 條件運算子語法，讓你用更簡短的語法達到同樣效果：
                //先判斷物件是否為 null，不是 null 才會存取其成員。它的寫法是在變數後面跟著一個問號，然後是存取成員名稱的表示式。
                if (para?.AsString().Contains(targetName) == true)
                {
                    filteredByPara.Add(elem);
                }
            }

            st.AppendLine("FilteredByParameter收集到的有：");
            
            foreach (Element elem in filteredByPara) {
                Parameter para = elem.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                st.AppendLine(para.AsString() + "......" + elem.Name);
            }

            MessageBox.Show(st.ToString());
			return Result.Succeeded;
        }
    }
}

