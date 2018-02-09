using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private Dictionary<string, DataTypes> _getVariableDictionary = null;
        private static string PrevElementType = string.Empty;
        private Regex _objRegex = null;
        private bool openSquarebracket = false;
        private bool closeSquareBracket = false;
        private bool appendflag = false;

        public class DataTypes
        {
            public string TypeValue { get; set; }
            public string TypeName { get; set; }
        }

        public Operations()
        {
            _initializeTables = new InitializeTables();
            _utility = new Utility();
            _getVariableDictionary = new Dictionary<string, DataTypes>();
            _getVariableDictionary.Add("int ", new DataTypes() { TypeName = "int", TypeValue = "0" });
            _getVariableDictionary.Add("Table ",
                new DataTypes() { TypeName = "com.olf.openjvs.Util", TypeValue = "Util.NULL_TABLE" });
            _getVariableDictionary.Add("String ", new DataTypes() { TypeName = "String", TypeValue = "\"\"" });
            _getVariableDictionary.Add("double ", new DataTypes() { TypeName = "double", TypeValue = "0d" });
            _getVariableDictionary.Add("ODateTime ",
                new DataTypes() { TypeName = "com.olf.openjvs.ODateTime", TypeValue = "Util.NULL_DATE_TIME" });
            _getVariableDictionary.Add("Instrument ",
                new DataTypes() { TypeName = "com.olf.openjvs.Instrument", TypeValue = "Util.NULL_INS_DATA" });
            _getVariableDictionary.Add("XString ",
                new DataTypes() { TypeName = "com.olf.openjvs.XString", TypeValue = "\"\"" });
            _getVariableDictionary.Add("Transaction ",
                new DataTypes() { TypeName = "com.olf.openjvs.Transaction", TypeValue = "Util.NULL_TRAN" });


            //string prevsElementType;
            //string str = CheckInitilize("int empty1,tb1=GetIntN(a,b);String tb2='test',tb234;Table tbnew,tb=new TableNew(),DT,", out prevsElementType);

        }

        private string CheckInitilize(string line, out string prevsElementType)
        {
            char[] chars;
            bool equalFlag = false;
            bool equalSpaceFlag = false;
            bool skipFlag = false;
            string ElementName = string.Empty;
            DataTypes DicOutput = new DataTypes();
            int additionCount = 0;
            string ElementType = string.Empty;
            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            bool openBracket = false;
            bool closeBracket = false;
            int currentposition = 0;
            int counter = 0;
            bool AddFlag = false;
            bool AddCounter = false;
            bool ItemFlag = false;

            chars = line.ToCharArray();
            prevsElementType = string.Empty;

            for (int i = 0; i < chars.Length; i++)
            {
                if (i < chars.Length - 1)
                {
                    if (!chars[i].Equals(' ') && !chars[i + 1].Equals('=') && !equalSpaceFlag && !skipFlag)
                    {
                        sb.Append(chars[i]);
                        continue;
                    }
                    currentposition = i;
                    _getVariableDictionary.Keys.Any(
                type => _utility.CheckVariableTypeName(string.Concat(sb.ToString(), ' '), type, out ElementType));
                }

                if (chars[i].Equals('('))
                {
                    openBracket = true;

                }

                if (chars[i].Equals(')'))
                {

                    closeBracket = true;
                }
                if (!chars[currentposition].Equals(','))
                {
                    sb1.Append(chars[currentposition]);
                }
                else if (chars[currentposition].Equals(',') && openBracket && !closeBracket)
                {
                    sb1.Append(chars[currentposition]);
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(ElementType))
                {
                    _getVariableDictionary.TryGetValue(ElementType, out DicOutput);

                    ElementName = ElementType;
                    prevsElementType = ElementName;

                    sb.Clear();
                    skipFlag = true;
                    continue;
                }
                //Table empty,tran_list = tblMasterDetails,empty1
                if (chars[i].Equals('='))
                {
                    if (chars[i + 1].Equals(' '))
                    {
                        equalSpaceFlag = true;

                    }
                    equalFlag = true;
                    AddFlag = false;
                }

                if (chars[i].Equals(','))
                {
                    if (!sb1.ToString().Contains('='))
                    {
                        AddFlag = true;
                        equalFlag = false;
                    }
                    sb1.Clear();


                    if (!equalFlag && AddFlag)
                    {
                        string str = string.Concat("=", DicOutput.TypeValue);
                        line = line.Insert(i + additionCount, str);
                        additionCount = additionCount + str.Length;
                        continue;
                    }

                    currentposition = counter;
                }
                if (chars[i].Equals(';'))
                {
                    if (!sb1.ToString().Contains('='))
                    {
                        AddFlag = true;
                        equalFlag = false;
                    }
                    sb1.Clear();
                    skipFlag = false;
                    if (!equalFlag && AddFlag)
                    {
                        string str = string.Concat("=", DicOutput.TypeValue);
                        line = line.Insert(i + additionCount, str);
                        additionCount = additionCount + str.Length;
                        continue;
                    }
                }
            }
            sb3.Append(line);
            return sb3.ToString();
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
                if (line.Contains("#INCLUDE_") || line.Contains("#include ") || line.Contains("#sdbg") ||
                    line.Contains("#SDBG"))
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
                commentSecondPart = commentSecondPart + commentFirstPart + drs[3].ToString();
                commentFirstPart = drs[2].ToString();

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
                commentSecondPart = commentSecondPart + commentFirstPart + drs[3].ToString();
                commentFirstPart = drs[2].ToString();

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
                    if (_utility.CheckAVSVariableDeclaration(line))
                    {
                        if (_utility.GetPositionOfAVSVariableTypeInLine(line) > line.LastIndexOf(";"))
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
            DataTable dtForMethodsAvailable, int containsGlobalTable)
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
                        dr[3] = containsGlobalTable;
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
                    _utility.closeSQLConnection(_conn, _dataReader);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _utility.closeSQLConnection(_conn, _dataReader);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DicOutput"></param>
        /// <param name="dtForInclude"></param>
        private void ModifyIncludeTable(DataTypes DicOutput, DataTable dtForInclude)
        {
            if (DicOutput.TypeName.Equals("int") || DicOutput.TypeName.Equals("String") ||
                DicOutput.TypeName.Equals("double"))
            {
                return;
            }

            string className = DicOutput.TypeName.Substring(DicOutput.TypeName.LastIndexOf('.') + 1,
                DicOutput.TypeName.Length - DicOutput.TypeName.LastIndexOf('.') - 1);
            string includeName = DicOutput.TypeName;

            bool includeFlag = true;
            foreach (DataRow dr in dtForInclude.Rows)
            {
                if (dr[0].ToString().Equals(className))
                {
                    includeFlag = false;
                }
            }
            if (includeFlag)
            {
                DataRow dr = dtForInclude.Rows.Add();
                dr[0] = className;
                dr[1] = includeName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForFile"></param>
        /// <param name="dtDataTable"></param>
        public void InitlializeVariables(DataTable dtForFile, DataTable dtDataTable)
        {
            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;
            string line;
            DataTypes DicOutput;
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

                    if (!methodNamefoundFlag && !methodStartFlag && _utility.DetectMethodNameStartPointer(line))
                    {

                        if (!line.Contains("=") && !line.Contains("+"))
                        {
                            methodNamefoundFlag = true;

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
                        openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(line) -
                                          _utility.GetCloseCurlyBracescount(line);
                        if (openCurlyBraces == 0)
                        {
                            methodStartFlag = false;
                        }
                        List<char> symbolArray = new List<char>() { '(', ')' };

                        if (line.Trim().StartsWith("if") || line.Trim().StartsWith("for"))
                        {
                            continue;
                        }
                        if (!line.Contains('='))
                        {
                            string[] strItemList = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (strItemList.Length > 0)
                            {
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
                                            if (!string.IsNullOrWhiteSpace(PrevElementType))
                                                ElementType = PrevElementType;
                                            _getVariableDictionary.Keys.Any(
                                                type => _utility.CheckVariableTypeName(subItem, type, out ElementType));

                                            if (!string.IsNullOrWhiteSpace(ElementType))
                                                PrevElementType = ElementType;

                                            _getVariableDictionary.TryGetValue(
                                                !string.IsNullOrWhiteSpace(ElementType) ? ElementType : PrevElementType,
                                                out DicOutput);

                                            if ((!string.IsNullOrWhiteSpace(ElementType) ||
                                                 !string.IsNullOrWhiteSpace(PrevElementType)) &&
                                                !string.IsNullOrWhiteSpace(DicOutput.TypeValue))
                                            {

                                                ModifyIncludeTable(DicOutput, dtDataTable);


                                                if (!symbolArray.Any(n => subItem.ToCharArray().Contains(n)))
                                                {
                                                    stringBuilder.Append(subItem.Insert(subItem.Length,
                                                        string.Format(" = {0}{1}", DicOutput.TypeValue,
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
                                            ModifyIncludeTable(DicOutput, dtDataTable);

                                            if (!symbolArray.Any(n => itemStr.ToCharArray().Contains(n)))
                                            {
                                                stringBuilder.Append(itemStr.Insert(itemStr.Length,
                                                    string.Format(" = {0}{1}", DicOutput.TypeValue.ToString(),
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
                                    {
                                        drs[1] = string.Concat(stringBuilder.ToString(), ';');
                                        PrevElementType = string.Empty;
                                        stringBuilder.Clear();
                                    }
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
                                if (strArray.Length > 0)
                                {
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
                                        if (!string.IsNullOrWhiteSpace(ElementType) &&
                                            !string.IsNullOrWhiteSpace(DicOutput.TypeValue))
                                        {
                                            ModifyIncludeTable(DicOutput, dtDataTable);

                                            IsChanged = true;
                                            calval1.Append(ConvertDeclaredValues(item + ';', ElementType,
                                                DicOutput.TypeValue,
                                                appendDataLabel));
                                            PrevElementType = string.Empty;
                                        }
                                    }
                                    if (IsChanged)
                                    {
                                        drs[1] = calval1;
                                        PrevElementType = string.Empty;
                                    }
                                    else
                                        drs[1] = drs[1];
                                }
                            }
                        }
                        else
                        {
                            char[] chars = line.ToCharArray();
                            bool equalFlag = false;
                            bool equalSpaceFlag = false;
                            string ElementName = string.Empty;
                            int additionCount = 0;
                            StringBuilder sb = new StringBuilder();
                            //  TablePtr tran_list = tblMasterDetails, empty;
                            for (int i = 0; i < chars.Length; i++)
                            {
                                if (chars[i].Equals(' ') && !chars[i + 1].Equals('=') && !equalSpaceFlag)
                                {
                                    sb.Append(chars[i]);
                                    continue;
                                }

                                _getVariableDictionary.Keys.Any(
                                    type => _utility.CheckVariableTypeName(sb.ToString(), type, out ElementType));

                                if (ElementType != null)
                                {
                                    ElementName = ElementType;
                                    sb.Clear();
                                }

                                if (chars[i].Equals('='))
                                {
                                    if (chars[i + 1].Equals(' '))
                                    {
                                        equalSpaceFlag = true;
                                    }
                                    equalFlag = true;
                                }

                                if (chars[i].Equals(','))
                                {
                                    if (!equalFlag)
                                    {
                                        string str = string.Concat("=", ElementType);
                                        if (chars[i].Equals(';'))
                                        {
                                            drs[1] = line.Insert(i + additionCount, str);
                                            additionCount = additionCount + str.Length;
                                        }
                                        else
                                        {
                                            drs[1] = line.Insert(i + additionCount, str);
                                            additionCount = additionCount + str.Length;
                                        }
                                    }

                                }


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
            string classNames = "";


            try
            {
                for (i = 0; i < methodsInaline.Length; i++)
                {
                    continueFlag = false;
                    if (methodsInaline[i] == null)
                    {
                        continue;
                    }
                    classNames = "";

                    methodName = _utility.MethodNameWithoutParameter(methodsInaline[i]);
                    parameterCount = _utility.GetParameterCount(methodsInaline[i]);
                    if (dtForMethodsAvailable != null)
                    {
                        methodsAvailable =
                            dtForMethodsAvailable.Select("MethodName='" + methodName + "' AND ParameterCount='" +
                                                         parameterCount + "'");
                        if (methodsAvailable.Length >= 1)
                        {
                            foreach (DataRow dataRow in methodsAvailable)
                            {
                                if (dataRow[0].ToString().Equals(fileName))
                                {
                                    classNames = "";
                                    break;
                                }
                                if (classNames.Equals(""))
                                {
                                    classNames = dataRow[0].ToString();
                                    continue;
                                }
                                classNames = classNames + "." + dataRow[0].ToString();
                            }

                            if (!classNames.Equals(""))
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
                    }
                    if (!continueFlag)
                    {
                        _query = string.Format("{0}{1}", Constants.AVSJVSYESMAPPINGSQL,
                            " and avsmethod = '" + methodName + "'");
                        _dataReader = OperationDao.ExecuteDataReader(_query, out _conn);
                        includeFlag = true;
                        while (_dataReader.Read())
                        {
                            _utility.AddInIncludeDT(dtForInclude, _dataReader.GetValue(3).ToString(), _dataReader.GetValue(1).ToString());
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
                        _utility.closeSQLConnection(_conn, _dataReader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _utility.closeSQLConnection(_conn, _dataReader);
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
                            _utility.AddInIncludeDT(dtForInclude, _dataReader.GetValue(3).ToString(), _dataReader.GetValue(1).ToString());
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
                    _utility.closeSQLConnection(_conn, _dataReader);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _utility.closeSQLConnection(_conn, _dataReader);
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

        public void ConvertConditionalStatements(DataTable dtForFile, string conditionType)
        {
            char[] lineArray;
            bool ifStartFlag = false;
            bool notFlag;
            bool conditionalOperatorFoundFlag = false;

            int bracketCount;
            int additionCount;
            bool ifandNotExistInThisLine;
            string line;
            int i = 0;

            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    ifStartFlag = false;
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }
                    ifandNotExistInThisLine = false;

                    lineArray = drs[1].ToString().ToCharArray();
                    additionCount = 0;
                    bracketCount = 0;
                    notFlag = false;
                    line = drs[1].ToString();

                    if (line.Contains(conditionType + " ") || line.Contains(conditionType + "("))
                    {
                        line = line.Replace("  ", " ").Trim();
                        bool exitWhileLoop = false;
                        int position = 0;
                        int actualPosition = 0;
                        while (!exitWhileLoop)
                        {
                            position = line.IndexOf(conditionType, actualPosition);
                            if (position == -1)
                            {
                                break;
                            }
                            actualPosition = position + 1;
                            int endIndex = position + conditionType.Length - 1;
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
                            i = position + 2;
                            ifStartFlag = true;
                            exitWhileLoop = true;
                        }
                    }

                    for (; ifStartFlag && i < lineArray.Length; i++)
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
                        }
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
            bool methodNamefoundFlag = false;
            bool methodStartFlag = false;
            int openCurlyBraces = 0;

            try
            {
                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (drs[1].Equals(""))
                    {
                        continue;
                    }

                    if (!methodStartFlag && !methodNamefoundFlag)
                    {
                        if (!_utility.CheckMain(drs[1].ToString()))
                        {
                            drs[1] = "public static " + drs[1].ToString();
                        }
                    }

                    if (!methodNamefoundFlag && !methodStartFlag && _utility.DetectMethodNameStartPointer(drs[1].ToString()))
                    {
                        bool ignorefound = false;

                        if (drs[1].ToString().IndexOf("=") > drs[1].ToString().IndexOf(")"))
                        {
                            ignorefound = true;
                        }
                        if (drs[1].ToString().IndexOf(";") > drs[1].ToString().IndexOf(")") && drs[1].ToString().IndexOf("=") == -1)
                        {
                            ignorefound = true;
                        }
                        if ((!drs[1].ToString().Contains("=") && !drs[1].ToString().Contains("+")) || ignorefound)
                        {
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GenerateOutputFile(DataTable dtForFile, DataTable dtForInclude, DataTable dtForLiterals,
            string outputPath, string outputFileName, DataTable dtForMethodsAvailable)
        {
            bool mainCheck;
            bool argtCheck = false;
            bool returntCheck = false;
            bool shouldAddGlobalTable = false;
            bool breakFlag = false;
            bool firstValidLineFlag = false;
            string line;
            string classNames;

            DataRow[] includeClassNames;

            try
            {
                includeClassNames = dtForMethodsAvailable.Select("FileName <> '" + outputFileName + "' AND ContainsGlobalTable = '" + 1 + "'");

                string fileName = outputPath + "\\" + outputFileName.Replace(".mls", "") + ".java";
                StreamWriter writer;
                writer = new System.IO.StreamWriter(fileName);

                argtCheck = _utility.CheckGlobalTable(dtForFile, "argt");
                returntCheck = _utility.CheckGlobalTable(dtForFile, "returnt");
                mainCheck = _utility.CheckMain(dtForFile);

                if (argtCheck || returntCheck)
                {
                    _utility.AddInIncludeDT(dtForInclude, "com.olf.openjvs.Table", "Table");
                    shouldAddGlobalTable = true;
                }
                _utility.AddInIncludeDT(dtForInclude, "com.olf.openjvs.OException", "OException");
                if (mainCheck)
                {
                    _utility.AddInIncludeDT(dtForInclude, "com.olf.openjvs.IContainerContext", "IContainerContext");
                    _utility.AddInIncludeDT(dtForInclude, "com.olf.openjvs.IScript", "IScript");
                }
                foreach (DataRow drs in dtForInclude.Rows)
                {
                    writer.Write("import " + drs[1] + ";\n");
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

                    if (drs[1].ToString().Equals("") && drs[2].ToString().Equals("") && drs[3].ToString().Equals(""))
                    {
                        continue;
                    }

                    if (!drs[1].ToString().Equals(""))
                    {
                        breakFlag = true;
                    }
                    writer.Write(drs[2] + "" + drs[3] + "\n");
                }

                if (mainCheck)
                {
                    writer.Write("\npublic class " + outputFileName.Replace(".mls", "") + " implements IScript" + "\n{\n");
                }
                else
                {
                    writer.Write("\npublic class " + outputFileName.Replace(".mls", ""));
                    writer.Write("\n{\n");
                    writer.Write("\npublic " + outputFileName.Replace(".mls", "") + "(IContainerContext context) throws OException\n{");
                    writer.Write("\nargt=context.getArgumentsTable();");
                    writer.Write("\nreturnt=context.getReturnTable();\n");
                    writer.Write("}\n");
                }

                if (argtCheck)
                {
                    writer.Write("public static Table argt;\n");
                }
                if (returntCheck)
                {
                    writer.Write("public static Table returnt;\n");
                }

                foreach (DataRow drs in dtForFile.Rows)
                {
                    if (!firstValidLineFlag && drs[1].Equals(""))
                    {
                        continue;
                    }
                    if (mainCheck && drs[1].ToString().Contains("void ") && _utility.CheckMain(drs[1].ToString()))
                    {
                        line = drs[1].ToString();
                        drs[1] = line.Substring(0, line.IndexOf("void ")) +
                                 "public void execute(IContainerContext context) " +
                                 line.Substring(line.IndexOf(")") + 1);
                    }
                    if (mainCheck && drs[1].ToString().Contains("int ") && _utility.CheckMain(drs[1].ToString()))
                    {
                        line = drs[1].ToString();
                        drs[1] = line.Substring(0, line.IndexOf("int ")) +
                                 "public void execute(IContainerContext context) " +
                                 line.Substring(line.IndexOf(")") + 1);
                    }

                    if (mainCheck && drs[1].ToString().Contains("{"))
                    {
                        int indexOfBracket = drs[1].ToString().IndexOf("{");
                        foreach (DataRow dataRow in includeClassNames)
                        {
                            drs[1] = drs[1].ToString().Substring(0, indexOfBracket + 1) +
                                     "\n" + dataRow[0].ToString() + " " + dataRow[0].ToString() + "= new " + dataRow[0].ToString() + ";\n" +
                                     drs[1].ToString().Substring(indexOfBracket + 1);
                        }
                    }

                    if (shouldAddGlobalTable && mainCheck && drs[1].ToString().Contains("{"))
                    {
                        int indexOfBracket = drs[1].ToString().IndexOf("{");
                        if (argtCheck && !returntCheck)
                        {
                            drs[1] = drs[1].ToString().Substring(0, indexOfBracket + 1) +
                                     "\nargt=context.getArgumentsTable();\n" +
                                     drs[1].ToString().Substring(indexOfBracket + 1);
                        }
                        if (returntCheck && !argtCheck)
                        {
                            drs[1] = drs[1].ToString().Substring(0, indexOfBracket + 1) +
                                     "\nreturnt=context.getReturnTable();\n" +
                                     drs[1].ToString().Substring(indexOfBracket + 1);
                        }
                        if (argtCheck && returntCheck)
                        {
                            drs[1] = drs[1].ToString().Substring(0, indexOfBracket + 1) +
                                     "\nargt=context.getArgumentsTable();" +
                                     "\nreturnt=context.getReturnTable();\n" +
                                     drs[1].ToString().Substring(indexOfBracket + 1);
                        }
                        shouldAddGlobalTable = false;
                    }
                    if (drs[1].ToString().Contains("StringLiteralDetected"))
                    {
                        ReplaceLiteral(drs, dtForLiterals);
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


                _query = Constants.ENUMSQL + wordsInFile + ")";

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
                                    _utility.AddInIncludeDT(dtForInclude, _dataReader.GetValue(2).ToString(),
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
                                    _utility.AddInIncludeDT(dtForInclude, _dataReader.GetValue(2).ToString(),
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _utility.closeSQLConnection(_conn, _dataReader);
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
                    if (line.IndexOf(";") > line.IndexOf(")") && line.IndexOf("=") == -1)
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
                const string LABEL_COMMENT = "//Insert Text Here";
                _objRegex = new Regex(pattern);
                string strVariableList = string.Empty;
                char[] chars = null;
                StringBuilder calval = new StringBuilder();
                bool openBracketFound = false;
                bool closBracketFound = false;
                bool ifstartFlag = false;
                int position = 0;
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
                            if (line.Contains(')'))
                            {

                                line = line.Substring(line.LastIndexOf(')'), line.Length - line.LastIndexOf(')') - 1);
                            }
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
                if (variableDictionary.Count > 0)
                {
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
                    else
                    {
                        dtForFile.AsEnumerable().ToList<DataRow>().
                            ForEach(
                                m =>
                                {
                                    m[1] = ((string)m[1])
                                        .Replace(LABEL_COMMENT, string.Empty);
                                });

                    }
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
            val = val.Trim();

            if (val.Equals("TRUE") || val.Equals("FALSE"))
                val = val == "TRUE" ? "1" : "0";

            if (_objRegex.IsMatch(val.Trim()) || val.Contains("GetIntN(") || val.Contains("GetIntN ("))
            {
                return "int";
            }
            else if (val.Contains("GetStringN(") || val.Contains("GetStringN (") ||
                     val.StartsWith("StringLiteral", StringComparison.OrdinalIgnoreCase))
            {
                return "string";
            }
            else if (val.Contains("GetDoubleN(") || val.Contains("GetDoubleN ("))
            {
                return "double";
            }

            else if (val.Contains("GetDateTimeN(") || val.Contains("GetDateTimeN ("))
            {
                return "ODateTime";
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

                    if (line.Trim().StartsWith("for(") || line.Trim().StartsWith("for ("))
                    {
                        line = line.Substring(line.IndexOf('(') + 1, line.Length - line.IndexOf('(') - 1);

                        string[] strSubItem = line.Split(new char[] { ';' });

                        if (strSubItem.Length > 0)
                        {
                            if (!string.IsNullOrWhiteSpace(strSubItem[0]))
                            {
                                if (!strSubItem[0].Contains("="))
                                {
                                    drs[1] = drs[1].ToString()
                                        .Replace(line,
                                            string.Format("{0};{1};{2}",
                                                string.Concat(strSubItem[0], "=", strSubItem[0]),
                                                strSubItem[1], strSubItem[2]));
                                }
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

        public void InitializeVariables(DataTable dtForFile, DataTable dtForInclude)
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
                        if (!drs[1].ToString().Contains("=") && !drs[1].ToString().Contains("+"))
                        {
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
                        if (_utility.CheckVariableDeclaration(drs[1].ToString()))
                        {
                            _dataReader = OperationDao.ExecuteDataReader(Constants.INITIALIZEVARIABLESQL, out _conn);
                            while (_dataReader.Read())
                            {
                                int positionOfVariableType;
                                string line = drs[1].ToString();

                                while (_utility.CheckVariableType(line, _dataReader.GetValue(0).ToString() + " "))
                                {
                                    positionOfVariableType = _utility.GetVariableTypePosition(line,
                                        _dataReader.GetValue(0).ToString() + " ");
                                    if (drs[1].ToString().Substring(positionOfVariableType).Count(x => x == '(') ==
                                        drs[1].ToString().Substring(positionOfVariableType).Count(x => x == ')'))
                                    {
                                        if (positionOfVariableType == 0 && !drs[1].ToString().Contains(";"))
                                        {
                                            break;
                                        }
                                        if (positionOfVariableType == 0 && drs[1].ToString().Contains("("))
                                        {
                                            string subLine = drs[1].ToString().Substring(0, line.IndexOf("("));
                                            if (!subLine.Contains(";") && !subLine.Contains("="))
                                            {
                                                break;
                                            }
                                        }
                                        line = drs[1].ToString().Substring(0, positionOfVariableType);
                                        bool openBracketFound = false;
                                        bool equalFound = false;
                                        int openBracketCount = 0;
                                        char[] lineArray =
                                            drs[1].ToString()
                                                .Substring(positionOfVariableType,
                                                    drs[1].ToString().IndexOf(";", positionOfVariableType) + 1 - positionOfVariableType)
                                                .ToCharArray();
                                        foreach (char character in lineArray)
                                        {
                                            if (openBracketFound)
                                            {
                                                if (character == '(')
                                                    openBracketCount++;
                                                if (character == ')')
                                                {
                                                    openBracketCount--;
                                                    if (openBracketCount == 0)
                                                    {
                                                        openBracketFound = false;
                                                    }
                                                }
                                                line = line + character;
                                                continue;
                                            }
                                            if (equalFound)
                                            {
                                                if (character == '(')
                                                {
                                                    openBracketCount++;
                                                    openBracketFound = true;
                                                    line = line + character;
                                                    continue;
                                                }
                                                if (character == ',')
                                                {
                                                    equalFound = false;
                                                    line = line + character;
                                                    continue;
                                                }
                                                line = line + character;
                                                continue;
                                            }
                                            if (character == '=')
                                            {
                                                line = line + character;
                                                equalFound = true;
                                                continue;
                                            }
                                            if (character == ',' || character == ';')
                                            {
                                                line = line + "=" + _dataReader.GetValue(1).ToString();
                                                if (!_dataReader.GetValue(2).ToString().Equals(""))
                                                {
                                                    _utility.AddInIncludeDT(dtForInclude,
                                                        _dataReader.GetValue(3).ToString(),
                                                        _dataReader.GetValue(2).ToString());
                                                }
                                                line = line + character;
                                                continue;
                                            }
                                            line = line + character;
                                        }
                                        drs[1] = line;
                                    }
                                    line = line.Substring(0, positionOfVariableType);
                                }
                            }
                            _utility.closeSQLConnection(_conn, _dataReader);
                        }

                        openCurlyBraces = openCurlyBraces + _utility.GetOpenCurlyBracescount(drs[1].ToString()) -
                                          _utility.GetCloseCurlyBracescount(drs[1].ToString());
                        if (openCurlyBraces == 0)
                        {
                            methodStartFlag = false;
                        }
                    }
                }
            }
            finally
            {
                _utility.closeSQLConnection(_conn, _dataReader);
            }
        }
    }
}