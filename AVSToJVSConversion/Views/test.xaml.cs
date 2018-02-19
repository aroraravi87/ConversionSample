using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AVSToJVSConversion.BLL;

namespace AVSToJVSConversion.Views
{
    /// <summary>
    /// Interaction logic for test.xaml
    /// </summary>
    public partial class test : Window
    {

        private Dictionary<string, string> _getVariableDictionary = null;
        private InitializeTables _initializeTables = null;
        private Operations _op = null;
        private Utility _utility = null;

        StringBuilder stringBuilder = new StringBuilder();

        string ElementType = string.Empty;
        private string PrevElementType = string.Empty;
        public test()
        {
            InitializeComponent();
            _initializeTables = new InitializeTables();
            _utility = new Utility();
            _getVariableDictionary = new Dictionary<string, string>();
            _getVariableDictionary.Add("int ", "0");
            _getVariableDictionary.Add("Table ", "Util.NULL_TABLE");
            _getVariableDictionary.Add("String ", "\"\"");
            _getVariableDictionary.Add("double ", "0d");
            _getVariableDictionary.Add("ODateTime ", "Util.NULL_DATE_TIME");
            _getVariableDictionary.Add("Instrument ", "Util.NULL_INS_DATA");
            _getVariableDictionary.Add("XString ", "\"\"");
            _getVariableDictionary.Add("Transaction ", "Util.NULL_TRAN");
        }



        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string line = @"int sub,int val;Table txt;String str,int st,Table 12";
            string DicOutput = ""
                ;
            bool appendDataLabel = false;
            bool checkVariableExist = false;
            List<char> symbolArray = new List<char>() { '(', ')' };



            if (!line.Contains('='))
            {
                string[] strItemList = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string lastItem = strItemList.Select(x => x).Last();
                foreach (var itemStr in strItemList)
                {
                    if (itemStr.Trim().Contains(','))
                    {
                        string[] strSubItemList = itemStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string subLastItem = strSubItemList.Select(x => x).Last();
                        foreach (var subItem in strSubItemList)
                        {
                            checkVariableExist =
                                _getVariableDictionary.Keys.Any(
                                    type => _utility.CheckVariableTypeName(subItem, type, out ElementType));
                            _getVariableDictionary.TryGetValue(ElementType, out DicOutput);
                            if (checkVariableExist)
                            {

                                if (!symbolArray.Any(n => subItem.ToCharArray().Contains(n)))
                                {

                                    stringBuilder.Append(subItem.Insert(subItem.Length,
                                        string.Format(" = {0}{1}", DicOutput, subItem.Equals(subLastItem) ? ' ' : ',')));
                                }
                            }
                        }
                    }


                    else
                    {
                        checkVariableExist =
                            _getVariableDictionary.Keys.Any(
                                type => _utility.CheckVariableTypeName(itemStr, type, out ElementType));
                        _getVariableDictionary.TryGetValue(ElementType, out DicOutput);
                        if (checkVariableExist)
                        {

                            if (!symbolArray.Any(n => itemStr.ToCharArray().Contains(n)))
                            {
                                stringBuilder.Append(itemStr.Insert(itemStr.Length,
                                    string.Format(" = {0}{1}", DicOutput, itemStr.Equals(lastItem) ? ' ' : ',')));
                            }
                        }
                    }
                    if (stringBuilder.Length > 0)
                        tblText.Text = stringBuilder.ToString().Trim() + ";";
                    else
                        tblText.Text = line;
                }
            }
            else
            {

                StringBuilder calval1 = new StringBuilder();
                bool IsChanged = false;
                string[] strArray = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                calval1.Clear();
                foreach (string item in strArray)
                {


                    _getVariableDictionary.Keys.Any(
                        type => _utility.CheckVariableTypeName(item, type, out ElementType));
                    if (string.IsNullOrWhiteSpace(ElementType))
                    {
                        ElementType = PrevElementType;
                        appendDataLabel = false;
                    }
                    _getVariableDictionary.TryGetValue(ElementType, out DicOutput);
                    if (!string.IsNullOrWhiteSpace(ElementType) && !string.IsNullOrWhiteSpace(DicOutput))
                    {
                        IsChanged = true;
                        calval1.Append(Operations.ConvertDeclaredValues(item + ';', ElementType, DicOutput,
                            appendDataLabel));
                    }
                }
                if (IsChanged)
                    tblText.Text = calval1.ToString();
                else
                    tblText.Text = line;
            }
        }


    }
}
