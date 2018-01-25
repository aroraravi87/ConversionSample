using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
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
    class InputProcessor
    {
        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Operations _operations = null;
        private InitializeTables _initializeTables = null;
        private IncludeFileHandler _includeFileHandler = null;
        private Utility _utility = null;
        private MainWindowViewModel _mainWindowViewModel = null;
        string _fileName;
        string _includeList;

        DataTable _dtForFile = null;
        DataTable _dtForLiterals = null;
        DataTable _dtForMethodsAvailable = null;
        DataTable _dtForInclude = null;
        DataTable _dtForVariables = null;
        DataTable _dtForvariables;

        public InputProcessor()
        {

            _operations = new Operations();
            _initializeTables = new InitializeTables();
            _includeFileHandler = new IncludeFileHandler();
            _utility = new Utility();
            _mainWindowViewModel = new MainWindowViewModel();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="avsScriptPath"></param>
        /// <param name="avsLibraryPath"></param>
        /// <param name="outputPath"></param>
        public bool Process(string avsScriptPath, string avsLibraryPath, string outputPath)
        {
            _utility.GetCustomLogAppender();

            if (!string.IsNullOrWhiteSpace(avsScriptPath))
            {
                try
                {
                    bool mainNotExist;
                    bool t = false;
                    int counter = 0;
                    foreach (string file in Directory.EnumerateFiles(avsScriptPath, "*.mls"))
                    {
                        StartProgressPanel(Directory.EnumerateFiles(avsScriptPath, "*.mls"), counter++);

                        _dtForMethodsAvailable = _initializeTables.GetDtForMethodPresent();
                        _fileName = file.Replace(avsScriptPath + "\\", "");
                        Log.Info(string.Format("FileName:{0},Status {1}", _fileName, "Processing Start.."));

                        /*if (file.Contains("test"))
                        {
                            t = true;
                        }
                        if(!t)
                            continue;*/

                        // if (!file.Contains("OC_PartyAgreement_Retrieve"))
                        //    continue;

                        _dtForFile = _operations.GetDataTableForFile(file);
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
                        _operations.InitlializeVariables(_dtForFile, _dtForInclude);
                        _operations.GetMethodsListInFile(_dtForFile, _fileName, _dtForMethodsAvailable);
                        _dtForVariables = _operations.GetVariableList(_dtForFile, _fileName);
                        _operations.ConvertStaticMethod(_dtForFile, _dtForMethodsAvailable, _dtForInclude, _fileName);
                        _operations.ConvertNonStaticMethod(_dtForFile, _dtForMethodsAvailable, _dtForInclude, _fileName);
                        _operations.ConvertConditionalStatements(_dtForFile, "if");
                        _operations.ConvertConditionalStatements(_dtForFile, "while");
                        _operations.ReplaceTrueFalse(_dtForFile);
                        _operations.ConvertForStatements(_dtForFile); // Initliazed for loop variable
                        // _operations.ValidateWhileStatement(_dtForFile);
                        // _operations.ReplaceWhileIntoBoolStatement(_dtForFile);
                        _operations.ConvertEnums(_dtForFile, _dtForVariables, _fileName, _dtForInclude);
                        _operations.AddPublicStaticInLibraryMethods(_dtForFile);
                        _operations.AddThrowsExceptionInLibraryMethods(_dtForFile);
                        _operations.GenerateOutputFile(_dtForFile, _dtForInclude, _dtForLiterals, outputPath, _fileName);
                        Log.Info(string.Format("FileName:{0},Status {1}", _fileName, "Completed Successfully"));
                    }
                }
                catch (Exception ex)
                {

                    Log.Error(string.Format("FileName:{0},Exception Message {1},Trace Info {2} ", _fileName, ex.Message,
                        ex.StackTrace));
                    return false;
                }
                finally
                {

                    if (_dtForFile != null)
                        _dtForFile.Dispose();
                    if (_dtForInclude != null)
                        _dtForInclude.Dispose();
                    if (_dtForVariables != null)
                        _dtForVariables.Dispose();
                    if (_dtForMethodsAvailable != null)
                    {
                        _dtForMethodsAvailable.Dispose();
                    }
                    if (_dtForLiterals != null)
                    {
                        _dtForLiterals.Dispose();
                    }
                    _dtForFile = null;
                    _dtForInclude = null;
                    _dtForVariables = null;
                    _dtForMethodsAvailable = null;
                    _dtForLiterals = null;
                }
            }
            return true;
        }

        private void StartProgressPanel(IEnumerable<string> strFiles, int countExecute)
        {
            int fileCounts = strFiles.Count();
            //int mode = fileCounts/100;
            //if (!countExecute.Equals(mode))
            //{
            //    false
            //}
            _mainWindowViewModel.ProgressStatus = (fileCounts - countExecute) / 100;


        }
    }
}
