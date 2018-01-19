using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AVSToJVSConversion.BLL;
using sc = System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using AVSToJVSConversion.Common;


namespace AVSToJVSConversion.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        #region === [Properties]================================

        public ICommand SelectCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properyName"></param>
        private void RaisedPropertyChanged(string properyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(properyName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MainWindowViewModel()
        {

            if (_validateError == null)
            {
                _validateError = new sc.Label();
                ErrorMessage = string.Empty;
            }
            SelectCommand = new CustomCommand(OnActionExecute);
        }
        #endregion

        #region === [Properties]================================

        private string _AVSPath;

        public string AVSPath
        {
            get { return _AVSPath; }
            set
            {
                _AVSPath = value;
                RaisedPropertyChanged("AVSPath");
            }
        }

        private string _libraryPath;

        public string LibraryPath
        {
            get { return _libraryPath; }
            set
            {
                _libraryPath = value;
                RaisedPropertyChanged("LibraryPath");

            }
        }

        private string _jVSPath;

        public string JVSPath
        {
            get { return _jVSPath; }
            set
            {
                _jVSPath = value;
                RaisedPropertyChanged("JVSPath");
            }
        }

        private sc.Label _validateError;

        public sc.Label ValidateError
        {
            get { return _validateError; }
            set
            {
                _validateError = value;
                RaisedPropertyChanged("ValidateError");
            }
        }


        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisedPropertyChanged("ErrorMessage");
            }
        }


        private bool _status;

        public bool Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisedPropertyChanged("Status");
            }
        }
        #endregion

        #region === [Private Methods] =============================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Parameter"></param>
        private void OnActionExecute(object Parameter)
        {
            switch (Parameter.ToString())
            {
                case "BrowseAVS":
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        AVSPath = folderDialog.SelectedPath;
                    }
                    break;
                case "BrowseLibrary":
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        LibraryPath = folderDialog.SelectedPath;
                    }
                    break;
                case "BrowseJVS":
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.ShowDialog();
                        JVSPath = folderDialog.SelectedPath;
                    }
                    break;
                case "Conversion":
                    ConvertAvsFiletoJvs();
                    break;
                case "Refresh":
                    AVSPath = string.Empty;
                    JVSPath = string.Empty;
                    LibraryPath = string.Empty;
                    ValidateError.Opacity = 0;
                    ErrorMessage = string.Empty;
                    Status = false;

                 //   string str = CheckWhile("while((!IsDone || !getvalue(a,b)) && !isdeleted)");

                    break;

            }

        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="line"></param>
        ///// <returns></returns>
        //private string CheckWhile(string line)
        //{
        //    string subStr = string.Empty;
        //    List<char> symbolArray = new List<char>() { '(', ')' };
        //    if (line.StartsWith("while") && symbolArray.Any(n => line.ToCharArray().Contains(n)))
        //    {
        //        if (line.Contains("(("))
        //            subStr = line.Substring(line.IndexOf('(') + 2, line.LastIndexOf(')') - line.IndexOf('(') - 1);
        //        else
        //            subStr = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);

        //        line = line.Replace(subStr, ValidateSymbols(subStr, line));
        //    }
        //    return line;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="strSymbol"></param>
        ///// <param name="line"></param>
        ///// <returns></returns>
        //private string ValidateSymbols(string strSymbol, string line)
        //{

        //    string subNotStr = string.Empty;
        //    string subStringValue = string.Empty;
        //    StringBuilder strbuilder = new StringBuilder();
        //    List<char> symbolList = new List<char>() { '=', '<', '>' };
        //    int counter = 1;
        //    bool openbracket = false;
        //    bool closeBracket = false;

        //    string[] strSymbolList = strSymbol.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);


        //    if (strSymbol.Contains('!') && !strSymbol.Contains("&&") && !strSymbol.Contains("||"))
        //    {
        //        if (strSymbol.Contains(')'))
        //        {
        //            if (strSymbol[strSymbol.LastIndexOf(')') - 1].Equals(')'))
        //            {
        //                strSymbol = strSymbol.Substring(1, strSymbol.Length - 2);
        //                closeBracket = true;
        //            }
        //            strSymbol = strSymbol.Replace(strSymbol,
        //                closeBracket ? string.Concat(strSymbol, "!=", 0, ')') : string.Concat(strSymbol, "!=", 0));
        //            closeBracket = false;
        //        }
        //        else
        //        {
        //            subNotStr = strSymbol.Substring(1, strSymbol.Length - 1);
        //            strSymbol = strSymbol.Replace(strSymbol, string.Concat(subNotStr, "!=", 0));
        //        }
        //        return strSymbol;
        //    }
        //    if (strSymbolList.Length > 0 && (strSymbol.Contains("&&") || strSymbol.Contains("||")))
        //    {
        //        string subItem = string.Empty;
        //        string value = string.Empty;
        //        foreach (var itemStr in strSymbolList)
        //        {
        //            if (itemStr.Contains("||"))
        //            {
        //                string[] strOR = itemStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        //                foreach (string item in strOR)
        //                {
        //                    if (item.StartsWith("("))
        //                    {
        //                        subItem = item.Replace("(", string.Empty);
        //                        openbracket = true;
        //                    }

        //                    else if (item.Contains(')'))
        //                    {

        //                        if (item[item.LastIndexOf(')') - 1] != ')')
        //                        {
        //                            subItem = item.Replace(")", string.Empty);
        //                            closeBracket = true;
        //                        }
        //                        else
        //                            subItem = item;
        //                    }
        //                    else
        //                    {
        //                        subItem = item;
        //                    }
        //                    if (symbolList.Any(x => item.ToCharArray().Contains(x)))
        //                    {
        //                        strbuilder.Append(item);
        //                    }
        //                    else
        //                    {
        //                        value = ValidateSymbols(subItem.Trim(), line);
        //                        if (openbracket)
        //                            value = '(' + value;
        //                        else if (closeBracket)
        //                            value = value + ')';

        //                        strbuilder.Append(value);


        //                    }
        //                    if (counter < strOR.Length)
        //                        strbuilder.Append(" || ");
        //                    counter++;
        //                }
        //            }

        //            else
        //            {
        //                if (itemStr.StartsWith("("))
        //                {
        //                    subItem = itemStr.Replace("(", string.Empty);
        //                    openbracket = true;
        //                }
        //                else if (itemStr.Contains(')'))
        //                {
        //                    if (itemStr[itemStr.LastIndexOf(')') - 1] != ')')
        //                    {
        //                        subItem = itemStr.Replace(")", string.Empty);
        //                        closeBracket = true;
        //                    }
        //                    else
        //                        subItem = itemStr;
        //                }
        //                else
        //                {
        //                    subItem = itemStr;
        //                }


        //                if (symbolList.Any(x => itemStr.ToCharArray().Contains(x)))
        //                {
        //                    strbuilder.Append(string.Concat(" &&", itemStr, "&&"));
        //                }
        //                else
        //                {
        //                    value = ValidateSymbols(subItem.Trim(), line);
        //                    if (openbracket)
        //                        value = '(' + value;
        //                    else if (closeBracket)
        //                        value = value + ')';
        //                    strbuilder.Append(string.Concat(" &&", value, "&&"));
        //                }
        //            }
        //        }

        //        subStringValue = strbuilder.ToString();

        //        if (subStringValue.Trim().StartsWith("&&"))
        //        {
        //            subStringValue = subStringValue.Substring(3, strbuilder.ToString().Length - 3);

        //        }
        //        if (subStringValue.Trim().EndsWith("&&"))
        //        {
        //            subStringValue = subStringValue.Substring(0, subStringValue.LastIndexOf('&') - 1);
        //        }
        //        if (subStringValue.Trim().Contains("&& &&"))
        //        {
        //            subStringValue = subStringValue.Replace("&& &&", "&&");
        //        }
        //        return subStringValue;
        //    }
        //    else
        //    {
        //        if (strSymbol.Contains(')'))
        //        {
        //            if (strSymbol[strSymbol.LastIndexOf(')') - 1].Equals(')'))
        //            {
        //                strSymbol = strSymbol.Substring(0, strSymbol.Length - 1);
        //                closeBracket = true;
        //            }
        //        }
        //        strSymbol = strSymbol.Replace(strSymbol, closeBracket ? string.Concat(strSymbol, ">", 0, ')') : string.Concat(strSymbol, ">", 0));
        //        closeBracket = false;
        //        return strSymbol;
        //    }
        //}



        /// <summary>
        /// 
        /// </summary>
        private void ConvertAvsFiletoJvs()
        {
            bool IsSuccess = false;
            InputProcessor inputProcessor2 = new InputProcessor();
            string emptyField = "**Missing Information :  ";
            if (string.IsNullOrEmpty(AVSPath) || string.IsNullOrEmpty(JVSPath))
            {
                if (string.IsNullOrEmpty(AVSPath))
                {
                    emptyField = emptyField + "AVS Script Path ";
                }
                if (string.IsNullOrEmpty(AVSPath) && string.IsNullOrEmpty(JVSPath))
                {
                    emptyField = emptyField + "&";
                }

                if (string.IsNullOrEmpty(JVSPath))
                {
                    emptyField = emptyField + " JVS Output Path ";
                }

                ValidateError.Foreground = new SolidColorBrush(Colors.Red);

                ValidateError.Content = emptyField;
                ErrorMessage = emptyField;
                Status = IsSuccess;
                // ValidateError.Opacity = 1;
            }
            else
            {
                if (string.IsNullOrEmpty(LibraryPath))
                {
                    IsSuccess = inputProcessor2.Process(AVSPath, AVSPath, JVSPath);
                }
                else
                {
                    IsSuccess = inputProcessor2.Process(AVSPath, LibraryPath, JVSPath);
                }
                if (IsSuccess)
                {
                    ValidateError.Foreground = new SolidColorBrush(Colors.GreenYellow);
                    ValidateError.Content = "*Conversion done sucessfully";
                    ErrorMessage = "*Conversion done sucessfully";
                }
                else
                {
                    ValidateError.Content = "*Conversion failed,Please contact to administrator";
                    ErrorMessage = "*Conversion failed,Please contact to administrator";
                }
                Status = IsSuccess;
                AVSPath = string.Empty;
                LibraryPath = string.Empty;
                JVSPath = string.Empty;

            }
        }


        #endregion


    }
}

