using System;
using System.IO;
using System.Data;
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
        string _fileName;
        string _includeList;

        DataTable _dtForFile = null;
        DataTable _dtForLiterals = null;
        DataTable _dtForMethodsAvailable = null;
        DataTable _dtForInclude = null;
        DataTable _dtForVariables = null;

        public InputProcessor()
        {

            _operations = new Operations();
            _initializeTables = new InitializeTables();
            _includeFileHandler = new IncludeFileHandler();
            _utility = new Utility();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="avsScriptPath"></param>
        /// <param name="avsLibraryPath"></param>
        /// <param name="outputPath"></param>
        public bool Process(string avsScriptPath, string avsLibraryPath, string outputPath)
        {
            if (!string.IsNullOrWhiteSpace(avsScriptPath))
            {
                try
                {
                    bool mainNotExist;
                        
                    foreach (string file in Directory.EnumerateFiles(avsScriptPath, "*.mls"))
                    {
                        _fileName = file.Replace(avsScriptPath + "\\", "");
                        //if (_fileName.Contains("CIT003_OASYS_TaxEventsByDay_s"))
                        //{
                        //    t = false;
                        //}
                        //if (t)
                        //{
                        //    continue;
                        //}
                        _dtForFile = _operations.GetDataTableForFile(file);
                        _operations.RemoveCommentsFromDtForFile(_dtForFile);
                        _operations.SeperateComments(_dtForFile);
                        _dtForLiterals = _operations.RemoveLiterals(_dtForFile, false);
                       
                        _operations.CheckIfMainExist(_dtForFile,out mainNotExist);
                        if (!mainNotExist)
                        {
                            _operations.HandleScriptIfMainNotExist(_dtForFile); // Method for Credit Rating Script
                        }

                        _dtForFile = _operations.ConvertMultipleLinesToSingle(_dtForFile);
                        _operations.RemoveMethodPrototype(_dtForFile);
                        _includeList = _operations.GetIncludeList(_dtForFile, _dtForLiterals);
                        if (!_includeList.Equals(""))
                        {
                            _dtForMethodsAvailable = _includeFileHandler.ManageIncludes(_includeList, avsLibraryPath);
                        }
                        _operations.RemoveIncludeListStatement(_dtForFile);
                        _dtForInclude = _initializeTables.GetDtForInclude();
                        _operations.ConvertInitialization(_dtForFile, _dtForInclude, true);
                         _operations.InitlializedVariables(_dtForFile);
                        if (_dtForMethodsAvailable == null)
                        {
                            _dtForMethodsAvailable = _initializeTables.GetDtForMethodPresent();
                        }

                       

                        _operations.GetMethodsListInFile(_dtForFile, _fileName, _dtForMethodsAvailable);
                        _dtForVariables = _operations.GetVariableList(_dtForFile, _fileName);
                        _operations.ConvertStaticMethod(_dtForFile, _dtForMethodsAvailable, _dtForInclude, _fileName);
                        _operations.ConvertNonStaticMethod(_dtForFile, _dtForMethodsAvailable, _dtForInclude, _fileName);
                        _operations.ConvertIfStatements(_dtForFile);
                        _operations.ReplaceTrueFalse(_dtForFile);

                        _operations.ConvertForStatements(_dtForFile); // Initliazed for loop variable

                        _operations.ValidateWhileStatement(_dtForFile);
                        _operations.ReplaceWhileIntoBoolStatement(_dtForFile);
                        _operations.ConvertEnums(_dtForFile, _dtForVariables, _fileName, _dtForInclude);
                        _operations.AddPublicStaticInLibraryMethods(_dtForFile);
                        _operations.AddThrowsExceptionInLibraryMethods(_dtForFile);
                        _operations.GenerateOutputFile(_dtForFile, _dtForInclude, _dtForLiterals, outputPath, _fileName);

                        _dtForFile.Dispose();
                        _dtForInclude.Dispose();
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
                catch (Exception ex)
                {
                    _utility.GetCustomLogAppender();
                    Log.Error(string.Format("FileName:{0},Exception Message {1},Trace Info {2} ",_fileName,ex.Message,ex.StackTrace));
                    return false;
                }
            }
            return true;
        }
    }
}
