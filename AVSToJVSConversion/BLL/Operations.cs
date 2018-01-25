using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System;

/*****************************************************************************
File Name:              (AVS_To_JVS.mls)
+------+----------+------------+-----------------------------------------------------------------------+
| S.No.| Date     | Who        | Description                                                            |
+------+----------+------------+-----------------------------------------------------------------------+
|      | 10 Jun   | Ajey Raghav| Initial version                                                       |
+------+----------+------------+-----------------------------------------------------------------------+

Description:       

*****************************************************************************/
using System.Windows.Shapes;
using AVSToJVSConversion.Common;
using AVSToJVSConversion.DLL;

namespace AVSToJVSConversion.BLL
{
    internal class Operations
    {
        private SqlConnection _conn = null;
        private SqlDataReader _dataReader = null;
        private string _query = string.Empty;
        private InitializeTables _initializeTables = null;
        private Utility _utility = null;
        private Dictionary<string, string> _getVariableDictionary = null;
        private static string PrevElementType = string.Empty;
        private Regex _objRegex = null;


        public class DataTypes
        {
            public string TypeValue { get; set; }
            public string TypeName { get; set; }
        }

        public Operations()
        {
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





        public DataTable GetDataTableForFile(string file)
        {
            string[] contents;
            DataTable dtForFile;
            DataRow dr;

            dtForFile = _initializeTables.GetDtForFile();
            contents = File.ReadAllLines(file);
            foreach (string content in contents)
            {
                dr = dtForFile.Rows.Add();
                dr[0] = content;
            }
            return dtForFile;
        }

        public void RemoveCommentsFromDtForFile(DataTable dtForFile)
        {
            string line;
            char[] lineArray;

            bool singleLineCommentFlag = false;
            bool multiLineCommentFlag = false;
            bool slashFoundFlag = false;
            bool starFoundFlag = false;
            bool stringFoundFlag = false;
            bool escapeCharFoundFlag = false;
            bool charFoundFlag = false;

            foreach (DataRow dr in dtForFile.Rows)
            {
                lineArray = dr[0].ToString().ToCharArray();
                singleLineCommentFlag = false;
                starFoundFlag = false;
                line = "";

                foreach (char character in lineArray)
                {
                    if (charFoundFlag || stringFoundFlag)
                    {
                        line = line + character;
                    }

                    if (singleLineCommentFlag)
                    {
                        continue;
                    }

                    if (starFoundFlag)
                    {
                        starFoundFlag = false;
                        if (character == '/')
                        {
                            multiLineCommentFlag = false;
                            continue;
                        }
                    }
                    if (multiLineCommentFlag && character != '*')
                    {
                        continue;
                    }
                    if (multiLineCommentFlag && character == '*')
                    {
                        starFoundFlag = true;
                        continue;
                    }

                    if (slashFoundFlag)
                    {
                        slashFoundFlag = false;
                        if (character == '/')
                        {
                            singleLineCommentFlag = true;
                            continue;
                        }
                        else if (character == '*')
                        {
                            multiLineCommentFlag = true;
                            continue;
                        }
                        line = line + '/';
                    }

                    if (escapeCharFoundFlag)
                    {
                        escapeCharFoundFlag = false;
                        continue;
                    }

                    if (stringFoundFlag && character == '\\')
                    {
                        escapeCharFoundFlag = true;
                        continue;
                    }
                    if (stringFoundFlag && character == '"')
                    {
                        stringFoundFlag = false;
                        continue;
                    }

                    if (charFoundFlag && character == '\\')
                    {
                        escapeCharFoundFlag = true;
                        continue;
                    }
                    if (charFoundFlag && character == '\'')
                    {
                        charFoundFlag = false;
                        continue;
                    }

                    if (charFoundFlag || stringFoundFlag)
                    {
                        continue;
                    }

                    if (character == '/' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        slashFoundFlag = true;
                        continue;
                    }
                    if (character == '"' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        line = line + character;
                        stringFoundFlag = true;
                        continue;
                    }
                    if (character == '\'' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        line = line + character;
                        charFoundFlag = true;
                        continue;
                    }
                    line = line + character;
                }
                dr[1] = line;
            }
        }

        public void SeperateComments(DataTable dtForFile)
        {
            char[] lineArray;

            string firstPart = "";
            string SecondPart = "";
            string line;

            bool singleLineCommentFlag = false;
            bool multiLineCommentFlag = false;
            bool slashFoundFlag = false;
            bool starFoundFlag = false;
            bool stringFoundFlag = false;
            bool escapeCharFoundFlag = false;
            bool charFoundFlag = false;
            bool newLineStartWithCommentFlag = false;

            foreach (DataRow drs in dtForFile.Rows)
            {
                lineArray = drs[0].ToString().ToCharArray();
                singleLineCommentFlag = false;
                starFoundFlag = false;
                line = drs[0].ToString();
                firstPart = "";
                SecondPart = "";

                foreach (char character in lineArray)
                {
                    if (newLineStartWithCommentFlag)
                    {
                        firstPart = firstPart + character;
                        if (starFoundFlag)
                        {
                            starFoundFlag = false;
                            if (character == '/')
                            {
                                multiLineCommentFlag = false;
                                newLineStartWithCommentFlag = false;
                                continue;
                            }
                        }

                        if (character == '*')
                        {
                            starFoundFlag = true;
                            continue;
                        }
                        continue;
                    }

                    if (singleLineCommentFlag)
                    {
                        SecondPart = SecondPart + character;
                        continue;
                    }

                    if (multiLineCommentFlag)
                    {
                        SecondPart = SecondPart + character;
                    }

                    if (starFoundFlag)
                    {
                        starFoundFlag = false;
                        if (character == '/')
                        {
                            multiLineCommentFlag = false;
                            continue;
                        }
                    }
                    if (multiLineCommentFlag && character != '*')
                    {
                        continue;
                    }
                    if (multiLineCommentFlag && character == '*')
                    {
                        starFoundFlag = true;
                        continue;
                    }

                    if (slashFoundFlag)
                    {
                        slashFoundFlag = false;
                        if (character == '/')
                        {
                            SecondPart = "//";
                            singleLineCommentFlag = true;
                            continue;
                        }
                        else if (character == '*')
                        {
                            SecondPart = "/*";
                            multiLineCommentFlag = true;
                            continue;
                        }
                    }

                    if (escapeCharFoundFlag)
                    {
                        escapeCharFoundFlag = false;
                        continue;
                    }

                    if (stringFoundFlag && character == '\\')
                    {
                        escapeCharFoundFlag = true;
                        continue;
                    }
                    if (stringFoundFlag && character == '"')
                    {
                        stringFoundFlag = false;
                        continue;
                    }

                    if (charFoundFlag && character == '\\')
                    {
                        escapeCharFoundFlag = true;
                        continue;
                    }
                    if (charFoundFlag && character == '\'')
                    {
                        charFoundFlag = false;
                        continue;
                    }

                    if (charFoundFlag || stringFoundFlag)
                    {
                        continue;
                    }

                    if (character == '/' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        slashFoundFlag = true;
                        continue;
                    }
                    if (character == '"' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        stringFoundFlag = true;
                        continue;
                    }
                    if (character == '\'' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        charFoundFlag = true;
                        continue;
                    }
                }
                newLineStartWithCommentFlag = multiLineCommentFlag;
                drs[2] = firstPart;
                drs[3] = SecondPart;
            }
        }

        public DataTable RemoveLiterals(DataTable dtForFile, bool isInclude)
        {
            DataRow dr;
            DataTable dtForLiterals;
            dtForLiterals = _initializeTables.GetDtForLiterals();

            string line;
            string literal = "";
            char[] lineArray;
            int number = 0;

            bool singleLineCommentFlag = false;
            bool multiLineCommentFlag = false;
            bool slashFoundFlag = false;
            bool starFoundFlag = false;
            bool stringFoundFlag = false;
            bool escapeCharFoundFlag = false;
            bool charFoundFlag = false;

            foreach (DataRow drs in dtForFile.Rows)
            {
                lineArray = drs[1].ToString().ToCharArray();
                singleLineCommentFlag = false;
                starFoundFlag = false;
                literal = "";
                line = "";

                foreach (char character in lineArray)
                {
                    if (singleLineCommentFlag)
                    {
                        continue;
                    }
                    if (starFoundFlag)
                    {
                        starFoundFlag = false;
                        if (character == '/')
                        {
                            multiLineCommentFlag = false;
                            continue;
                        }
                    }
                    if (multiLineCommentFlag && character != '*')
                    {
                        continue;
                    }
                    if (multiLineCommentFlag && character == '*')
                    {
                        starFoundFlag = true;
                        continue;
                    }

                    if (slashFoundFlag)
                    {
                        slashFoundFlag = false;
                        if (character == '/')
                        {
                            singleLineCommentFlag = true;
                            continue;
                        }
                        else if (character == '*')
                        {
                            multiLineCommentFlag = true;
                            continue;
                        }
                        line = line + '/';
                    }

                    if (charFoundFlag || stringFoundFlag)
                    {
                        literal = literal + character.ToString();
                    }

                    if (escapeCharFoundFlag)
                    {
                        escapeCharFoundFlag = false;
                        continue;
                    }

                    if (stringFoundFlag && character == '\\')
                    {
                        escapeCharFoundFlag = true;
                        continue;
                    }
                    if (stringFoundFlag && character == '"')
                    {
                        line = line + "StringLiteralDetected" + number;
                        if (!isInclude)
                        {
                            dr = dtForLiterals.Rows.Add();
                            dr[0] = "StringLiteralDetected" + number;
                            dr[1] = literal;
                        }
                        number++;
                        literal = "";
                        stringFoundFlag = false;
                        continue;
                    }

                    if (charFoundFlag && character == '\\')
                    {
                        escapeCharFoundFlag = true;
                        continue;
                    }
                    if (charFoundFlag && character == '\'')
                    {
                        line = line + "StringLiteralDetected" + number;
                        if (!isInclude)
                        {
                            dr = dtForLiterals.Rows.Add();
                            dr[0] = "StringLiteralDetected" + number;
                            dr[1] = literal;
                        }
                        number++;
                        literal = "";
                        charFoundFlag = false;
                        continue;
                    }

                    if (charFoundFlag || stringFoundFlag)
                    {
                        continue;
                    }

                    if (character == '/' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        slashFoundFlag = true;
                        continue;
                    }
                    if (character == '"' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        literal = character.ToString();
                        stringFoundFlag = true;
                        continue;
                    }
                    if (character == '\'' && !singleLineCommentFlag && !multiLineCommentFlag && !slashFoundFlag &&
                        !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                    {
                        literal = character.ToString();
                        charFoundFlag = true;
                        continue;
                    }
                    line = line + character;
                }
                drs[1] = line;
            }
            return dtForLiterals;
        }

        public void CheckIfMainExist(DataTable dtForFile, out bool isMainExist)
        {
            char[] lineArray;
            isMainExist = true;
            foreach (DataRow drs in dtForFile.Rows)
            {
                lineArray = drs[1].ToString().Trim().ToCharArray();

                foreach (char character in lineArray)
                {
                    if (character == '{')
                    {
                        isMainExist = false;
                        drs[1] = "void main()" + drs[1];
                        return;
                    }
                    else
                    {
                        isMainExist = true;
                        return;
                    }
                }
            }

        }

        public string GetIncludeList(DataTable dtForFile, DataTable dtForLiterals)
        {
            string includeFileName;
            string includeList = "";
            string line;

            foreach (DataRow drs in dtForFile.Rows)
            {
                line = drs[1].ToString().Trim();
                if (line.Contains("#INCLUDE_"))
                {
                    drs[1] = "";
                    line = "";
                }
                if (line.Contains("#include "))
                {
                    line = line.Replace("#include ", "").Trim();
                    includeFileName = line.Replace(";", "").Trim();
                    foreach (DataRow dr in dtForLiterals.Rows)
                    {
                        if (includeFileName.Equals(dr[0].ToString()))
                        {
                            includeFileName = dr[1].ToString().Replace("\"", "");
                            break;
                        }
                    }

                    if (includeList.Equals(""))
                    {
                        includeList = includeFileName;
                    }
                    else
                    {
                        includeList = includeList + "," + includeFileName;
                    }
                }
            }
            return includeList;
        }

        public void RemoveIncludeListStatement(DataTable dtForFile)
        {
            string line;
            foreach (DataRow drs in dtForFile.Rows)
            {
                line = drs[1].ToString().Trim();
                if (line.Contains("#INCLUDE_") || line.Contains("#include ") || line.Contains("#sdbg ") ||
                    line.Contains("#SDBG "))
                {
                    drs[1] = "";
                }
            }
        }

        public DataTable ConvertMultipleLinesToSingle(DataTable dtForFileOfInclude)
        {
            DataTable dtForFileTemp;
            DataTable dtForFile;
            DataRow dr;

            dtForFileTemp = _initializeTables.GetDtForFile();
            dtForFile = _initializeTables.GetDtForFile();

            string line = "";
            string completeLine = "";
            string commentFirstPart = "";
            string commentSecondPart = "";

            foreach (DataRow drs in dtForFileOfInclude.Rows)
            {
                completeLine = completeLine + drs[0].ToString();
                line = line + " " + drs[1].ToString().Trim();
                commentFirstPart = commentFirstPart + drs[2].ToString();
                commentSecondPart = commentSecondPart + drs[3].ToString();

                if (line.Trim().Equals(""))
                {
                    dr = dtForFileTemp.Rows.Add();
                    dr[0] = completeLine;
                    dr[1] = line.Trim();
                    dr[2] = commentFirstPart;
                    dr[3] = commentSecondPart;
                    completeLine = "";
                    commentFirstPart = "";
                    commentSecondPart = "";
                    line = "";
                    continue;
                }

                if (line.Count(x => x == '(') == line.Count(x => x == ')'))
                {
                    dr = dtForFileTemp.Rows.Add();
                    dr[0] = completeLine;
                    dr[1] = line.Trim();
                    dr[2] = commentFirstPart;
                    dr[3] = commentSecondPart;
                    completeLine = "";
                    commentFirstPart = "";
                    commentSecondPart = "";
                    line = "";
                    continue;
                }
            }
            dtForFileOfInclude.Dispose();
            line = "";
            completeLine = "";
            commentFirstPart = "";
            commentSecondPart = "";

            foreach (DataRow drs in dtForFileTemp.Rows)
            {
                completeLine = completeLine + drs[0].ToString();
                line = line + " " + drs[1].ToString().Trim();
                commentFirstPart = commentFirstPart + drs[2].ToString();
                commentSecondPart = commentSecondPart + drs[3].ToString();

                if (line.Trim().Equals(""))
                {
                    dr = dtForFile.Rows.Add();
                    dr[0] = completeLine;
                    dr[1] = line.Trim();
                    dr[2] = commentFirstPart;
                    dr[3] = commentSecondPart;
                    completeLine = "";
                    commentFirstPart = "";
                    commentSecondPart = "";
                    continue;
                }

                if (line.Contains("(") || line.Contains("{") || line.Contains("}") || line.Contains(";") ||
                    line.Contains(":") || line.Contains("#"))
                {
                    if (_utility.CheckVariableDeclaration(line))
                    {
                        if (_utility.GetPositionOfVariableTypeInLine(line) > line.LastIndexOf(";"))
                        {
                            if (_utility.GetCount(line, "(") > 0 && line.Contains("="))
                            {
                                continue;
                            }
                        }
                    }

                    dr = dtForFile.Rows.Add();
                    dr[0] = completeLine;
                    dr[1] = line.Trim();
                    dr[2] = commentFirstPart;
                    dr[3] = commentSecondPart;
                    completeLine = "";
                    commentFirstPart = "";
                    commentSecondPart = "";
                    line = "";
                    continue;
                }
            }
            dtForFileTemp.Dispose();
            return dtForFile;
        }

        public void RemoveMethodPrototype(DataTable dtForFile)
        {
            bool methodStartFlag = false;
            bool methodNamefoundFlag = false;

            int openCurlyBraces = 0;

            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }

                    if (!methodNamefoundFlag && !methodStartFlag &&
                        _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                    {
                        methodNamefoundFlag = true;
                    }
                    if (methodNamefoundFlag)
                    {
                        if (_utility.DetectMethodStartPointer(drs[1].ToString()))
                        {
                            methodStartFlag = true;
                            methodNamefoundFlag = false;
                        }
                        else if (_utility.DetectMethodPrototype(drs[1].ToString()))
                        {
                            if (!drs[1].ToString().Contains("=") && !drs[1].ToString().Contains("+"))
                            {
                                drs[1] = "";
                                drs[3] = "";
                            }

                            methodNamefoundFlag = false;
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (methodStartFlag)
                    {
                        openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(drs[1].ToString()) -
                                          _utility.GetCloseCurlyBracescount(drs[1].ToString());
                        if (openCurlyBraces == 0)
                        {
                            methodStartFlag = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetMethodsListInFile(DataTable dtForFileOfInclude, string fileName,
            DataTable dtForMethodsAvailable)
        {
            bool methodStartFlag = false;
            bool methodNamefoundFlag = false;

            string methodName;


            int openCurlyBraces = 0;
            int methodParameter = 0;

            DataRow dr;

            foreach (DataRow drs in dtForFileOfInclude.Rows)
            {
                if (drs[1].Equals(""))
                {
                    continue;
                }

                if (!methodNamefoundFlag && !methodStartFlag && _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                {
                    if (!drs[1].ToString().Contains("=") && !drs[1].ToString().Contains("+"))
                    {
                        methodName = GetOnlyMethodName(drs[1].ToString());
                        methodParameter = GetMethodParameterCount(drs[1].ToString());
                        dr = dtForMethodsAvailable.Rows.Add();
                        dr[0] = fileName;
                        dr[1] = methodName;
                        dr[2] = methodParameter;
                        methodNamefoundFlag = true;
                    }
                }
                if (methodNamefoundFlag)
                {
                    if (_utility.DetectMethodStartPointer(drs[1].ToString()))
                    {
                        methodStartFlag = true;
                        methodNamefoundFlag = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (methodStartFlag)
                {
                    openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(drs[1].ToString()) -
                                      _utility.GetCloseCurlyBracescount(drs[1].ToString());
                    if (openCurlyBraces == 0)
                    {
                        methodStartFlag = false;
                    }
                }
            }
            return dtForMethodsAvailable;
        }

        public string GetMethodName(string line)
        {
            string methodName = "";
            char[] splittedLine = line.ToCharArray();

            int i;
            int methodNameStartPosition;
            int methodNameEndPosition;

            methodNameStartPosition = line.IndexOf("(");
            methodNameEndPosition = line.IndexOf(")");

            for (i = methodNameStartPosition - 1; i >= 0; i--)
            {
                if (splittedLine[i].Equals(' '))
                {
                    if (line.Substring(i + 1, methodNameStartPosition - (i + 1)).Trim().Equals(""))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            methodNameStartPosition = i + 1;
            methodName = line.Substring(methodNameStartPosition, methodNameEndPosition + 1 - methodNameStartPosition);

            return methodName;
        }

        public string GetOnlyMethodName(string line)
        {
            string methodName = "";
            char[] splittedLine = line.ToCharArray();

            int i;
            int methodNameStartPosition;
            int methodNameEndPosition;

            methodNameStartPosition = line.IndexOf("(");
            methodNameEndPosition = line.IndexOf(")");

            for (i = methodNameStartPosition - 1; i >= 0; i--)
            {
                if (splittedLine[i].Equals(' '))
                {
                    if (line.Substring(i + 1, methodNameStartPosition - (i + 1)).Trim().Equals(""))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            methodName = line.Substring(i + 1, methodNameStartPosition - (i + 1)).Trim();

            return methodName;
        }

        public int GetMethodParameterCount(string line)
        {

            char[] splittedLine = line.ToCharArray();

            int i;
            int methodNameStartPosition;
            int methodNameEndPosition;

            methodNameStartPosition = line.IndexOf("(");
            methodNameEndPosition = line.IndexOf(")");
            if (
                line.Substring(methodNameStartPosition + 1, methodNameEndPosition - (methodNameStartPosition + 1))
                    .Trim()
                    .Equals(""))
            {
                return 0;
            }

            return
                _utility.GetCount(
                    line.Substring(methodNameStartPosition + 1, methodNameEndPosition - (methodNameStartPosition + 1)),
                    ",") + 1;
        }

        public void ConvertInitialization(DataTable dtForFile, DataTable dtForInclude, bool shoudInclude)
        {

            bool includeFlag;
            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    _query = Constants.INITIALIZESQL;
                    _dataReader = OperationDao.ExecuteDataReader(_query, out _conn);
                    while (_dataReader.Read())
                    {
                        includeFlag = true;
                        if (drs[1].ToString().Contains(_dataReader.GetValue(0).ToString()))
                        {
                            if (!_dataReader.GetValue(2).ToString().Equals(""))
                            {
                                if (shoudInclude)
                                {
                                    foreach (DataRow dr in dtForInclude.Rows)
                                    {
                                        if (dr[0].ToString().Equals(_dataReader.GetValue(2).ToString()))
                                        {
                                            includeFlag = false;
                                        }
                                    }
                                    if (includeFlag)
                                    {
                                        DataRow dr = dtForInclude.Rows.Add();
                                        dr[0] = _dataReader.GetValue(2).ToString();
                                        dr[1] = _dataReader.GetValue(3).ToString();
                                    }
                                }

                            }

                            drs[1] = drs[1].ToString()
                                .Replace(_dataReader.GetValue(0).ToString(), _dataReader.GetValue(1).ToString());
                        }
                    }
                    _dataReader.Close();
                    DbConnection.CloseSqlConnection(_conn);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public void InitlializedVariables(DataTable dtForFile)
        {
            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;
            string line;
            string DicOutput;
            bool checkVariableExist = false;
            string PrevElementType = string.Empty;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                string ElementType = string.Empty;

                foreach (DataRow drs in dtForFile.Rows)
                {
                    bool appendDataLabel = true;
                    stringBuilder.Clear();
                    line = drs[1].ToString();
                    if (line.Equals(String.Empty))
                    {
                        continue;
                    }

                    //if (_utility.DetectMethodNameStartPointer(line))
                    //{
                    //    if (!line.Contains(","))
                    //    {
                    //        methodStartFlag = true;
                    //    }
                    //}


                    //if (!methodNamefoundFlag && !methodStartFlag && _utility.DetectMethodNameStartPointer(line))
                    //{

                    //    if (!line.Contains("=") && !line.Contains("+"))
                    //    {
                    //        methodNamefoundFlag = true;

                    //    }
                    //}

                    //if (methodNamefoundFlag)
                    //{
                    //    if (_utility.DetectMethodStartPointer(Convert.ToString(drs[1])))
                    //    {
                    //        methodStartFlag = true;
                    //        methodNamefoundFlag = false;
                    //    }
                    //    else
                    //    {
                    //        continue;
                    //    }
                    //}
                    //if (methodStartFlag)
                    //{
                    //    openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(line) -
                    //                      _utility.GetCloseCurlyBracescount(line);
                    //    if (openCurlyBraces == 0)
                    //    {
                    //        methodStartFlag = false;
                    //    }
                    List<char> symbolArray = new List<char>() { '(', ')' };

                    if (line.Trim().StartsWith("if") || line.Trim().StartsWith("for"))
                    {
                        continue;
                    }





                    if (!line.Contains('='))
                    {
                        string[] strItemList = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        strItemList[0] = strItemList[0].Replace(strItemList[0],
                            strItemList[0].Contains('{')
                                ? strItemList[0].Substring(strItemList[0].IndexOf('{'),
                                    strItemList[0].Length - strItemList[0].IndexOf('{'))
                                : strItemList[0]);

                        string lastItem = strItemList.Select(x => x).Last();
                        foreach (var itemStr in strItemList)
                        {
                            if (itemStr.Trim().Contains(','))
                            {
                                string[] strSubItemList = itemStr.Split(new char[] { ',' },
                                    StringSplitOptions.RemoveEmptyEntries);
                                string subLastItem = strSubItemList.Select(x => x).Last();
                                foreach (var subItem in strSubItemList)
                                {
                                    //    if (!string.IsNullOrWhiteSpace(PrevElementType))
                                    //        ElementType = PrevElementType;
                                    _getVariableDictionary.Keys.Any(
                                        type => _utility.CheckVariableTypeName(subItem, type, out ElementType));

                                    //if (!string.IsNullOrWhiteSpace(ElementType))
                                    //    PrevElementType = ElementType;

                                    _getVariableDictionary.TryGetValue(!string.IsNullOrWhiteSpace(ElementType) ? ElementType : PrevElementType, out DicOutput);

                                    if (!symbolArray.Any(n => subItem.ToCharArray().Contains(n)))
                                    {
                                        if (!string.IsNullOrWhiteSpace(ElementType) && !string.IsNullOrWhiteSpace(DicOutput))
                                        {
                                             stringBuilder.Append(subItem.Insert(subItem.Length,
                                                                                    string.Format(" = {0}{1}", DicOutput,
                                                                                        subItem.Equals(subLastItem) ? ' ' : ',')));
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
                                            string.Format(" = {0}{1}", DicOutput,
                                                itemStr.Equals(lastItem) ? ' ' : ';')));
                                    }
                                }

                            }

                        }
                        if (stringBuilder.Length > 0)
                        {
                            if (line.Contains('{'))
                                drs[1] = string.Concat(drs[1].ToString()
                                                                        .Replace(line.Substring(line.IndexOf('{'), line.Length - line.IndexOf('{'))
                                                                                , stringBuilder.ToString()), ';');
                            else
                                drs[1] = string.Concat(stringBuilder.ToString(), ';');
                        }
                        else
                            drs[1] = drs[1];
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
                            //if (string.IsNullOrWhiteSpace(ElementType))
                            //{
                            //    ElementType = PrevElementType;
                            //    appendDataLabel = false;
                            //}
                            _getVariableDictionary.TryGetValue(ElementType, out DicOutput);
                            if (!string.IsNullOrWhiteSpace(ElementType) && !string.IsNullOrWhiteSpace(DicOutput))
                            {
                                IsChanged = true;
                                calval1.Append(ConvertDeclaredValues(item + ';', ElementType, DicOutput,
                                    appendDataLabel));
                            }
                        }
                        if (IsChanged)
                            drs[1] = calval1;
                        else
                            drs[1] = drs[1];
                    }
                }

                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ConvertDeclaredValues(string line, string elementType, string replaceval,
            bool appendDataLabel)
        {
            StringBuilder calval = new StringBuilder();
            bool openBracketFound = false;
            bool equalFound = false;
            char[] chars;
            int strLength = line.Length;

            if (!string.IsNullOrWhiteSpace(PrevElementType))
                line = line.Substring(0, strLength);
            else
                line = line.Substring((elementType.Length), strLength - (elementType.Length));
            calval.Clear();
            chars = line.ToCharArray();

            foreach (char character in chars)
            {

                if (openBracketFound)
                {
                    if (character == ')' &&
                        (calval.ToString().Count(n => n == '(') != calval.ToString().Count(n => n == ')')))
                    {
                        openBracketFound = false;
                    }
                    calval.Append(character);

                    continue;
                }

                if (equalFound)
                {
                    if (character == '(')
                    {
                        openBracketFound = true;
                    }
                    if (character == ',')
                    {
                        equalFound = false;
                    }
                    calval.Append(character);

                    continue;
                }

                if (!equalFound && !openBracketFound && character != '=' && (character == ',' || character == ';'))
                {
                    if (calval.ToString().Contains(','))
                    {
                        if (character == ';')
                        {
                            calval.Append(character);
                            if (calval[calval.ToString().IndexOf(character) - 1] == ',' && character == ';')
                            {
                                calval.Replace(character.ToString(), string.Empty);
                                PrevElementType = elementType;
                                continue;
                            }
                        }

                    }
                    if (calval[calval.Length - 1] == ';')
                    {
                        calval.Replace(character.ToString(), string.Empty);

                    }
                    calval.Append(string.Format("={0}{1}", replaceval, character));
                    PrevElementType = string.Empty;
                    continue;
                }

                if (!equalFound && !openBracketFound && character == '=')
                {
                    calval.Append(character);
                    equalFound = true;
                    continue;
                }

                calval.Append(character);

            }
            return string.Concat(appendDataLabel ? elementType : string.Empty, ' ', calval.ToString());
        }


        public void ConvertStaticMethod(DataTable dtForFile, DataTable dtForMethodsAvailable, DataTable dtForInclude,
            string fileName)
        {

            string[] methodsInaline;

            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    if (_utility.DetectMethodNameStartPointer(drs[1].ToString()))
                    {
                        methodsInaline = _utility.GetMethodNamesInALine(drs[1].ToString());
                        //methodsInaline = filterMethodName(methodsInaline);
                        StaticMethodConversion(methodsInaline, drs, dtForMethodsAvailable, dtForInclude, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void StaticMethodConversion(string[] methodsInaline, DataRow drs2, DataTable dtForMethodsAvailable,
            DataTable dtForInclude, string fileName)
        {
            string methodName;
            int i;
            int parameterCount;

            bool continueFlag;
            bool includeFlag;

            DataRow[] methodsAvailable;

            try
            {
                for (i = 0; i < methodsInaline.Length; i++)
                {
                    continueFlag = false;
                    if (methodsInaline[i] == null)
                    {
                        continue;
                    }

                    methodName = _utility.MethodNameWithoutParameter(methodsInaline[i]);
                    parameterCount = _utility.GetParameterCount(methodsInaline[i]);
                    if (dtForMethodsAvailable != null)
                    {
                        methodsAvailable =
                            dtForMethodsAvailable.Select("MethodName='" + methodName + "' AND ParameterCount='" +
                                                         parameterCount + "'");
                        if (methodsAvailable.Length == 1)
                        {
                            if (!methodsAvailable[0][0].ToString().Equals(fileName))
                            {
                                if (_utility.GetCount(drs2[1].ToString(), methodName) > 1)
                                {
                                    string line = drs2[1].ToString().Replace("  ", " ").Trim();
                                    bool exitWhileLoop = false;
                                    int position = 0;
                                    int actualPosition = 0;
                                    while (!exitWhileLoop)
                                    {
                                        position = line.IndexOf(methodName, actualPosition);
                                        if (position == -1)
                                        {
                                            break;
                                        }
                                        actualPosition = position + 1;
                                        int endIndex = position + methodName.Length - 1;
                                        if (position > 0)
                                        {
                                            while (line.ToCharArray()[position - 1] == ' ')
                                            {
                                                position--;
                                            }

                                            if (line.ToCharArray()[position - 1] == '.' ||
                                                line.ToCharArray()[position - 1] == '_')
                                            {
                                                continue;
                                            }
                                            if (position == actualPosition - 1 &&
                                                Char.IsLetterOrDigit(line.ToCharArray()[position - 1]))
                                            {
                                                continue;
                                            }
                                        }
                                        if (endIndex < line.Length - 1)
                                        {
                                            if (Char.IsLetterOrDigit(line.ToCharArray()[endIndex + 1]))
                                            {
                                                continue;
                                            }
                                            position = endIndex;
                                            while (line.ToCharArray()[position + 1] == ' ')
                                            {
                                                position++;
                                            }
                                            endIndex = position;
                                            if (line.ToCharArray()[endIndex + 1] == '.' ||
                                                line.ToCharArray()[endIndex + 1] == '_')
                                            {
                                                continue;
                                            }
                                        }
                                        drs2[1] = line.Substring(0, actualPosition - 1) +
                                                  methodsAvailable[0][0].ToString().Replace(".mls", "") + "." +
                                                  line.Substring(actualPosition - 1);
                                        exitWhileLoop = true;
                                    }
                                }
                                else
                                {
                                    drs2[1] = drs2[1].ToString()
                                        .Replace(methodName,
                                            methodsAvailable[0][0].ToString().Replace(".mls", "") + "." + methodName);
                                }
                            }
                            continueFlag = true;
                        }
                        else if (methodsAvailable.Length > 1)
                        {
                            continueFlag = true;
                        }
                    }
                    if (!continueFlag)
                    {
                        _query = string.Format("{0}{1}", Constants.AVSJVSYESMAPPINGSQL,
                            " and avsmethod = '" + methodName + "'");
                        _dataReader = OperationDao.ExecuteDataReader(_query, out _conn);
                        includeFlag = true;
                        while (_dataReader.Read())
                        {
                            foreach (DataRow drs in dtForInclude.Rows)
                            {
                                if (drs[0].ToString().Equals(_dataReader.GetValue(1).ToString()))
                                {
                                    includeFlag = false;
                                }
                            }
                            if (includeFlag)
                            {
                                DataRow drs = dtForInclude.Rows.Add();
                                drs[0] = _dataReader.GetValue(1).ToString();
                                drs[1] = _dataReader.GetValue(3).ToString();
                            }
                            if (_utility.GetCount(drs2[1].ToString(), methodName) > 1)
                            {
                                string line = drs2[1].ToString().Replace("  ", " ");
                                bool exitWhileLoop = false;
                                int position = 0;
                                int actualPosition = 0;
                                while (!exitWhileLoop)
                                {
                                    position = line.IndexOf(methodName, actualPosition);
                                    if (position == -1)
                                    {
                                        break;
                                    }
                                    actualPosition = position + 1;
                                    int endIndex = position + methodName.Length - 1;
                                    if (position > 0)
                                    {
                                        while (line.ToCharArray()[position - 1] == ' ')
                                        {
                                            position--;
                                        }
                                        if (line.ToCharArray()[position - 1] == '.' ||
                                            line.ToCharArray()[position - 1] == '_')
                                        {
                                            continue;
                                        }
                                        if (position == actualPosition - 1 &&
                                            Char.IsLetterOrDigit(line.ToCharArray()[position - 1]))
                                        {
                                            continue;
                                        }
                                    }
                                    if (endIndex < line.Length - 1)
                                    {
                                        if (Char.IsLetterOrDigit(line.ToCharArray()[endIndex + 1]))
                                        {
                                            continue;
                                        }
                                        position = endIndex;
                                        while (line.ToCharArray()[position + 1] == ' ')
                                        {
                                            position++;
                                        }
                                        if (line.ToCharArray()[position + 1] == '.' ||
                                            line.ToCharArray()[position + 1] == '_')
                                        {
                                            continue;
                                        }
                                    }
                                    drs2[1] = line.Substring(0, actualPosition - 1) +
                                              _dataReader.GetValue(1).ToString() + "." +
                                              _dataReader.GetValue(2).ToString() +
                                              line.Substring(endIndex + 1);
                                    exitWhileLoop = true;
                                }
                            }
                            else
                            {
                                drs2[1] = drs2[1].ToString()
                                    .Replace(methodName,
                                        _dataReader.GetValue(1).ToString() + "." + _dataReader.GetValue(2).ToString());
                            }
                            break;
                        }
                        _dataReader.Close();
                        DbConnection.CloseSqlConnection(_conn);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConvertNonStaticMethod(DataTable dtForFile, DataTable dtForMethodsAvailable, DataTable dtForInclude,
            string fileName)
        {
            string[] methodsInaline;



            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    if (_utility.DetectMethodNameStartPointer(drs[1].ToString()))
                    {
                        methodsInaline = _utility.GetMethodNamesInALine(drs[1].ToString());
                        //methodsInaline = filterMethodName(methodsInaline);
                        NonStaticMethodConversion(methodsInaline, drs, dtForMethodsAvailable, dtForInclude, fileName);

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void NonStaticMethodConversion(string[] methodsInline, DataRow drs2, DataTable dtForMethodsAvailable,
            DataTable dtForInclude, string fileName)
        {


            string methodName;
            string argumentName;
            string sql;

            int i;
            int argumentLength = 0;
            int parameterCount;
            int methodNameIndex = 0;
            int methodstartIndex;
            int bracketCount;
            int j;

            char[] strArray;

            bool includeFlag;
            bool continueFlag;
            bool methodNameFound;


            DataRow[] methodsAvailable;

            try
            {

                for (i = 0; i < methodsInline.Length; i++)
                {
                    continueFlag = false;
                    if (methodsInline[i] == null)
                    {
                        continue;
                    }

                    methodName = _utility.MethodNameWithoutParameter(methodsInline[i]);
                    parameterCount = _utility.GetParameterCount(methodsInline[i]);

                    if (dtForMethodsAvailable != null)
                    {
                        methodsAvailable =
                            dtForMethodsAvailable.Select("MethodName='" + methodName + "' AND ParameterCount='" +
                                                         parameterCount + "'");
                        if (methodsAvailable.Length == 1)
                        {
                            continueFlag = true;
                        }
                        else if (methodsAvailable.Length > 1)
                        {
                            continueFlag = true;
                        }
                    }
                    if (!continueFlag)
                    {


                        _query = string.Format("{0}{1}", Constants.AVSJVSNOMAPPINGSQL,
                            " and avsmethod = '" + methodName + "'");

                        _dataReader = OperationDao.ExecuteDataReader(_query, out _conn);
                        includeFlag = true;
                        while (_dataReader.Read())
                        {
                            foreach (DataRow drs in dtForInclude.Rows)
                            {
                                if (drs[0].ToString().Equals(_dataReader.GetValue(1).ToString()))
                                {
                                    includeFlag = false;
                                }
                            }
                            if (includeFlag)
                            {
                                DataRow drs = dtForInclude.Rows.Add();
                                drs[0] = _dataReader.GetValue(1).ToString();
                                drs[1] = _dataReader.GetValue(3).ToString();

                            }

                            bracketCount = 0;
                            argumentName = "";

                            methodNameFound = true;
                            methodNameIndex = 0;
                            while (methodNameFound)
                            {
                                if (methodNameIndex > 0)
                                    methodNameIndex = drs2[1].ToString().IndexOf(methodName, methodNameIndex + 1);
                                if (methodNameIndex == 0)
                                    methodNameIndex = drs2[1].ToString().IndexOf(methodName, methodNameIndex);
                                if (
                                    !Char.IsLetterOrDigit(
                                        drs2[1].ToString().ToCharArray()[methodNameIndex + methodName.Length]))
                                {
                                    methodNameFound = false;
                                }
                            }
                            methodstartIndex = drs2[1].ToString().IndexOf("(", methodNameIndex);
                            strArray =
                                drs2[1].ToString()
                                    .Substring(methodstartIndex + 1, drs2[1].ToString().Length - methodstartIndex - 1)
                                    .ToCharArray();
                            for (j = 0; j < strArray.Length; j++)
                            {
                                if (strArray[j] == ')')
                                {
                                    bracketCount--;
                                    if (bracketCount < 0)
                                    {
                                        for (int k = 0; k < j; k++)
                                        {
                                            argumentName = argumentName + strArray[k];
                                        }
                                        argumentLength = j + 1;
                                        break;
                                    }
                                }
                                if (strArray[j] == '(')
                                {
                                    bracketCount++;
                                }
                                if (strArray[j] == ',')
                                {
                                    if (bracketCount == 0)
                                    {
                                        for (int k = 0; k < j; k++)
                                        {
                                            argumentName = argumentName + strArray[k];
                                        }
                                        argumentLength = j + 2;
                                        break;
                                    }
                                }
                            }

                            drs2[1] = drs2[1].ToString().Substring(0, methodstartIndex + 1) +
                                      drs2[1].ToString().Substring(methodstartIndex + argumentLength);
                            drs2[1] = drs2[1].ToString().Substring(0, methodNameIndex) + argumentName + "." +
                                      _dataReader.GetValue(2).ToString() +
                                      drs2[1].ToString().Substring(methodstartIndex);

                            break;
                        }
                    }
                    _dataReader.Close();
                    DbConnection.CloseSqlConnection(_conn);


                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string[] FilterMethodName(string[] methodsInaline)
        {
            try
            {

                for (int i = 0; i < methodsInaline.Length; i++)
                {
                    if (methodsInaline[i] == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < i; j++)
                    {
                        if (methodsInaline[j] == null)
                        {
                            continue;
                        }

                        if (methodsInaline[i].Equals(methodsInaline[j]))
                        {
                            methodsInaline[i] = null;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return methodsInaline;
        }

        public void ConvertIfStatements(DataTable dtForFile)
        {
            char[] lineArray;
            bool iFlag;
            bool ifFlag = false;
            bool ifStartFlag = false;
            char previousLetter;
            bool notFlag;
            bool conditionalOperatorFoundFlag = false;
            ;
            int bracketCount;
            int additionCount;
            bool ifandNotExistInThisLine;

            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    ifandNotExistInThisLine = false;

                    lineArray = drs[1].ToString().ToCharArray();
                    additionCount = 0;
                    bracketCount = 0;
                    iFlag = false;
                    notFlag = false;
                    previousLetter = ' ';

                    for (int i = 0; i < lineArray.Length; i++)
                    {
                        if (ifStartFlag)
                        {
                            if (char.IsLetterOrDigit(lineArray[i]))
                            {
                                continue;
                            }
                            if (lineArray[i] == '<' || lineArray[i] == '>' || lineArray[i] == '=')
                            {
                                conditionalOperatorFoundFlag = true;
                                continue;
                            }

                            if (lineArray[i] == '!' && lineArray[i + 1] != '=')
                            {
                                ifandNotExistInThisLine = true;
                                notFlag = true;
                                continue;
                            }

                            if (lineArray[i] == '(')
                            {
                                bracketCount++;
                                continue;
                            }

                            if (lineArray[i] == '|' || lineArray[i] == '&')
                            {
                                if (!conditionalOperatorFoundFlag)
                                {
                                    if (notFlag)
                                    {
                                        drs[1] = drs[1].ToString().Insert(i + additionCount, "==0");
                                        additionCount = additionCount + 3;
                                        notFlag = false;

                                    }
                                    else
                                    {
                                        drs[1] = drs[1].ToString().Insert(i + additionCount, ">0");
                                        additionCount = additionCount + 2;

                                    }
                                }
                                else
                                {
                                    conditionalOperatorFoundFlag = false;
                                }
                                i++;
                                continue;
                            }

                            if (lineArray[i] == ')')
                            {
                                bracketCount--;
                                if (bracketCount < 0)
                                {
                                    if (!conditionalOperatorFoundFlag)
                                    {
                                        if (notFlag)
                                        {
                                            drs[1] = drs[1].ToString().Insert(i + additionCount, "==0");
                                            additionCount = additionCount + 3;
                                            ifStartFlag = false;
                                            notFlag = false;
                                        }
                                        else
                                        {
                                            drs[1] = drs[1].ToString().Insert(i + additionCount, ">0");
                                            additionCount = additionCount + 2;
                                            ifStartFlag = false;
                                        }
                                    }
                                    else
                                    {
                                        notFlag = false;
                                        ifStartFlag = false;
                                        conditionalOperatorFoundFlag = false;
                                    }

                                }
                                continue;
                            }
                        }

                        if (ifFlag && lineArray[i] == ' ')
                        {
                            previousLetter = lineArray[i];
                            continue;
                        }

                        if (ifFlag && lineArray[i] == '(')
                        {
                            previousLetter = lineArray[i];
                            ifFlag = false;
                            ifStartFlag = true;

                            continue;
                        }
                        if (ifFlag)
                        {
                            ifFlag = false;
                        }

                        if (!Char.IsLetterOrDigit(previousLetter) && lineArray[i] == 'i')
                        {
                            iFlag = true;
                            previousLetter = lineArray[i];
                            continue;
                        }
                        if (iFlag && lineArray[i] == 'f')
                        {
                            ifFlag = true;
                            iFlag = false;
                            previousLetter = lineArray[i];
                            continue;
                        }
                        if (iFlag && lineArray[i] != 'f')
                        {
                            iFlag = false;
                            previousLetter = lineArray[i];
                            continue;
                        }
                        previousLetter = lineArray[i];
                    }
                    if (ifandNotExistInThisLine)
                    {

                        drs[1] = drs[1].ToString().Replace("!", "");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public void ReplaceTrueFalse(DataTable dtForFile)
        {
            int position = 0;
            foreach (DataRow drs in dtForFile.Rows)
            {
                if (drs[1].Equals(""))
                {
                    continue;
                }
                position = 0;
                while (drs[1].ToString().IndexOf("TRUE", position) >= 0)
                {
                    position = drs[1].ToString().IndexOf("TRUE", position);
                    if (drs[1].ToString().ToCharArray()[position - 1] != '_' &&
                        drs[1].ToString().ToCharArray()[position - 1] != '.' &&
                        !Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[position - 1]) &&
                        drs[1].ToString().ToCharArray()[position + 4] != '_' &&
                        drs[1].ToString().ToCharArray()[position + 4] != '.' &&
                        !Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[position + 4]))
                    {
                        drs[1] = drs[1].ToString().Substring(0, position) + 1 +
                                 drs[1].ToString().Substring(position + 4);
                    }
                    position++;
                }
                position = 0;
                while (drs[1].ToString().IndexOf("FALSE", position) >= 0)
                {
                    position = drs[1].ToString().IndexOf("FALSE", position);
                    if (drs[1].ToString().ToCharArray()[position - 1] != '_' &&
                        drs[1].ToString().ToCharArray()[position - 1] != '.' &&
                        !Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[position - 1]) &&
                        drs[1].ToString().ToCharArray()[position + 5] != '_' &&
                        drs[1].ToString().ToCharArray()[position + 5] != '.' &&
                        !Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[position + 5]))
                    {
                        drs[1] = drs[1].ToString().Substring(0, position) + 0 +
                                 drs[1].ToString().Substring(position + 5);
                    }
                    position++;
                }
            }
        }

        public void AddPublicStaticInLibraryMethods(DataTable dtForFile)
        {
            bool mainCheck;

            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;

            try
            {
                mainCheck = CheckMain(dtForFile);
                if (mainCheck)
                {
                    return;
                }
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }

                    if (!methodStartFlag && !methodNamefoundFlag)
                    {
                        drs[1] = "public static " + drs[1].ToString();
                    }

                    if (!methodNamefoundFlag && !methodStartFlag &&
                        _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                    {
                        methodNamefoundFlag = true;
                    }
                    if (methodNamefoundFlag)
                    {
                        if (_utility.DetectMethodStartPointer(drs[1].ToString()))
                        {
                            methodStartFlag = true;
                            methodNamefoundFlag = false;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (methodStartFlag)
                    {
                        openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(drs[1].ToString()) -
                                          _utility.GetCloseCurlyBracescount(drs[1].ToString());
                        if (openCurlyBraces == 0)
                        {
                            methodStartFlag = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckMain(DataTable dtForFile)
        {
            foreach (DataRow drs in dtForFile.Rows)
            {
                if (drs[1].ToString().Contains(" main()") ||
                    (drs[1].ToString().Contains(" main ") && drs[1].ToString().Contains(" ()")))
                {
                    return true;
                }
            }
            return false;
        }

        public void GenerateOutputFile(DataTable dtForFile, DataTable dtForInclude, DataTable dtForLiterals,
            string outputPath, string outputFileName)
        {
            bool mainCheck;
            bool argtCheck = false;
            int lineaftermain = -1;
            bool breakFlag = false;
            bool firstValidLineFlag = false;
            string line;

            try
            {
                string fileName = outputPath + "\\" + outputFileName.Replace(".mls", "") + ".java";
                System.IO.StreamWriter writer;
                writer = new System.IO.StreamWriter(fileName);

                foreach (DataRow drs in dtForInclude.Rows)
                {
                    writer.Write("import " + drs[1] + ";\n");
                }
                writer.Write("import com.olf.openjvs.OException;\n");

                mainCheck = CheckMain(dtForFile);

                if (mainCheck)
                {
                    writer.Write("import com.olf.openjvs.IContainerContext;\n");
                    writer.Write("import com.olf.openjvs.IScript;\n");

                    writer.Write("\n");

                }

                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (breakFlag)
                    {
                        if (!drs[2].Equals(""))
                        {
                            writer.Write(drs[2] + "\n");
                        }
                        break;
                    }

                    if (drs[1].Equals("") && drs[2].Equals("") && drs[3].Equals(""))
                    {
                        continue;
                    }

                    if (!drs[1].Equals(""))
                    {
                        breakFlag = true;
                    }
                    writer.Write(drs[2] + "" + drs[3] + "\n");
                }

                if (mainCheck)
                {
                    writer.Write("public class " + outputFileName.Replace(".mls", "") + " implements IScript" + "\n");
                    writer.Write("{" + "\n");
                    argtCheck = Checkargt(dtForFile);
                }
                else
                {
                    writer.Write("\n");
                    writer.Write("public class " + outputFileName.Replace(".mls", "") + "\n");
                    writer.Write("{" + "\n");
                }

                if (argtCheck)
                {
                    writer.Write("Table argt;\n");
                }

                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (!firstValidLineFlag && drs[1].Equals(""))
                    {
                        continue;
                    }
                    if (lineaftermain > -1)
                    {
                        lineaftermain++;
                    }
                    if (mainCheck && drs[1].ToString().Contains("void ") &&
                        drs[1].ToString().Contains(" main") && drs[1].ToString().Contains("()"))
                    {
                        line = drs[1].ToString();
                        drs[1] = line.Substring(0, line.IndexOf("void ")) +
                                 "public void execute(IContainerContext context) " +
                                 line.Substring(line.IndexOf(")") + 1);
                        if (argtCheck)
                        {
                            lineaftermain = 0;
                        }
                    }
                    if (mainCheck && drs[1].ToString().Contains("int ") &&
                        drs[1].ToString().Contains(" main") && drs[1].ToString().Contains("()"))
                    {
                        line = drs[1].ToString();
                        drs[1] = line.Substring(0, line.IndexOf("int ")) +
                                 "public void execute(IContainerContext context) " +
                                 line.Substring(line.IndexOf(")") + 1);
                        if (argtCheck)
                        {
                            lineaftermain = 0;
                        }
                    }
                    if (drs[1].ToString().Contains("StringLiteralDetected"))
                    {
                        ReplaceLiteral(drs, dtForLiterals);
                    }
                    if (lineaftermain == 1)
                    {
                        if (drs[1].ToString().Trim().Equals("{"))
                        {
                            drs[1] = drs[1] + "\nargt=context.getArgumentsTable();\n";
                        }
                        else if (drs[1].ToString().Contains("{"))
                        {
                            int indexOfBracket;
                            indexOfBracket = drs[1].ToString().IndexOf("{");
                            drs[1] = drs[1].ToString().Substring(0, indexOfBracket + 1) +
                                     "\nargt=context.getArgumentsTable();\n" +
                                     drs[1].ToString().Substring(indexOfBracket + 1);
                        }
                        else
                        {
                            drs[1] = "argt=context.getArgumentsTable(); \n" + drs[1];
                        }
                    }
                    if (firstValidLineFlag)
                    {
                        writer.Write(drs[2] + "" + drs[1] + "" + drs[3] + "\n");
                    }
                    else
                    {
                        writer.Write(drs[1] + "" + drs[3] + "\n");
                        firstValidLineFlag = true;
                    }
                }
                writer.Write("}" + "\n");
                writer.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Checkargt(DataTable dtForFile)
        {
            foreach (DataRow drs in dtForFile.Rows)
            {
                if (drs[1].ToString().Contains("argt"))
                {
                    return true;
                }
            }
            return false;
        }

        public void ReplaceLiteral(DataRow drs, DataTable dtForLiterals)
        {
            int index;
            int number = 0;
            string literalName;

            try
            {

                index = drs[1].ToString().IndexOf("StringLiteralDetected");

                while (drs[1].ToString().Length > (index + 21) && Char.IsDigit(drs[1].ToString()[index + 21]))
                {
                    number = number * 10 + Convert.ToInt32(drs[1].ToString().ToCharArray()[index + 21].ToString());
                    index++;
                }

                literalName = "StringLiteralDetected" + number;

                foreach (DataRow dr in dtForLiterals.Rows)
                {
                    if (dr[0].ToString().Equals(literalName))
                    {
                        drs[1] = drs[1].ToString().Replace(literalName, dr[1].ToString());
                        break;
                    }
                }

                if (drs[1].ToString().Contains("StringLiteralDetected"))
                {
                    ReplaceLiteral(drs, dtForLiterals);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ConvertEnums(DataTable dtForFile, DataTable dtForVariables, string fileName, DataTable dtForInclude)
        {
            int startIndex;
            int endIndex;
            int position;
            bool ignoreFound = false;

            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;
            string methodName = "";

            string wordsInFile = "";
            bool prevWasSymbol = true;
            char[] chars;
            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    chars = drs[1].ToString().ToCharArray();
                    foreach (char character in chars)
                    {
                        if (!char.IsLetterOrDigit(character) && character != '_')
                        {
                            if (!prevWasSymbol)
                            {
                                wordsInFile = wordsInFile + "',";
                            }
                            prevWasSymbol = true;
                            continue;
                        }
                        if (prevWasSymbol)
                        {
                            wordsInFile = wordsInFile + "'";
                        }
                        wordsInFile = wordsInFile + character;
                        prevWasSymbol = false;
                    }
                }
                if (wordsInFile.Trim().Equals(""))
                {
                    wordsInFile = "''";
                }

                if (wordsInFile.LastIndexOf(",") == wordsInFile.Length - 1)
                {
                    wordsInFile = wordsInFile.Substring(0, wordsInFile.Length - 1);
                }


                _query =
                    "select Enums.FileName, Enums.Enum, Enums.Include, Enums.Append, Enums.Exclude from Enums where Enums.Enum in (" +
                    wordsInFile + ")";

                _dataReader = OperationDao.ExecuteDataReader(_query, out _conn);
                while (_dataReader.Read())
                {
                    startIndex = 0;
                    endIndex = 0;

                    methodNamefoundFlag = false;
                    methodStartFlag = false;
                    openCurlyBraces = 0;
                    methodName = "";

                    foreach (DataRow drs in dtForFile.Rows)
                    {
                        if (drs[1].Equals(""))
                        {
                            continue;
                        }

                        if (!methodNamefoundFlag && !methodStartFlag &&
                            _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                        {
                            ignoreFound = false;
                            if (drs[1].ToString().IndexOf("=") > drs[1].ToString().IndexOf(")"))
                            {
                                ignoreFound = true;
                            }
                            if ((drs[1].ToString().Contains("=") || drs[1].ToString().Contains("+")) || ignoreFound)
                            {
                                position = 0;
                                while (drs[1].ToString().IndexOf(_dataReader.GetValue(1).ToString(), position) >= 0)
                                {
                                    position = drs[1].ToString().IndexOf(_dataReader.GetValue(1).ToString(), position);

                                    startIndex = drs[1].ToString().IndexOf(_dataReader.GetValue(1).ToString(), position);
                                    endIndex = startIndex + _dataReader.GetValue(1).ToString().Length - 1;
                                    if (startIndex > 0)
                                    {
                                        if (drs[1].ToString().ToCharArray()[startIndex - 1] == '.' ||
                                            drs[1].ToString().ToCharArray()[startIndex - 1] == '_')
                                        {
                                            position++;
                                            continue;
                                        }
                                        if (Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[startIndex - 1]))
                                        {
                                            position++;
                                            continue;
                                        }
                                    }
                                    if (endIndex < drs[1].ToString().Length - 1)
                                    {
                                        if (drs[1].ToString().ToCharArray()[endIndex + 1] == '.' ||
                                            drs[1].ToString().ToCharArray()[endIndex + 1] == '_')
                                        {
                                            position++;
                                            continue;
                                        }
                                        if (Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[endIndex + 1]))
                                        {
                                            position++;
                                            continue;
                                        }
                                    }

                                    if (_utility.CheckWhetherAppendShouldBeAddedOrNot(drs[1].ToString(), startIndex,
                                        _dataReader.GetValue(4).ToString()))
                                    {
                                        drs[1] = drs[1].ToString().Substring(0, startIndex) +
                                                 _dataReader.GetValue(0).ToString() + "." +
                                                 _dataReader.GetValue(1).ToString() +
                                                 _dataReader.GetValue(3).ToString() +
                                                 drs[1].ToString().Substring(endIndex + 1);
                                        position = position + _dataReader.GetValue(0).ToString().Length + 2;
                                    }
                                    else
                                    {
                                        drs[1] = drs[1].ToString().Substring(0, startIndex) +
                                                 _dataReader.GetValue(0).ToString() + "." +
                                                 _dataReader.GetValue(1).ToString() +
                                                 drs[1].ToString().Substring(endIndex + 1);
                                        position = position + _dataReader.GetValue(0).ToString().Length + 2;
                                    }
                                    _utility.AddIncludeDT(dtForInclude, _dataReader.GetValue(2).ToString(),
                                        _dataReader.GetValue(0).ToString());
                                    continue;
                                }
                            }
                        }

                        if (!methodNamefoundFlag && !methodStartFlag &&
                            _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                        {
                            ignoreFound = false;
                            if (drs[1].ToString().IndexOf("=") > drs[1].ToString().IndexOf(")"))
                            {
                                ignoreFound = true;
                            }
                            if ((!drs[1].ToString().Contains("=") && !drs[1].ToString().Contains("+")) || ignoreFound)
                            {
                                methodNamefoundFlag = true;
                                methodName =
                                    drs[1].ToString()
                                        .Substring(drs[1].ToString().IndexOf(" "),
                                            drs[1].ToString().IndexOf("(") - drs[1].ToString().IndexOf(" "))
                                        .Trim();
                            }
                        }

                        if (methodNamefoundFlag)
                        {
                            if (_utility.DetectMethodStartPointer(Convert.ToString(drs[1])))
                            {
                                methodStartFlag = true;
                                methodNamefoundFlag = false;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (methodStartFlag)
                        {
                            position = 0;
                            while (drs[1].ToString().IndexOf(_dataReader.GetValue(1).ToString(), position) >= 0)
                            {
                                position = drs[1].ToString().IndexOf(_dataReader.GetValue(1).ToString(), position);

                                startIndex = drs[1].ToString().IndexOf(_dataReader.GetValue(1).ToString(), position);
                                endIndex = startIndex + _dataReader.GetValue(1).ToString().Length - 1;
                                if (startIndex > 0)
                                {
                                    if (drs[1].ToString().ToCharArray()[startIndex - 1] == '.' ||
                                        drs[1].ToString().ToCharArray()[startIndex - 1] == '_')
                                    {
                                        position++;
                                        continue;
                                    }
                                    if (Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[startIndex - 1]))
                                    {
                                        position++;
                                        continue;
                                    }
                                }
                                if (endIndex < drs[1].ToString().Length - 1)
                                {
                                    if (drs[1].ToString().ToCharArray()[endIndex + 1] == '.' ||
                                        drs[1].ToString().ToCharArray()[endIndex + 1] == '_')
                                    {
                                        position++;
                                        continue;
                                    }
                                    if (Char.IsLetterOrDigit(drs[1].ToString().ToCharArray()[endIndex + 1]))
                                    {
                                        position++;
                                        continue;
                                    }
                                }
                                if (_utility.CheckInVariableList(dtForVariables, _dataReader.GetValue(1).ToString(),
                                    methodName, fileName))
                                {
                                    if (_utility.CheckWhetherAppendShouldBeAddedOrNot(drs[1].ToString(), startIndex,
                                        _dataReader.GetValue(4).ToString()))
                                    {
                                        drs[1] = drs[1].ToString().Substring(0, startIndex) +
                                                 _dataReader.GetValue(0).ToString() + "." +
                                                 _dataReader.GetValue(1).ToString() +
                                                 _dataReader.GetValue(3).ToString() +
                                                 drs[1].ToString().Substring(endIndex + 1);
                                    }
                                    else
                                    {
                                        drs[1] = drs[1].ToString().Substring(0, startIndex) +
                                                 _dataReader.GetValue(0).ToString() + "." +
                                                 _dataReader.GetValue(1).ToString() +
                                                 drs[1].ToString().Substring(endIndex + 1);
                                    }
                                    _utility.AddIncludeDT(dtForInclude, _dataReader.GetValue(2).ToString(),
                                        _dataReader.GetValue(0).ToString());
                                    position = position + _dataReader.GetValue(0).ToString().Length + 2;
                                    continue;
                                }
                                position++;
                            }

                            openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(drs[1].ToString()) -
                                              _utility.GetCloseCurlyBracescount(drs[1].ToString());
                            if (openCurlyBraces == 0)
                            {
                                methodName = "";
                                methodStartFlag = false;
                            }
                        }
                    }
                }
                _dataReader.Close();
                DbConnection.CloseSqlConnection(_conn);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void AddThrowsExceptionInLibraryMethods(DataTable dtForFile)
        {

            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;
            int positionOfCloseBraces = -1;

            foreach (DataRow drs in dtForFile.Rows)
            {
                if (drs[1].Equals(""))
                {
                    continue;
                }

                if (!methodNamefoundFlag && !methodStartFlag && _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                {
                    if (drs[1].ToString().Contains("=") || drs[1].ToString().Contains("+") ||
                        drs[1].ToString().Contains(";"))
                    {
                        continue;
                    }
                    methodNamefoundFlag = true;
                }
                if (methodNamefoundFlag)
                {
                    if (_utility.DetectMethodStartPointer(drs[1].ToString()))
                    {
                        positionOfCloseBraces = drs[1].ToString().IndexOf("{");
                        if (positionOfCloseBraces == 0)
                        {
                            drs[1] = " throws OException " + drs[1].ToString();
                        }
                        else
                        {
                            drs[1] = drs[1].ToString().Substring(0, positionOfCloseBraces) + " throws OException " +
                                     drs[1].ToString().Substring(positionOfCloseBraces);
                        }
                        methodStartFlag = true;
                        methodNamefoundFlag = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (methodStartFlag)
                {
                    openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(drs[1].ToString()) -
                                      _utility.GetCloseCurlyBracescount(drs[1].ToString());
                    if (openCurlyBraces == 0)
                    {
                        methodStartFlag = false;
                    }
                }
            }
        }

        public DataTable GetVariableList(DataTable dtForFile, string fileName)
        {


            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;
            DataTable dtForVariables;
            DataRow dr;
            bool firstGo = false;
            string methodName = "";
            string variableList = "";
            string parameterList = "";
            string line;
            bool ignorefound = false;

            dtForVariables = _initializeTables.GetDtForVariables();

            foreach (DataRow drs in dtForFile.Rows)
            {

                line = drs[1].ToString();
                if (line.Equals(String.Empty))
                {
                    continue;
                }

                if (!methodNamefoundFlag && !methodStartFlag && _utility.DetectMethodNameStartPointer(line))
                {
                    ignorefound = false;
                    if (line.IndexOf("=") > line.IndexOf(")"))
                    {
                        ignorefound = true;
                    }
                    if ((!line.Contains("=") && !line.Contains("+")) || ignorefound)
                    {
                        methodNamefoundFlag = true;
                        methodName = line.Substring(line.IndexOf(" "), line.IndexOf("(") - line.IndexOf(" ")).Trim();
                        firstGo = true;
                    }

                }

                if (!methodStartFlag && methodNamefoundFlag && firstGo)
                {
                    firstGo = false;
                    int startPositionOfMethodParameter = line.IndexOf("(") + 1;
                    int endPositionOfMethodParameter = line.IndexOf(")");
                    string parameter =
                        line.Substring(startPositionOfMethodParameter,
                            endPositionOfMethodParameter - startPositionOfMethodParameter).Trim();
                    if (parameter.Length > 0)
                    {
                        int count = _utility.GetCount(parameter, ",") + 1;
                        for (int i = 0; i < count; i++)
                        {
                            if (parameter.Contains(","))
                            {
                                if (parameterList.Length == 0)
                                {
                                    parameterList =
                                        parameter.Substring(parameter.IndexOf(" "),
                                            parameter.IndexOf(",") - parameter.IndexOf(" ")).Trim();
                                }
                                else
                                {
                                    parameterList = parameterList + "," +
                                                    parameter.Substring(parameter.IndexOf(" "),
                                                        parameter.IndexOf(",") - parameter.IndexOf(" ")).Trim();
                                }
                                parameter = parameter.Substring(parameter.IndexOf(",") + 1).Trim();
                            }
                            else
                            {
                                if (count == 1)
                                {
                                    parameterList = parameter.Substring(parameter.IndexOf(" ")).Trim();
                                }
                                else
                                {
                                    parameterList = parameterList + "," +
                                                    parameter.Substring(parameter.IndexOf(" ")).Trim();
                                }
                            }
                        }
                    }
                }

                if (methodNamefoundFlag)
                {
                    if (_utility.DetectMethodStartPointer(Convert.ToString(drs[1])))
                    {
                        methodStartFlag = true;
                        methodNamefoundFlag = false;
                        line = line.Substring(line.IndexOf("{"));
                    }
                    else
                    {
                        continue;
                    }
                }
                if (methodStartFlag)
                {
                    if (_utility.CheckVariableDeclaration(line))
                    {
                        variableList = _utility.GetVariableList(line, variableList);
                        variableList = _utility.CorrectTheVariableList(variableList);
                    }

                    openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(line) -
                                      _utility.GetCloseCurlyBracescount(line);
                    if (openCurlyBraces == 0)
                    {
                        if (!parameterList.Equals("") || !variableList.Equals(""))
                        {
                            dr = dtForVariables.Rows.Add();
                            dr[0] = fileName;
                            dr[1] = methodName;
                            dr[2] = parameterList;
                            dr[3] = variableList;
                            parameterList = "";
                            variableList = "";
                        }
                        methodStartFlag = false;
                    }
                }
            }
            return dtForVariables;
        }

        public void AddTryCatchStatement(DataTable dtForFile)
        {
            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }


                }
            }
            catch (Exception ex)
            {

            }
        }

        public void ReplaceWhileIntoBoolStatement(DataTable dtForFile)
        {
            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }

                    string line = drs[1].ToString();
                    List<char> symbolArray = new List<char>() { '(', ')' };
                    if (line.Contains("while") && symbolArray.Any(n => line.ToCharArray().Contains(n)))
                    {
                        line.Substring(line.IndexOf('('), line.IndexOf(')'));

                    }

                }
            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForFile"></param>
        public void ValidateWhileStatement(DataTable dtForFile)
        {
            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    drs[1] = CheckWhile(drs[1].ToString());

                }
            }
            catch (Exception ex)
            {
                throw ex;
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
        //        subStr = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - line.IndexOf('(') - 1);
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
        //    StringBuilder strbuilder = new StringBuilder();
        //    List<char> symbolList = new List<char>() { '=', '<', '>' };
        //    int counter = 1;


        //    if (strSymbol.Contains('!') && !strSymbol.Contains("&&") && !strSymbol.Contains("||"))
        //    {
        //        subNotStr = strSymbol.Substring(1, strSymbol.Length - 1);
        //        strSymbol = strSymbol.Replace(strSymbol, string.Concat(subNotStr, "!=", 0));
        //        return strSymbol;
        //    }
        //    string[] str = strSymbol.Split(new char[] { '&', '|' }, StringSplitOptions.RemoveEmptyEntries);

        //    if (strSymbol.Contains("&&"))
        //    {

        //        foreach (string item in str)
        //        {
        //            if (symbolList.Any(x => item.ToCharArray().Contains(x)))
        //            {
        //                strbuilder.Append(item);
        //            }
        //            else
        //            {
        //                strbuilder.Append(ValidateSymbols(item.Trim(), line));


        //            }
        //            if (counter < str.Length)
        //                strbuilder.Append(" && ");
        //            counter++;
        //        }
        //        return strbuilder.ToString();
        //    }
        //    else if (strSymbol.Contains("||"))
        //    {
        //        foreach (string item in str)
        //        {
        //            if (symbolList.Any(x => item.ToCharArray().Contains(x)))
        //            {
        //                strbuilder.Append(item);
        //            }
        //            else
        //            {
        //                strbuilder.Append(ValidateSymbols(item.Trim(), line));


        //            }
        //            if (counter < str.Length)
        //                strbuilder.Append(" || ");
        //            counter++;
        //        }
        //        return strbuilder.ToString();
        //    }
        //    else
        //    {
        //        strSymbol = strSymbol.Replace(strSymbol, string.Concat(strSymbol, ">", 0));
        //        return strSymbol;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string CheckWhile(string line)
        {
            string subStr = string.Empty;
            List<char> symbolArray = new List<char>() { '(', ')' };
            if (line.StartsWith("while") && symbolArray.Any(n => line.ToCharArray().Contains(n)))
            {
                if (line.Contains("(("))
                    subStr = line.Substring(line.IndexOf('(') + 2, line.LastIndexOf(')') - line.IndexOf('(') - 1);
                else
                    subStr = line.Substring(line.IndexOf('(') + 1, line.LastIndexOf(')') - line.IndexOf('(') - 1);

                line = line.Replace(subStr, ValidateSymbols(subStr, line));
            }
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private string ValidateSymbols(string strSymbol, string line)
        {

            string subNotStr = string.Empty;
            string subStringValue = string.Empty;
            StringBuilder strbuilder = new StringBuilder();
            List<char> symbolList = new List<char>() { '=', '<', '>' };
            int counter = 1;
            bool openbracket = false;
            bool closeBracket = false;

            string[] strSymbolList = strSymbol.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);


            if (strSymbol.Contains('!') && !strSymbol.Contains("&&") && !strSymbol.Contains("||"))
            {
                if (strSymbol.Contains(')'))
                {
                    if (strSymbol[strSymbol.LastIndexOf(')') - 1].Equals(')'))
                    {
                        strSymbol = strSymbol.Substring(1, strSymbol.Length - 2);
                        closeBracket = true;
                    }
                    strSymbol = strSymbol.Replace(strSymbol,
                        closeBracket ? string.Concat(strSymbol, "!=", 0, ')') : string.Concat(strSymbol, "!=", 0));
                    closeBracket = false;
                }
                else
                {
                    subNotStr = strSymbol.Substring(1, strSymbol.Length - 1);
                    strSymbol = strSymbol.Replace(strSymbol, string.Concat(subNotStr, "!=", 0));
                }
                return strSymbol;
            }
            if (strSymbolList.Length > 0 && (strSymbol.Contains("&&") || strSymbol.Contains("||")))
            {
                string subItem = string.Empty;
                string value = string.Empty;
                foreach (var itemStr in strSymbolList)
                {
                    if (itemStr.Contains("||"))
                    {
                        string[] strOR = itemStr.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string item in strOR)
                        {
                            if (item.StartsWith("("))
                            {
                                subItem = item.Replace("(", string.Empty);
                                openbracket = true;
                            }

                            else if (item.Contains(')'))
                            {

                                if (item[item.LastIndexOf(')') - 1] != ')')
                                {
                                    subItem = item.Replace(")", string.Empty);
                                    closeBracket = true;
                                }
                                else
                                    subItem = item;
                            }
                            else
                            {
                                subItem = item;
                            }
                            if (symbolList.Any(x => item.ToCharArray().Contains(x)))
                            {
                                strbuilder.Append(item);
                            }
                            else
                            {
                                value = ValidateSymbols(subItem.Trim(), line);
                                if (openbracket)
                                    value = '(' + value;
                                else if (closeBracket)
                                    value = value + ')';

                                strbuilder.Append(value);


                            }
                            if (counter < strOR.Length)
                                strbuilder.Append(" || ");
                            counter++;
                        }
                    }

                    else
                    {
                        if (itemStr.StartsWith("("))
                        {
                            subItem = itemStr.Replace("(", string.Empty);
                            openbracket = true;
                        }
                        else if (itemStr.Contains(')'))
                        {
                            if (itemStr[itemStr.LastIndexOf(')') - 1] != ')')
                            {
                                subItem = itemStr.Replace(")", string.Empty);
                                closeBracket = true;
                            }
                            else
                                subItem = itemStr;
                        }
                        else
                        {
                            subItem = itemStr;
                        }


                        if (symbolList.Any(x => itemStr.ToCharArray().Contains(x)))
                        {
                            strbuilder.Append(string.Concat(" &&", itemStr, "&&"));
                        }
                        else
                        {
                            value = ValidateSymbols(subItem.Trim(), line);
                            if (openbracket)
                                value = '(' + value;
                            else if (closeBracket)
                                value = value + ')';
                            strbuilder.Append(string.Concat(" &&", value, "&&"));
                        }
                    }
                }

                subStringValue = strbuilder.ToString();

                if (subStringValue.Trim().StartsWith("&&"))
                {
                    subStringValue = subStringValue.Substring(3, strbuilder.ToString().Length - 3);

                }
                if (subStringValue.Trim().EndsWith("&&"))
                {
                    subStringValue = subStringValue.Substring(0, subStringValue.LastIndexOf('&') - 1);
                }
                if (subStringValue.Trim().Contains("&& &&"))
                {
                    subStringValue = subStringValue.Replace("&& &&", "&&");
                }
                return subStringValue;
            }
            else
            {
                if (strSymbol.Contains(')'))
                {
                    if (strSymbol[strSymbol.LastIndexOf(')') - 1].Equals(')'))
                    {
                        strSymbol = strSymbol.Substring(0, strSymbol.Length - 1);
                        closeBracket = true;
                    }
                }
                strSymbol = strSymbol.Replace(strSymbol,
                    closeBracket ? string.Concat(strSymbol, ">", 0, ')') : string.Concat(strSymbol, ">", 0));
                closeBracket = false;
                return strSymbol;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForFile"></param>
        public void HandleScriptIfMainNotExist(DataTable dtForFile)
        {
            try
            {
                Dictionary<string, DataTypes> variableDictionary = new Dictionary<string, DataTypes>();
                List<char> symbolArray = new List<char>() { '(', ')', '+' };
                string pattern = @"^[0-9]+$";
                const string LABEL_COMMENT = "//Append String Here";
                _objRegex = new Regex(pattern);
                string strVariableList = string.Empty;
                char[] chars = null;
                StringBuilder calval = new StringBuilder();
                bool openBracketFound = false;

                foreach (DataRow drs in dtForFile.Rows)
                {

                    string line = drs[1].ToString();

                    if (line.Equals("") || line.Equals(" "))
                    {
                        continue;
                    }

                    if (line.Contains("main"))
                    {
                        drs[1] = line + LABEL_COMMENT;

                        continue;
                    }




                    if (line.Trim().StartsWith("if") || line.Trim().StartsWith("for"))
                    {
                        //continue;
                        if (line.Trim().StartsWith("if"))
                        {
                            line = line.Substring(line.IndexOf(')'), line.Length - line.IndexOf(')') - 1);
                            //if (!symbolArray.Any(x => line.Contains(x)))
                            //{
                            if (line.Contains("="))
                            {
                                string[] strList = line.Split(new char[] { '=' },
                                    StringSplitOptions.RemoveEmptyEntries);
                                if (symbolArray.Any(x => strList[0].Contains(x)))
                                {
                                    strList[0] = strList[0].Replace('(', ' ').Replace(')', ' ').Replace('+', ' ').Trim();
                                }
                                if (!variableDictionary.Any(x => x.Key.Equals(strList[0].Trim())) &&
                                    !variableDictionary.Any(x => x.Value.TypeValue.Equals(strList[0].Trim())))
                                    if (!string.IsNullOrWhiteSpace(strList[0]))
                                    {
                                        variableDictionary.Add(strList[0].Trim(),
                                            new DataTypes()
                                            {
                                                TypeValue = strList[1],
                                                TypeName = CheckDataType(strList[1].Replace(';', ' '))
                                            });
                                    }
                                // }
                            }
                        }
                        continue;

                    }
                    else
                    {
                        if (line.Contains("="))
                        {
                            string[] strList = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (symbolArray.Any(x => strList[0].Contains(x)))
                            {
                                strList[0] = strList[0].Replace('+', ' ').Trim();
                            }
                            if (!variableDictionary.Any(x => x.Key.Equals(strList[0].Trim())) &&
                                !variableDictionary.Any(x => x.Value.TypeValue.Equals(strList[0].Trim()))
                                )
                                if (!string.IsNullOrWhiteSpace(strList[0]))
                                {
                                    variableDictionary.Add(strList[0].Trim(), new DataTypes()
                                    {
                                        TypeValue = strList[1],
                                        TypeName = CheckDataType(strList[1].Replace(';', ' '))
                                    });
                                }
                        }
                    }
                }

                StringBuilder variableList = new StringBuilder();
                strVariableList = variableDictionary.Select(x => x.Key).Last();


                foreach (var item in variableDictionary)
                {
                    variableList.Append(string.Format("{0} {1}{2}", item.Value.TypeName, item.Key,
                        item.Key.Equals(strVariableList) ? ";" : ";"));
                }


                if (
                    dtForFile.AsEnumerable()
                        .Any(x => ((string)x[1]).Contains(LABEL_COMMENT)))
                {
                    dtForFile.AsEnumerable().ToList<DataRow>().
                        ForEach(
                            m =>
                            {
                                m[1] = ((string)m[1])
                                    .Replace(LABEL_COMMENT, variableList.ToString());
                            });

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private string CheckDataType(string val)
        {
            if (_objRegex.IsMatch(val.Trim()))
            {
                return "int";
            }
            else if (val.Contains("StringLiteral") && !val.StartsWith("Table"))
            {
                return "string";
            }
            else
            {
                return "TablePtr";
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForFile"></param>
        public void ConvertForStatements(DataTable dtForFile)
        {

            string line = string.Empty;
            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (string.IsNullOrWhiteSpace(drs[1].ToString()))
                    {
                        continue;
                    }

                    line = drs[1].ToString();

                    if (line.Trim().StartsWith("for"))
                    {
                        line = line.Substring(line.IndexOf('(') + 1, line.Length - line.IndexOf('(') - 1);

                        string[] strSubItem = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (!string.IsNullOrWhiteSpace(strSubItem[0]))
                        {
                            if (!strSubItem[0].Contains("="))
                            {
                                drs[1] = drs[1].ToString().Replace(line, string.Format("{0};{1};{2}", string.Concat(strSubItem[0], "=", strSubItem[0]),
                                    strSubItem[1], strSubItem[2]));

                            }
                        }
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