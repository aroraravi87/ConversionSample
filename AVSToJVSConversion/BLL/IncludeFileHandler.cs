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
        private readonly Operations _operations;
        
        public IncludeFileHandler()
        {
            _operations = new Operations();
        }

        public void ManageIncludes(string includeList, string libraryPath, DataTable dtForMethodsAvailable)
        {
            try
            {
                DataTable dtForFileOfInclude;
                DataTable dtForLiterals;
                string[] includeFileNames;
                string fileName;
                string includeListOfIncludeFile;

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
                            ManageIncludes(includeListOfIncludeFile, libraryPath, dtForMethodsAvailable);
                        }
                        _operations.RemoveIncludeListStatement(dtForFileOfInclude);
                        _operations.ConvertInitialization(dtForFileOfInclude, null, false);
                        _operations.GetMethodsListInFile(dtForFileOfInclude, fileName, dtForMethodsAvailable);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
