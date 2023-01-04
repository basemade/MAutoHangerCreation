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
    //TransactionGroup範例：

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TransactGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //創建模型線
            using (TransactionGroup transGroup = new TransactionGroup(doc, "Level & Grid"))
            {
                if (transGroup.Start() == TransactionStatus.Started)
                {
                        
                    if (CreateLevel(doc, 25.0) && CreateGrid(doc, new XYZ(0, 0, 0), new XYZ(10, 0, 0)))
                    {   
                        transGroup.Assimilate();
                    }
                    else
                    {
                        transGroup.RollBack();
                    }
                }

            }
            return Result.Succeeded;
        }

        public bool CreateLevel(Document doc, double elevation)
        {
            using (Transaction transAct = new Transaction(doc, "Creating Level"))
            {
                if (TransactionStatus.Started == transAct.Start())
                //Transaction.Start() >>> If finished successfully, this method returns TransactionStatus.Started.
                //1.transAct.Start()執行後，回傳TransactionStatus.Started
                //2.TransactionStatus.Started == TransactionStatus.Started >>> true
                //啟動事務來修改文件
                //更嚴謹一些的寫法
                {
                    if (null != Level.Create(doc, elevation))
                    {
                        return (TransactionStatus.Committed == transAct.Commit());
                    }
                    //if 不能創建樓層，撤銷這個事務
                    transAct.RollBack();
                }
            }
            return false;
        }

        public bool CreateGrid(Document doc, XYZ p1, XYZ p2)
        {
            using (Transaction transAct = new Transaction(doc, "Creating Grid"))
            {
                if (TransactionStatus.Started == transAct.Start())
                {
                    Line gridLine = Line.CreateBound(p1, p2);

                    if ((null != gridLine) && (null != Grid.Create(doc, gridLine)))
                    {
                        if (TransactionStatus.Committed == transAct.Commit())
                        {
                            return true;
                        }
                    }
                    //if 不能創建樓層，撤銷這個事務
                    transAct.RollBack();
                }
            }
            return false;
        }

    }
}

