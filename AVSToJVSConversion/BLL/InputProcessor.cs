using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using AVSToJVSConversion.ViewModel;
using log4net;

/*****************************************************************************
File Name:              (InputProcessor.mls)
+------+----------+------------+-----------------------------------------------------------------------+
| S.No.| Date     | Who        | Description                                                            |
+------+----------+------------+-----------------------------------------------------------------------+
|      | 10 Jun   | Ajey Raghav| Initial version                                                       |
+------+----------+------------+-----------------------------------------------------------------------+

Description:       

*****************************************************************************/

namespace AVSToJVSConversion.BLL
{
    internal class InputProcessor
    {
        public static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Operations _operations = null;
        private InitializeTables _initializeTables = null;
        private IncludeFileHandler _includeFileHandler = null;
        private Utility _utility = null;
        private string _fileName;
        private string _includeList;
        private MainWindowViewModel objMainWindowViewModel { get; set; }
        private DataTable _dtForFile = null;
        private DataTable _dtForLiterals = null;
        private DataTable _dtForMethodsAvailable = null;
        private DataTable _dtForInclude = null;
        private DataTable _dtForVariables = null;

        public InputProcessor()
        {
            _operations = new Operations();
            _initializeTables = new InitializeTables();
            _includeFileHandler = new IncludeFileHandler();
            _utility = new Utility();
            objMainWindowViewModel = new MainWindowViewModel();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="avsScriptPath"></param>
        /// <param name="avsLibraryPath"></param>
        /// <param name="outputPath"></param>
        public void Process(string avsScriptPath, string avsLibraryPath, string outputPath, string fileName)
        {

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    bool mainNotExist;
                    bool t = false;
                    int counter = 1;
                    int containsGlobalTable = 0;


                    _dtForMethodsAvailable = _initializeTables.GetDtForMethodPresent();
                    _fileName = fileName.Replace(avsScriptPath + "\\", "");
                    Log.Info(string.Format("FileName:{0},Status {1}", _fileName, "Processing Start.."));

                    /*if (file.Contains("CEI_RTP_PwrDayAhead"))
                    {
                        t = true;
                    }
                    if(!t)
                        continue;*/

                    // if (!file.Contains("FAS49_PM_Pfolio_FX"))
                    //   continue;

                    _dtForFile = _operations.GetDataTableForFile(fileName);
                    _operations.RemoveCommentsFromDtForFile(_dtForFile);
                    _operations.SeperateComments(_dtForFile);
                    _dtForLiterals = _operations.RemoveLiterals(_dtForFile, false);

                    _operations.CheckIfMainExist(_dtForFile, out mainNotExist);
                    if (!mainNotExist)
                    {
                        _operations.HandleScriptIfMainNotExist(_dtForFile); // Method for Credit Rating Script
                    }

                    _dtForFile = _operations.ConvertMultipleLinesToSingle(_dtForFile);
                    _operations.RemoveMethodPrototype(_dtForFile);
                    _includeList = _operations.GetIncludeList(_dtForFile, _dtForLiterals);
                    if (!_includeList.Equals(""))
                    {
                        _includeFileHandler.ManageIncludes(_includeList, avsLibraryPath, _dtForMethodsAvailable);
                    }
                    _operations.RemoveIncludeListStatement(_dtForFile);
                    _dtForInclude = _initializeTables.GetDtForInclude();
                    _operations.ConvertInitialization(_dtForFile, _dtForInclude, true);
                    _operations.InitializeVariables(_dtForFile, _dtForInclude);
                    if (_utility.CheckGlobalTable(_dtForFile, "argt") ||
                        _utility.CheckGlobalTable(_dtForFile, "returnt"))
                    {
                        containsGlobalTable = 1;
                    }
                    _operations.GetMethodsListInFile(_dtForFile, _fileName, _dtForMethodsAvailable, containsGlobalTable);
                    _dtForVariables = _operations.GetVariableList(_dtForFile, _fileName);
                    _operations.ConvertStaticMethod(_dtForFile, _dtForMethodsAvailable, _dtForInclude, _fileName);
                    _operations.ConvertNonStaticMethod(_dtForFile, _dtForMethodsAvailable, _dtForInclude, _fileName);
                    _operations.ConvertConditionalStatements(_dtForFile, "if");
                    _operations.ConvertConditionalStatements(_dtForFile, "while");
                    _operations.ReplaceTrueFalse(_dtForFile);
                    _operations.ConvertForStatements(_dtForFile); // Initliazed for loop variable
                    _operations.ConvertEnums(_dtForFile, _dtForVariables, _fileName, _dtForInclude);
                    _operations.AddPublicStaticInLibraryMethods(_dtForFile);
                    _operations.AddThrowsExceptionInLibraryMethods(_dtForFile);
                    _operations.GenerateOutputFile(_dtForFile, _dtForInclude, _dtForLiterals, outputPath, _fileName, _dtForMethodsAvailable);
                    Log.Info(string.Format("FileName:{0},Status {1}", _fileName, "Completed Successfully"));




                }

                catch (Exception ex)
                {
                    GenerateFailedScripts(ex);
                }
                finally
                {
                    _utility.destroyDT(_dtForFile);
                    _utility.destroyDT(_dtForInclude);
                    _utility.destroyDT(_dtForLiterals);
                    _utility.destroyDT(_dtForMethodsAvailable);
                    _utility.destroyDT(_dtForVariables);
                }
            }
            // return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        private void GenerateFailedScripts(Exception ex)
        {
            if (!Directory.Exists(Path.Combine(ConfigurationManager.AppSettings["basePath"], "FailedScripts")))
            {
                Directory.CreateDirectory(Path.Combine(ConfigurationManager.AppSettings["basePath"],
                    "FailedScripts"));
            }
            if (File.Exists(Path.Combine(ConfigurationManager.AppSettings["basePath"],
                  string.Format("{0}\\{1}", "FailedScripts", _fileName))))
            {
                File.Delete(Path.Combine(ConfigurationManager.AppSettings["basePath"],
                    string.Format("{0}\\{1}", "FailedScripts", _fileName)));
            }
            using (StreamWriter sw = File.CreateText(Path.Combine(ConfigurationManager.AppSettings["basePath"],
                string.Format("{0}\\{1}", "FailedScripts", _fileName))))
            {
                sw.WriteLine("FileName:{0},Exception Message {1},Trace Info {2}", _fileName, ex.Message,
                    ex.StackTrace);
            }
        }
    }
}
