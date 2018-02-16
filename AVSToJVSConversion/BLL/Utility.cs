
/*****************************************************************************
File Name:              (AVS_To_JVS.mls)
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
    using System.Data.SqlClient;
    using Common;
    using DLL;

    internal class Utility
    {
       
        #region ==[Private Members ]======================
        
        private SqlConnection _conn = null;
        private SqlDataReader _dataReader = null; 
       
        #endregion

        #region ==[Public Members]========================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodStartFlag"></param>
        /// <param name="returnLine"></param>
        /// <param name="includeList"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public string GetIncludeStatement(bool methodStartFlag, string returnLine, string includeList, string line)
        {
            if (!methodStartFlag && DetectIncludeStatement(returnLine))
            {
                if (includeList.Equals(""))
                {
                    includeList = GetIncludeFileName(line.Trim());
                }
                else
                {
                    includeList = includeList + "," + GetIncludeFileName(line.Trim());
                }
            }
            return includeList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectIncludeStatement(string line)
        {
            if (line.Contains("#include"))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string GetIncludeFileName(string line)
        {
            string includeFileName;
            int indexOfFirst;
            int indexOfSecond;

            includeFileName = line.Replace("#include", "").Trim();

            indexOfFirst = includeFileName.IndexOf("\"");
            indexOfSecond = includeFileName.IndexOf("\"", indexOfFirst + 1);

            includeFileName = includeFileName.Substring(indexOfFirst + 1, indexOfSecond - (indexOfFirst + 1));

            return includeFileName;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectMethodNameStartPointer(string line)
        {
            if (line.Contains("("))
            {
                return true;
            }
            return false;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectMethodNameEndPointer(string line)
        {
            if (line.Contains(")"))
            {
                return true;
            }
            return false;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="line"></param>
       /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string GetMethodNameTillEnd(string line)
        {
            string methodName = "";
            char[] splittedLine = line.ToCharArray();

            int i;
            int methodNameStartPosition;

            methodNameStartPosition = line.IndexOf("(");
            for (i = methodNameStartPosition - 1; i >= 0; i--)
            {
                if (splittedLine[i].Equals(" "))
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

            methodName = line.Substring(methodNameStartPosition);

            return methodName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string GetMethodNameEnding(string line)
        {
            string methodName = "";
            int methodNameEndPosition;

            methodNameEndPosition = line.IndexOf(")");
            methodName = line.Substring(0, methodNameEndPosition + 1 - 0);

            return methodName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectMethodStartPointer(string line)
        {
            if (line.Contains("{"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectMethodPrototype(string line)
        {
            if (line.Contains(";"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectMethodEndPointer(string line)
        {
            if (line.Contains("}"))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetOpenCurlyBracescount(string line)
        {
            int count = 0;
            if (!line.Contains("{"))
            {
                return 0;
            }
            for (int i = 0; i < line.Length; i++)
            {
                if (line.ToCharArray()[i].Equals('{'))
                {
                    count++;
                }
            }
            return count;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetCloseCurlyBracescount(string line)
        {
            int count = 0;
            if (!line.Contains("}"))
            {
                return 0;
            }
            for (int i = 0; i < line.Length; i++)
            {
                if (line.ToCharArray()[i].Equals('}'))
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetSmallBracketOpenCount(string line)
        {
            int count = 0;
            if (!line.Contains("("))
            {
                return 0;
            }

            for (int i = 0; i < line.Length; i++)
            {
                if (line.ToCharArray()[i].Equals('('))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetSmallBracketCloseCount(string line)
        {
            int count = 0;
            if (!line.Contains(")"))
            {
                return 0;
            }
            for (int i = 0; i < line.Length; i++)
            {
                if (line.ToCharArray()[i].Equals(')'))
                {
                    count++;
                }
            }

            return count;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetParameterCount(string line)
        {
            int parametersCount;
            int openSmallBracket;
            int closeSmallBracket;
            string parameters;

            openSmallBracket = line.IndexOf("(");
            closeSmallBracket = line.IndexOf(")");

            parameters = line.Substring(openSmallBracket + 1, closeSmallBracket - (openSmallBracket + 1)).Trim();

            if (parameters.Equals(""))
            {
                parametersCount = 0;
            }
            else
            {
                if (parameters.Contains(","))
                {
                    parametersCount = parameters.Split(',').Length;
                }
                else
                {
                    parametersCount = 1;
                }
            }
            return parametersCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool DetectMultipleLines(string line)
        {
            int smallBracketOpenCount;
            int smallBracketcloseCount;

            smallBracketOpenCount = GetSmallBracketOpenCount(line);
            smallBracketcloseCount = GetSmallBracketCloseCount(line);

            if (smallBracketOpenCount > smallBracketcloseCount)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string[] GetMethodNamesInALine(string line)
        {
            int smallBracketOpen;
            int smallBracketclose;
            int smallBracketOpenCount;
            string[] methods;
            char[] splittedLine;
            int i;
            int j;
            bool emptySmallBracketFound;

            smallBracketOpenCount = GetSmallBracketOpenCount(line);

            methods = new string[smallBracketOpenCount];

            for (i = 0; i < smallBracketOpenCount; i++)
            {
                emptySmallBracketFound = false;
                splittedLine = line.ToCharArray();

                smallBracketOpen = line.LastIndexOf("(");
                smallBracketclose = line.IndexOf(")", smallBracketOpen);

                for (j = smallBracketOpen - 1; j >= 0; j--)
                {
                    if (!char.IsLetterOrDigit(splittedLine[j]) && splittedLine[j] != '_')
                    {
                        if (line.Substring(j + 1, smallBracketOpen - (j + 1)).Trim().Equals(""))
                        {
                            if (splittedLine[j] == ' ')
                            {
                                continue;
                            }
                            else
                            {
                                line = line.Substring(0, j + 1 - 0) + "a" +
                                       line.Substring(smallBracketclose + 1, line.Length - (smallBracketclose + 1));
                                methods[i] = null;
                                emptySmallBracketFound = true;
                                break;
                            }
                        }
                        break;
                    }
                }
                if (!emptySmallBracketFound)
                {
                    methods[i] = line.Substring(j + 1, smallBracketclose + 1 - (j + 1));
                    line = line.Substring(0, j + 1 - 0) + "a" +
                           line.Substring(smallBracketclose + 1, line.Length - (smallBracketclose + 1));
                }
            }

            return methods;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public string MethodNameWithoutParameter(string methodName)
        {
            return methodName.Substring(0, methodName.IndexOf("(")).Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public int GetCount(string line, string keyword)
        {
            int count = 0;

            if (!line.Contains(keyword))
            {
                return 0;
            }

            do
            {
                count++;
                line = line.Substring(line.IndexOf(keyword) + keyword.Length);
            } while (line.Contains(keyword));

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool CheckVariableDeclaration(string line)
        {
            try
            {
                _dataReader = OperationDao.ExecuteDataReader(Constants.JVSVARIABLELISTSQL, out _conn);
                while (_dataReader.Read())
                {
                    if (CheckVariableType(line, _dataReader.GetValue(0).ToString() + " "))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                closeSQLConnection(_conn, _dataReader);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool CheckAVSVariableDeclaration(string line)
        {
            try
            {
                _dataReader = OperationDao.ExecuteDataReader(Constants.AVSVARIABLELISTSQL, out _conn);
                while (_dataReader.Read())
                {
                    if (CheckVariableType(line, _dataReader.GetValue(0).ToString() + " "))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                closeSQLConnection(_conn, _dataReader);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="variableType"></param>
        /// <returns></returns>
        public bool CheckVariableType(string line, string variableType)
        {
            int position = -1;
            int count = 0;

            if (line.Contains(variableType))
            {
                count = GetCount(line, variableType);

                for (int i = 0; i < count; i++)
                {
                    position = line.IndexOf(variableType);
                    if (position == 0)
                    {
                        return true;
                    }
                    else
                    {
                        if (!char.IsLetterOrDigit(line.ToCharArray()[position - 1]) &&
                            line.ToCharArray()[position - 1] != '_')
                        {
                            return true;
                        }
                    }
                    line = line.Substring(position + variableType.Length);
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetPositionOfVariableTypeInLine(string line)
        {
            int largest = -1;
            try
            {
                _dataReader = OperationDao.ExecuteDataReader(Constants.JVSVARIABLELISTSQL, out _conn);
                while (_dataReader.Read())
                {
                    if (GetVariableTypePosition(line, _dataReader.GetValue(0).ToString() + " ") > largest)
                    {
                        largest = GetVariableTypePosition(line, _dataReader.GetValue(0).ToString() + " ");
                    }
                }
            }
            finally
            {
                closeSQLConnection(_conn, _dataReader);
            }
            return largest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int GetPositionOfAVSVariableTypeInLine(string line)
        {
            int largest = -1;
            try
            {
                _dataReader = OperationDao.ExecuteDataReader(Constants.AVSVARIABLELISTSQL, out _conn);
                while (_dataReader.Read())
                {
                    if (GetVariableTypePosition(line, _dataReader.GetValue(0).ToString() + " ") > largest)
                    {
                        largest = GetVariableTypePosition(line, _dataReader.GetValue(0).ToString() + " ");
                    }
                }
            }
            finally
            {
                closeSQLConnection(_conn, _dataReader);
            }
            return largest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="variableType"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool CheckVariableTypeName(string line, string variableType, out string element)
        {

            int position = -1;
            int count = 0;

            if (line.Contains(variableType))
            {
                count = GetCount(line, variableType);

                for (int i = 0; i < count; i++)
                {
                    position = line.IndexOf(variableType);
                    if (position == 0)
                    {
                        element = variableType;
                        return true;
                    }
                    else
                    {
                        if (!Char.IsLetterOrDigit(line.ToCharArray()[position - 1]) &&
                            line.ToCharArray()[position - 1] != '_')
                        {
                            element = variableType;
                            return true;
                        }
                    }
                    line = line.Substring(position + variableType.Length);
                }
            }
            element = string.Empty;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="variableType"></param>
        /// <returns></returns>
        public int GetVariableTypePosition(string line, string variableType)
        {
            int position = -1;
            int count = 0;

            if (line.Contains(variableType))
            {
                count = GetCount(line, variableType);

                for (int i = 0; i < count; i++)
                {
                    position = line.LastIndexOf(variableType);
                    if (position == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        if (!char.IsLetterOrDigit(line.ToCharArray()[position - 1]) &&
                            line.ToCharArray()[position - 1] != '_')
                        {
                            return position;
                        }
                    }
                    line = line.Substring(0, position);
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="variableList"></param>
        /// <returns></returns>
        public string GetVariableList(string line, string variableList)
        {
            try
            {
                _dataReader = OperationDao.ExecuteDataReader(Constants.JVSVARIABLELISTSQL, out _conn);
                while (_dataReader.Read())
                {
                    variableList = GetVariableListOfGivenType(line, variableList,
                        _dataReader.GetValue(0).ToString() + " ");
                }
            }
            finally
            {
                closeSQLConnection(_conn, _dataReader);
            }
            return variableList;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="dtForVariables"></param>
        /// <param name="methodName"></param>
        /// <param name="fileName"></param>
        public void GetVariableList(string line, DataTable dtForVariables, string methodName, string fileName)
        {
            string variableList = "";
            DataRow dr;
            try
            {
                _dataReader = OperationDao.ExecuteDataReader(Constants.JVSVARIABLELISTSQL, out _conn);
                while (_dataReader.Read())
                {
                    variableList = GetVariableListOfGivenType(line, variableList,
                        _dataReader.GetValue(0) + " ");
                    variableList = CorrectTheVariableList(variableList);
                    if (!variableList.Equals(""))
                    {
                        for (int i = 0; i < GetCount(variableList, ",") + 1; i++)
                        {
                            dr = dtForVariables.Rows.Add();
                            dr[0] = fileName;
                            dr[1] = methodName;
                            dr[2] = _dataReader.GetValue(0).ToString();
                            dr[3] = variableList.Split(',')[i];
                        }
                        variableList = "";
                    }
                }
            }
            finally
            {
                closeSQLConnection(_conn, _dataReader);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="variableList"></param>
        /// <param name="variableType"></param>
        /// <returns></returns>
        private string GetVariableListOfGivenType(string line, string variableList, string variableType)
        {
            int positionOfVariableType = -1;
            int positionOfvariabletypeEnding = -1;
            int count = 0;

            if (CheckVariableType(line, variableType))
            {
                count = GetCount(line, variableType);

                for (int i = 0; i < count; i++)
                {
                    positionOfVariableType = line.IndexOf(variableType);
                    if (positionOfVariableType == 0)
                    {
                        positionOfvariabletypeEnding = line.IndexOf(";");
                        if (positionOfvariabletypeEnding == -1)
                        {
                            if (line.Trim().ToCharArray()[line.Trim().Length - 1] == ',')
                            {
                                positionOfvariabletypeEnding = line.LastIndexOf(",");
                            }
                            else
                            {
                                positionOfvariabletypeEnding = line.Length - 1;
                            }
                        }
                        if (variableList.Equals(""))
                        {
                            variableList = line.Substring(positionOfVariableType + variableType.Length,
                                positionOfvariabletypeEnding - (positionOfVariableType + variableType.Length));
                        }
                        else
                        {
                            variableList = variableList + "," +
                                           line.Substring(positionOfVariableType + variableType.Length,
                                               positionOfvariabletypeEnding -
                                               (positionOfVariableType + variableType.Length));
                        }
                    }
                    else
                    {
                        if (!char.IsLetterOrDigit(line.ToCharArray()[positionOfVariableType - 1]) &&
                            line.ToCharArray()[positionOfVariableType - 1] != '_')
                        {
                            positionOfvariabletypeEnding = line.IndexOf(";", positionOfVariableType);
                            if (positionOfvariabletypeEnding == -1)
                            {
                                if (line.Trim().ToCharArray()[line.Trim().Length - 1] == ',')
                                {
                                    positionOfvariabletypeEnding = line.LastIndexOf(",");
                                }
                                else
                                {
                                    positionOfvariabletypeEnding = line.Length - 1;
                                }
                            }
                            if (variableList.Equals(""))
                            {
                                variableList = line.Substring(positionOfVariableType + variableType.Length,
                                    positionOfvariabletypeEnding - (positionOfVariableType + variableType.Length));
                            }
                            else
                            {
                                variableList = variableList + "," +
                                               line.Substring(positionOfVariableType + variableType.Length,
                                                   positionOfvariabletypeEnding -
                                                   (positionOfVariableType + variableType.Length));
                            }
                        }
                    }
                    if (positionOfvariabletypeEnding == line.Length - 1)
                    {
                        line = "";
                    }
                    else
                    {
                        line = line.Substring(positionOfvariabletypeEnding + 1);
                    }
                    if (line.Trim().Equals(""))
                    {
                        break;
                    }
                }
            }
            return variableList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="startindex"></param>
        /// <param name="excudeMethods"></param>
        /// <returns></returns>
        public bool CheckWhetherAppendShouldBeAddedOrNot(string line, int startindex, string excudeMethods)
        {
            int methodEndPosition;
            bool methodEndFound = false;
            string methodName = "";
            bool rejectMethodEndFound = false;
            int countOfMethodRejectPoint = 0;


            if (excudeMethods.Equals(""))
            {
                return true;
            }
            for (int i = startindex - 1; i >= 0; i--)
            {
                if (methodEndFound)
                {
                    if (char.IsLetterOrDigit(line.ToCharArray()[i]))
                    {
                        methodName = line.ToCharArray()[i] + methodName;
                        continue;
                    }
                    else if (line.ToCharArray()[i] == ' ' || line.ToCharArray()[i] == '(')
                    {
                        if (methodName.Equals(""))
                        {
                            continue;
                        }
                        break;
                    }
                    else
                    {
                        if (methodName.Equals(""))
                        {
                            methodEndFound = false;
                        }
                        else
                        {
                            break;
                        }
                    }
                }


                if (!methodEndFound && line.ToCharArray()[i] == ')')
                {
                    rejectMethodEndFound = true;
                    countOfMethodRejectPoint++;
                }

                if (rejectMethodEndFound && line.ToCharArray()[i] == '(')
                {
                    countOfMethodRejectPoint--;
                    if (countOfMethodRejectPoint == 0)
                    {
                        rejectMethodEndFound = false;
                    }
                    continue;
                }

                if (!rejectMethodEndFound && line.ToCharArray()[i] == '(')
                {
                    methodEndPosition = i - 1;
                    methodEndFound = true;
                }
                else
                {
                    continue;
                }
            }
            if (!methodName.Equals("") && excudeMethods.Contains(methodName))
            {
                if (!excudeMethods.Contains(","))
                {
                    if (excudeMethods.Equals(methodName))
                    {
                        return false;
                    }
                }
                string[] excludeMethod = excudeMethods.Split(',');
                for (int i = 0; i < excludeMethod.Length; i++)
                {
                    if (excludeMethod[i].Trim().Equals(methodName))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return true;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForVariables"></param>
        /// <param name="enumName"></param>
        /// <param name="methodName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool CheckInVariableList(DataTable dtForVariables, string enumName, string methodName, string fileName)
        {
            foreach (DataRow drs in dtForVariables.Rows)
            {
                if (drs[1].ToString().Equals("") ||
                    (drs[1].ToString().Equals(methodName) && drs[0].ToString().Equals(fileName)))
                {
                    if (drs[3].ToString().Trim().Equals(enumName))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForVariables"></param>
        /// <param name="drs"></param>
        /// <param name="methodName"></param>
        /// <param name="fileName"></param>
        public void referVariableFromLibraryInLine(DataTable dtForVariables, DataRow drs, string methodName, string fileName)
        {
            DataRow[] datarows = dtForVariables.Select("MethodName='' AND FileName<>'" +
                                                         fileName + "'");
            foreach (DataRow dr in datarows)
            {
                if (drs[1].ToString().Contains(dr[3].ToString()))
                {
                    DataRow[] datarows2 = dtForVariables.Select("VariableName='" + dr[3].ToString() + "' AND FileName='" +
                                                         fileName + "' AND MethodName IN ('','" + methodName + "')");
                    if (datarows2.Length == 0)
                    {
                        int position = 0;
                        int startIndex = 0;
                        int endIndex;
                        while (drs[1].ToString().IndexOf(dr[3].ToString(), position) >= 0)
                        {
                            position = drs[1].ToString().IndexOf(dr[3].ToString(), position);

                            startIndex = drs[1].ToString().IndexOf(dr[3].ToString(), position);
                            endIndex = startIndex + dr[3].ToString().Length - 1;
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

                            drs[1] = drs[1].ToString().Substring(0, startIndex) +
                                     dr[0].ToString().Replace(".mls", "") + "." + dr[3].ToString() +
                                     drs[1].ToString().Substring(endIndex + 1);

                            position = position + dr[3].ToString().Length + 2;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForInclude"></param>
        /// <param name="include"></param>
        /// <param name="className"></param>
        public void AddInIncludeDT(DataTable dtForInclude, string include, string className)
        {
            bool includeFlag = true;
            foreach (DataRow drs in dtForInclude.Rows)
            {
                if (drs[0].ToString().Equals(className))
                {
                    includeFlag = false;
                }
            }
            if (includeFlag)
            {
                DataRow drs = dtForInclude.Rows.Add();
                drs[0] = className;
                drs[1] = include;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableList"></param>
        /// <returns></returns>
        public string CorrectTheVariableList(string variableList)
        {
            int openBracketCount = 0;
            bool equalFound = false;
            bool openBracketFound = false;
            string returnList = "";
            char[] chars;
            if (variableList.Trim().Equals(""))
            {
                return "";
            }
            chars = variableList.ToCharArray();
            foreach (char character in chars)
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
                    continue;
                }

                if (equalFound)
                {
                    if (character == '(')
                    {
                        openBracketCount++;
                        openBracketFound = true;
                        continue;
                    }
                    if (character == ',')
                    {
                        equalFound = false;
                        returnList = returnList + character;
                        continue;
                    }
                }

                if (!equalFound && !openBracketFound && character != '=' && character != ' ')
                {
                    returnList = returnList + character;
                    continue;
                }

                if (!equalFound && !openBracketFound && character == '=')
                {
                    equalFound = true;
                }
            }
            return returnList;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetCustomLogAppender()
        {
            string path = String.Concat("LogFile_", DateTime.Now.Day
                , DateTime.Now.Month
                , DateTime.Now.Year, "_", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, ".log");



            log4net.Repository.ILoggerRepository repository = log4net.LogManager.GetRepository();
            foreach (log4net.Appender.IAppender appender in repository.GetAppenders())
            {
                if (appender.Name.CompareTo("RollingFileAppender") == 0 && appender is log4net.Appender.FileAppender)
                {
                    log4net.Appender.FileAppender fileAppender = (log4net.Appender.FileAppender)appender;
                    fileAppender.File = System.IO.Path.Combine(fileAppender.File, path);

                    fileAppender.ActivateOptions();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        public void destroyDT(DataTable dataTable)
        {
            try
            {
                if (dataTable != null)
                    dataTable.Dispose();
                dataTable = null;
            }
            catch (Exception e)
            {
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForFile"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool CheckMain(string line)
        {
            if (line.Contains(" main()") ||
                (line.Contains(" main ") && line.Contains(" ()")))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtForFile"></param>
        /// <param name="globalTableName"></param>
        /// <returns></returns>
        public bool CheckGlobalTable(DataTable dtForFile, string globalTableName)
        {
            foreach (DataRow drs in dtForFile.Rows)
            {
                if (drs[1].ToString().Contains(globalTableName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_conn"></param>
        /// <param name="_dataReader"></param>
        public void closeSQLConnection(SqlConnection _conn, SqlDataReader _dataReader)
        {
            try
            {
                _dataReader.Close();
                DbConnection.CloseSqlConnection(_conn);
            }
            catch (Exception e)
            {

            }
        } 
        #endregion
    }
}
