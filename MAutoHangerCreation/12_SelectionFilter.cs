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
    public class SelectionFilter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            StringBuilder st = new StringBuilder();

            ISelectionFilter gagaFilter = new PipeSelFilter(doc);
            Reference selPipeRefs = sel.PickObject(ObjectType.Element, gagaFilter);


           
            return Result.Succeeded;
        }

        public class PipeSelFilter : ISelectionFilter
        {
            cate
            public bool AllowElement(Element elem)
            {

                if (elem.get_Parameter(BuiltInCategory) )
            }
        }
    }
}

