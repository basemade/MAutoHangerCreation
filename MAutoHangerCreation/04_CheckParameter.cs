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
    //觀察篩選出來的Element的參數
    //記得在Revit裡頭刪除的其餘管附件，只留下管吊架以利觀察
    //>>>>>觀察結果：得到的parameter是源於FamilySymbol，parameter中包括"API識別名稱"

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CheckParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            StringBuilder st = new StringBuilder();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementClassFilter filter1 = new ElementClassFilter(typeof(FamilySymbol));
            ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
            LogicalAndFilter andFilter = new LogicalAndFilter(filter1, filter2);
            IList<Element> elemList = collector.WherePasses(andFilter).ToElements();

            st.AppendLine("FilteredElementCollector收集到的第一個element是：");
            Parameter para = elemList[0].get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            st.AppendLine(para.AsString() + "......" + elemList[0].Name);
            st.AppendLine();

            st.AppendLine("這個element所含有的參數是：");
            foreach (Parameter para1 in elemList[0].Parameters)
            {
                st.AppendLine(getParameterInformation(para1, doc));
            }

            MessageBox.Show(st.ToString());
            return Result.Succeeded;
        }

        //下面方法參考台大API_Tutorial_9
        private string getParameterInformation(Parameter para, Document document)
        {
            string defName = para.Definition.Name;

            switch (para.StorageType)
            {
                case StorageType.Double:
                    return defName + ":" + para.AsValueString();

                case StorageType.ElementId:
                    ElementId id = para.AsElementId();
                    if (id.IntegerValue >= 0)
                        return defName + ":" + document.GetElement(id).Name;
                    else
                        return defName + ":" + id.IntegerValue.ToString();

                case StorageType.Integer:
                    if (ParameterType.YesNo == para.Definition.ParameterType)
                    {
                        if (para.AsInteger() == 0)
                            return defName + ":" + false.ToString();
                        else
                            return defName + ":" + true.ToString();
                    }
                    else
                        return defName + ":" + para.AsInteger().ToString();

                case StorageType.String:
                    return defName + ":" + para.ToString();

                default:
                    return "未公開的參數";

            }
        }
    }
}





        