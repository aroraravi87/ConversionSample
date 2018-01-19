using System;
using System.Data;

namespace AVS_To_JVS
{
    class ReadFile2
    {
        public String readFile(DataTable dtForFileOfInclude, string fileName)
        {
            LineValidator lineValidator = new LineValidator();
            Utility utility = new Utility();
            System.Data.DataRow dr = null;
            System.Data.DataRow drE = null;

            int currentMethodLineCount = 0;
            int methodParametersCount;
            int openCurlyBraces = 0;

            bool methodStartFlag = false;
            bool methodNameStartFlag = false;
            bool methodNamefoundFlag = false;
            bool multipleLineFlag = false;

            bool methodExistFlag = false;
            bool forLoopFlag = false;

            string returnLine;
            string includeList = "";
            string lastLine = "";
            string currentMethodName = "";
            string[] methodsInaline;
            string methodName;

            foreach (DataRow drs1 in dtForFileOfInclude.Rows)
            {
                returnLine = drs1[1].ToString().Trim();
                if (returnLine.Equals(""))
                {
                    continue;
                }
                
                includeList = utility.getIncludeStatement(methodStartFlag, returnLine, includeList, drs1[1].ToString());

                if (methodNameStartFlag)
                {
                    if (utility.detectMethodNameEndPointer(returnLine))
                    {
                        methodNameStartFlag = false;
                        currentMethodName = currentMethodName + utility.getMethodNameEnding(returnLine);
                        methodNamefoundFlag = true;
                    }
                    else
                    {
                        currentMethodName = currentMethodName + returnLine;
                        continue;
                    }
                }
                if (!methodNamefoundFlag && !methodStartFlag && utility.detectMethodNameStartPointer(returnLine))
                {
                    methodNameStartFlag = true;
                    
                    if (utility.detectMethodNameEndPointer(returnLine))
                    {
                        methodNameStartFlag = false;
                        currentMethodName = utility.getMethodName(returnLine);
                        methodNamefoundFlag = true;
                    }
                    else
                    {
                        currentMethodName = utility.getMethodNameTillEnd(returnLine);
                        continue;
                    }
                }
                if (methodNamefoundFlag)
                {
                    if (utility.detectMethodStartPointer(returnLine))
                    {
                        methodName = utility.methodNameWithoutParameter(currentMethodName);
                        methodParametersCount = utility.getParameterCount(currentMethodName);
                        methodExistFlag = true;
                        forLoopFlag = true;
                        foreach (System.Data.DataRow drs in dtForMethodCalled.Rows)
                        {
                            if (drs[2].Equals(methodName) && drs[4].ToString().Equals("" + methodParametersCount))
                            {
                                forLoopFlag = false;
                                if (!drs[3].Equals("") && drs[0].Equals(fileName))
                                {
                                    drs[6] = "This is a overload Method with same No of parameters";
                                    dr = InputProcessor.dtForMethodCalled.Rows.Add();
                                    dr[1] = drs[1];

                                    dr[6] = "This is a overload Method with same No of parameters";
                                    break;
                                }
                                if (!drs[3].Equals("") && !drs[0].Equals(fileName))
                                {
                                    drs[6] = "Conflicting Method";
                                    dr = InputProcessor.dtForMethodCalled.Rows.Add();
                                    dr[1] = drs[1];

                                    dr[6] = "Conflicting Method";
                                    break;

                                }
                                methodExistFlag = false;
                                drs[0] = fileName;
                                drs[3] = currentMethodName;
                                drE = drs;
                                break;
                            }

                        }
                        if (forLoopFlag)
                        {
                            dr = InputProcessor.dtForMethodCalled.Rows.Add();
                        }
                        if (methodExistFlag)
                        {
                            dr[0] = fileName;
                            dr[2] = methodName;
                            dr[3] = currentMethodName;
                            dr[4] = methodParametersCount;
                            drE = dr;


                        }
                        methodStartFlag = true;
                        methodNamefoundFlag = false;
                    }
                    else
                        if (utility.detectMethodPrototype(returnLine))
                        {
                            currentMethodName = "";
                            methodNamefoundFlag = false;
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                }
                if (multipleLineFlag)
                {
                    returnLine = lastLine + returnLine;
                    multipleLineFlag = false;
                }
                if (methodStartFlag)
                {
                    if (utility.detectMethodNameStartPointer(returnLine))
                    {
                        if (utility.detectMultipleLines(returnLine))
                        {
                            multipleLineFlag = true;
                            lastLine = returnLine;
                            continue;
                        }
                        else
                        {
                            methodsInaline = utility.getMethodNamesInALine(returnLine);

                            InputProcessor.dtForMethodCalled = utility.addMethodsToList(InputProcessor.dtForMethodCalled, methodsInaline, currentMethodName);

                        }
                    }
                    openCurlyBraces = openCurlyBraces + utility.getOpenCurlyBracescount(returnLine) - utility.getCloseCurlyBracescount(returnLine);
                    if (openCurlyBraces == 0)
                    {
                        methodStartFlag = false;
                        currentMethodName = "";
                        drE[5] = "" + (linesCount - currentMethodLineCount + 1);
                        continue;
                    }
                }
            }
            if (!includeList.Equals(""))
            {
                IncludeFileHandler includeFileHandler = new IncludeFileHandler();
                includeFileHandler.handleIncludeList(includeList, libraryPath);
            }
            return includeList;
        }
    }
}
