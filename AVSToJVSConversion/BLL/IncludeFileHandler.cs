
/*****************************************************************************
File Name:              (IncludeFileHandler2.mls)
+------+----------+------------+-----------------------------------------------------------------------+
| S.No.| Date     | Who        | Description                                                            |
+------+----------+------------+-----------------------------------------------------------------------+
|      | 10 Jun   | Ajey Raghav| Initial version                                                       |
+------+----------+------------+-----------------------------------------------------------------------+

Description:       

*****************************************************************************/

namespace AVSToJVSConversion.BLL
{
    using System;
    using System.Data;
    using System.IO;

    class IncludeFileHandler
    {
        #region === [Variable and Constructor] ====================

        private readonly Operations _operations;
        private readonly Utility _utility;

        public IncludeFileHandler()
        {
            _operations = new Operations();
            _utility = new Utility();
        } 
        #endregion

        #region ==[Public Members]====================================

        public void ManageIncludes(string includeList, string libraryPath, DataTable dtForMethodsAvailable, DataTable dtForVariables)
        {
            try
            {
                DataTable dtForFileOfInclude;
                DataTable dtForLiterals;
                string[] includeFileNames;
                string fileName;
                string includeListOfIncludeFile;
                int containsGlobalTable = 0;

                if (includeList.Contains(","))
                {

                    includeFileNames = includeList.Split(',');
                }
                else
                {
                    includeFileNames = new String[1];
                    includeFileNames[0] = includeList;
                }

                for (int i = 0; i < includeFileNames.Length; i++)
                {
                    containsGlobalTable = 0;
                    foreach (string file in Directory.EnumerateFiles(libraryPath, includeFileNames[i] + ".mls"))
                    {
                        fileName = file.Replace(libraryPath + "\\", "");

                        dtForFileOfInclude = _operations.GetDataTableForFile(file);
                        _operations.RemoveCommentsFromDtForFile(dtForFileOfInclude);
                        dtForLiterals = _operations.RemoveLiterals(dtForFileOfInclude, false);
                        dtForFileOfInclude = _operations.ConvertMultipleLinesToSingle(dtForFileOfInclude);
                        _operations.RemoveMethodPrototype(dtForFileOfInclude);
                        includeListOfIncludeFile = _operations.GetIncludeList(dtForFileOfInclude, dtForLiterals);
                        if (!includeListOfIncludeFile.Equals(""))
                        {
                            ManageIncludes(includeListOfIncludeFile, libraryPath, dtForMethodsAvailable, dtForVariables);
                        }
                        _operations.RemoveIncludeListStatement(dtForFileOfInclude);
                        _operations.ConvertInitialization(dtForFileOfInclude, null, false);
                        if (_utility.CheckGlobalTable(dtForFileOfInclude, "argt") ||
                            _utility.CheckGlobalTable(dtForFileOfInclude, "returnt"))
                        {
                            containsGlobalTable = 1;
                        }
                        _operations.GetMethodsListInFile(dtForFileOfInclude, fileName, dtForMethodsAvailable, containsGlobalTable, dtForVariables);

                        _utility.destroyDT(dtForFileOfInclude);
                        _utility.destroyDT(dtForLiterals);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 
        #endregion
    }
}
