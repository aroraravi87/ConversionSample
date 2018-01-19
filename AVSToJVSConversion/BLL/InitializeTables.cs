using System.Data;

/*****************************************************************************
File Name:              (InitializeTables.mls)
+------+----------+------------+-----------------------------------------------------------------------+
| S.No.| Date     | Who        | Description                                                            |
+------+----------+------------+-----------------------------------------------------------------------+
|      | 10 Jun   | Ajey Raghav| Initial version                                                       |
+------+----------+------------+-----------------------------------------------------------------------+

Description:       

*****************************************************************************/

namespace AVSToJVSConversion.BLL
{
    class InitializeTables
    {
        public DataTable GetDtForFile()
        {
            DataTable dtForFile;
            DataColumn dc;

            dtForFile = new DataTable();

            dc = new DataColumn("Line", typeof(System.String));
            dc.DefaultValue = "";
            dtForFile.Columns.Add(dc);

            dc = new DataColumn("lineWithoutAnyCommentAndLiteral", typeof(System.String));
            dc.DefaultValue = "";
            dtForFile.Columns.Add(dc);

            dc = new DataColumn("PriorCommentedPart", typeof(System.String));
            dc.DefaultValue = "";
            dtForFile.Columns.Add(dc);

            dc = new DataColumn("LaterCommentedPart", typeof(System.String));
            dc.DefaultValue = "";
            dtForFile.Columns.Add(dc);

            return dtForFile;
        }


        public DataTable GetDtForLiterals()
        {
            DataTable dtForLiterals;
            DataColumn dc;

            dtForLiterals = new DataTable();

            dc = new DataColumn("literalName", typeof(System.String));
            dc.DefaultValue = "";
            dtForLiterals.Columns.Add(dc);

            dc = new DataColumn("literalValue", typeof(System.String));
            dc.DefaultValue = "";
            dtForLiterals.Columns.Add(dc);

            return dtForLiterals;
        }

        public DataTable GetDtForMethodPresent()
        {
            DataTable dtForMethodsAvailable;
            DataColumn dc;

            dtForMethodsAvailable = new DataTable();

            dc = new DataColumn("FileName", typeof(System.String));
            dc.DefaultValue = "";
            dtForMethodsAvailable.Columns.Add(dc);

            dc = new DataColumn("MethodName", typeof(System.String));
            dc.DefaultValue = "";
            dtForMethodsAvailable.Columns.Add(dc);

            dc = new DataColumn("ParameterCount", typeof(System.Int16));
            dtForMethodsAvailable.Columns.Add(dc);

            return dtForMethodsAvailable;
        }

        public DataTable GetDtForInclude()
        {
            DataTable dtForInclude;
            DataColumn dc;

            dtForInclude = new DataTable();

            dc = new DataColumn("ClassName", typeof(System.String));
            dc.DefaultValue = "";
            dtForInclude.Columns.Add(dc);

            dc = new DataColumn("IncludeName", typeof(System.String));
            dc.DefaultValue = "";
            dtForInclude.Columns.Add(dc);

            return dtForInclude;
        }

        public DataTable GetDtForVariables()
        {
            DataTable dtForVariables;
            DataColumn dc;

            dtForVariables = new DataTable();

            dc = new DataColumn("FileName", typeof(System.String));
            dc.DefaultValue = "";
            dtForVariables.Columns.Add(dc);

            dc = new DataColumn("MethodName", typeof(System.String));
            dc.DefaultValue = "";
            dtForVariables.Columns.Add(dc);

            dc = new DataColumn("Parameter", typeof(System.String));
            dc.DefaultValue = "";
            dtForVariables.Columns.Add(dc);

            dc = new DataColumn("Variable", typeof(System.String));
            dc.DefaultValue = "";
            dtForVariables.Columns.Add(dc);

            return dtForVariables;
        }
    }
}
