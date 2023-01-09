using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


//一個好的示範如何建立Transaction
//https://youtu.be/EYdEOJ4VS8c?t=1389
namespace MAutoHangerCreation
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TransactionIII : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference pickObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (pickObj != null)
                {
                    // Get hosting element geometry
                    ElementId walId = pickObj.ElementId;
                    Element wal = doc.GetElement(walId);

                    LocationCurve locCurve = wal.Location as LocationCurve;

                    Curve crv = locCurve.Curve;
                    XYZ pt = crv.Evaluate(0.5, true);


                    // Collecting family symbols
                    FilteredElementCollector filCollector = new FilteredElementCollector(doc);
                    IList<Element> syms = filCollector.OfCategory(BuiltInCategory.OST_Doors).ToElements();

                    foreach (var i in syms)
                    {
                        FamilySymbol famSym = i as FamilySymbol;
                        if (i.Name == "1810x2110mm")
                        {
                            //Start transaction
                            using (Transaction t = new Transaction(doc, "Place Family Instance"))
                            {
                                t.Start();

                                //Create instance
                                if (!famSym.IsActive)
                                {
                                    famSym.Activate();
                                }

                                doc.Create.NewFamilyInstance(pt, famSym, wal, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                                t.Commit();
                            }
                        }
                    }

                }

                return Result.Succeeded;
            }

            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }
}
