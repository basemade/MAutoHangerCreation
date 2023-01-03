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
    //Transaction範例：
    //預先定義一些"幾何"的線和面
    //然後再透過Transaction生成"草圖"的面
    //ps.直接在Revit裡製作物件(ex:牆、樓板...)的順序：幾何->草圖->物件
    //https://blog.csdn.net/weixin_44153630/article/details/105964625

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Transact : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            #region 預先定義幾何線
            XYZ point1 = XYZ.Zero;
            XYZ point2 = new XYZ(10, 0, 0);
            XYZ point3 = new XYZ(10, 10, 0);
            XYZ point4 = new XYZ(0, 10, 0);

            Line geomLine1 = Line.CreateBound(point1, point2);
            Line geomLine2 = Line.CreateBound(point4, point3);
            Line geomLine3 = Line.CreateBound(point1, point4);
            #endregion

            #region 預先定義幾何平面
            XYZ origin = XYZ.Zero;
            XYZ normal = new XYZ(0, 0, 1);
            Plane geomPlane = Plane.CreateByNormalAndOrigin(normal, origin);
            //創建平面為static method
            //https://www.revitapidocs.com/2019/4cf2ea2c-2907-adb4-9481-b716a4291df1.htm
            #endregion

            //創建模型線
            using (Transaction transAct = new Transaction(doc))
            {
                if (transAct.Start("Create model curves") == TransactionStatus.Started)
                //檢查當前狀態是否為Started；這樣寫更嚴謹
                {
                    SketchPlane sketch = SketchPlane.Create(doc, geomPlane);

                    ModelLine modelLine1 = doc.Create.NewModelCurve(geomLine1, sketch) as ModelLine;
                    ModelLine modelLine2 = doc.Create.NewModelCurve(geomLine2, sketch) as ModelLine;
                    ModelLine modelLine3 = doc.Create.NewModelCurve(geomLine3, sketch) as ModelLine;
                }

                //詢問用戶是否要提交
                TaskDialog taskDialog = new TaskDialog("gaga");
                taskDialog.MainContent = "Click either [OK] to Commit, or [Cancel] to Roll back the transaction.";
                TaskDialogCommonButtons buttons = TaskDialogCommonButtons.Ok |
                TaskDialogCommonButtons.Cancel;
                taskDialog.CommonButtons = buttons;


                if (TaskDialogResult.Ok == taskDialog.Show())
                {
                    transAct.Commit();
                    //if (TransactionStatus.Committed != transAct.Commit())
                    //{
                    //    TaskDialog.Show("Failure", "Transaction could not be committed!");
                    //}
                    //不太知道這裡為什麼會這樣寫，不重要
                    //重點就是看commit或roll back
                }
                else
                {
                    transAct.RollBack();
                    TaskDialog.Show("Failure", "Roll back the transaction!");
                }

            }
            return Result.Succeeded;
        }
    }
}

