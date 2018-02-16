
/*****************************************************************************
File Name:              (LineValidator.mls)
+------+----------+------------+-----------------------------------------------------------------------+
| S.No.| Date     | Who        | Description                                                            |
+------+----------+------------+-----------------------------------------------------------------------+
|      | 10 Jun   | Ajey Raghav| Initial version                                                       |
+------+----------+------------+-----------------------------------------------------------------------+

Description:       

*****************************************************************************/

namespace AVSToJVSConversion.BLL
{
    /// <summary>
    /// 
    /// </summary>
    class LineValidator
    {

        #region ===[Public Members]==========================

        public static bool MultiLineCommentFlag = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string Operation10(string content)
        {

            string line;
            char[] lineArray;
            int number = 0;

            bool singleLineCommentFlag = false;
            bool slashFoundFlag = false;
            bool starFoundFlag = false;
            bool stringFoundFlag = false;
            bool escapeCharFoundFlag = false;
            bool charFoundFlag = false;

            lineArray = content.ToCharArray();

            line = string.Empty;

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
                        MultiLineCommentFlag = false;
                        continue;
                    }
                }
                if (MultiLineCommentFlag && character != '*')
                {
                    continue;
                }
                if (MultiLineCommentFlag && character == '*')
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
                        MultiLineCommentFlag = true;
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
                    line = line + "StringLiteralDetected" + number;
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
                    charFoundFlag = false;
                    continue;
                }

                if (charFoundFlag || stringFoundFlag)
                {
                    continue;
                }

                if (character == '/' && !singleLineCommentFlag && !MultiLineCommentFlag && !slashFoundFlag && !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                {
                    slashFoundFlag = true;
                    continue;
                }
                if (character == '"' && !singleLineCommentFlag && !MultiLineCommentFlag && !slashFoundFlag && !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                {
                    stringFoundFlag = true;
                    continue;
                }
                if (character == '\'' && !singleLineCommentFlag && !MultiLineCommentFlag && !slashFoundFlag && !escapeCharFoundFlag && !stringFoundFlag && !charFoundFlag)
                {
                    charFoundFlag = true;
                    continue;
                }
                line = line + character;
            }
            return line;
        } 
        #endregion
    }
}
