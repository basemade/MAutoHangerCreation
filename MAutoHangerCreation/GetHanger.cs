using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;



namespace MAutoHangerCreation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class GetHanger:IExternalCommand:{
        public Result Execute(ExternalCommandData commandData, out string message, ElementSet elements) { 
    }
    }
}
