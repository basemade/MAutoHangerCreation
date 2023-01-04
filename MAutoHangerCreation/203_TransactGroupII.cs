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
    //TransactionGroup範例
    //簡要版，可以使用這個就好

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TransactGroupII : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //創建模型線
            using (TransactionGroup transGroup = new TransactionGroup(doc, "Level & Grid"))
            {
                transGroup.Start();              
                    if (CreateLevel(doc, 25.0) && CreateGrid(doc, new XYZ(0, 0, 0), new XYZ(10, 0, 0)))
                    {   
                        transGroup.Assimilate();
                    }
                    else
                    {
                        transGroup.RollBack();
                    }
            }
            return Result.Succeeded;
        }

        public bool CreateLevel(Document doc, double elevation)
        {
            using (Transaction transAct = new Transaction(doc, "Creating Level"))
            {
                transAct.Start();
                if (null != Level.Create(doc, elevation))
                {
                    transAct.Commit();
                }
                else
                    transAct.RollBack();
            }
            return true;
        }

        public bool CreateGrid(Document doc, XYZ p1, XYZ p2)
        {
            using (Transaction transAct = new Transaction(doc, "Creating Grid"))
            {
                transAct.Start();
                Line gridLine = Line.CreateBound(p1, p2);

                if ((null != gridLine) && (null != Grid.Create(doc, gridLine)))
                {
                    transAct.Commit();
                }
                else
                    transAct.RollBack();
                
            }
            return true;
        }

    }
}

