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
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class GetAllPipeAccessory : IExternalCommand{
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements){
            UIApplication uiapp = commandData.Application;
			UIDocument uidoc = uiapp.ActiveUIDocument;
			Document doc = uidoc.Document;
            StringBuilder st = new StringBuilder();

            FilteredElementCollector collection = new FilteredElementCollector(doc);
            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);

            IList<Element> elemList = collection.WherePasses(filter).ToElements();
            st.AppendLine("FilteredElementCollector收集到的有：");
            foreach (Element elem in elemList) {
                Parameter para = elem.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                st.AppendLine(para.AsString() + "......" + elem.Name);
            }

            MessageBox.Show(st.ToString());
			return Result.Succeeded;
        }
    }
}

