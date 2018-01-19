using System;
using System.Data;
using System.IO;

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
    class IncludeFileHandler
    {
        private Operations _operations = null;
        private InitializeTables _initializeTables = null;

        public IncludeFileHandler()
        {
            _operations = new Operations();
            _initializeTables = new InitializeTables();
        }

        public DataTable ManageIncludes(string includeList, string libraryPath)
        {

            try
            {
                DataTable dtForFileOfInclude;
                DataTable dtForMethodsAvailable = null;
                string[] includeFileNames;
                string fileName;


                if (includeList.Contains(","))
                {

                    includeFileNames = includeList.Split(',');
                }
                else
                {
                    includeFileNames = new String[1];
                    includeFileNames[0] = includeList;
                }

                dtForMethodsAvailable = _initializeTables.GetDtForMethodPresent();

                for (int i = 0; i < includeFileNames.Length; i++)
                {
                    foreach (string file in Directory.EnumerateFiles(libraryPath, includeFileNames[i] + ".mls"))
                    {
                        fileName = file.Replace(libraryPath + "\\", "");
                        dtForFileOfInclude = _operations.GetDataTableForFile(file);
                        _operations.RemoveCommentsFromDtForFile(dtForFileOfInclude);
                        _operations.RemoveLiterals(dtForFileOfInclude, true);
                        dtForFileOfInclude = _operations.ConvertMultipleLinesToSingle(dtForFileOfInclude);
                        _operations.RemoveIncludeListStatement(dtForFileOfInclude);
                        _operations.RemoveMethodPrototype(dtForFileOfInclude);
                        _operations.RemoveIncludeListStatement(dtForFileOfInclude);
                        _operations.ConvertInitialization(dtForFileOfInclude, null, false);
                        _operations.GetMethodsListInFile(dtForFileOfInclude, fileName, dtForMethodsAvailable);
                    }
                }
                return dtForMethodsAvailable;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
    }
}
