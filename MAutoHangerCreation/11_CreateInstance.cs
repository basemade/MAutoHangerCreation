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
    public class CreateInstance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            StringBuilder st = new StringBuilder();
            StringBuilder st2 = new StringBuilder();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementClassFilter filter1 = new ElementClassFilter(typeof(FamilySymbol));
            ElementCategoryFilter filter2 = new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory);
            LogicalAndFilter andFilter = new LogicalAndFilter(filter1, filter2);
            IList<Element> symbolList = collector.WherePasses(andFilter).ToElements();

#region 找吊架
            string paraName = "API識別名稱";
            string targetName = "吊架";
            List<Element> filteredByPara = new List<Element>();

            foreach (Element sym in symbolList)
            {
                Parameter symPara = sym.LookupParameter(paraName);
                if (symPara?.AsString().Contains(targetName) == true)
                {
                    filteredByPara.Add(sym);
                }
            }
            Parameter hangerPara = filteredByPara[0].get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            st.AppendLine("要放置的吊架是：");
            st.AppendLine(hangerPara.AsString() + "......" + filteredByPara[0].Name);
            MessageBox.Show(st.ToString()); 
#endregion

#region 找樓層
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            IList<Element> theLevels = collector2.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements();
            //簡易寫法
            st2.AppendLine("此模型的樓層有：");
            foreach (Element elem in theLevels)
            {
                Parameter elemPara = elem.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                st2.AppendLine(elemPara.AsString() + "......" + elem.Name);
            }
            st2.AppendLine();

            //使用LINQ作為篩選
            st2.AppendLine("測試用LINQ語法找出1FL，結果找到：");
            var findlevels = from element in theLevels
                             where element.Name == "1FL"
                             select element;

            //使用LINQ後需要轉型別
            List<Element> levList = findlevels.ToList<Element>();
            Level lev = levList[0] as Level;

            Parameter levPara = lev.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            st2.AppendLine(levPara.AsString() + "......" + lev.Name);
            MessageBox.Show(st2.ToString());
            #endregion


            using (Transaction transAct = new Transaction(doc)) {
                transAct.Start("Create standard-alone instance");
                FamilyInstance famIns = doc.Create.NewFamilyInstance(new XYZ(0,0,0), symbolList[0], lev, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }
            


            return Result.Succeeded;
        }
    }
}

