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
    //透過02_CheckType這個CS可以發現，01_GetAllPipeAccessory並沒有將ElementType與Instance分離
    //ps.ElementType is a superclass of FamilySymbol，因此可以利用WhereElementIsNotElementType分離類型與實例
        //ps.也需注意目前管附件裡面都是FamilySymbol，若Category裡面有ElementType，就需要再做區別

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CheckType : IExternalCommand{
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements){
            UIApplication uiapp = commandData.Application;
			UIDocument uidoc = uiapp.ActiveUIDocument;
			Document doc = uidoc.Document;
            StringBuilder st = new StringBuilder();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);

            IList<Element> elemList_1 = collector.WherePasses(filter).ToElements();
            st.AppendLine("FilteredElementCollector收集到的有：" + elemList_1.Count().ToString());

            IList<Element> elemList_2 = collector.WherePasses(filter).WhereElementIsElementType().ToElements();
            st.AppendLine("WhereElementIsElementType：" + elemList_2.Count().ToString());

            IList<Element> elemList_3 = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();
            st.AppendLine("WhereElementIsNotElementType：" + elemList_3.Count().ToString());

            MessageBox.Show(st.ToString());
			return Result.Succeeded;
        }
    }
}

