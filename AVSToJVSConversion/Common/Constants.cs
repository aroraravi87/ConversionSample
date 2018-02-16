
namespace AVSToJVSConversion.Common
{
    public class Constants
    {
        //SQL Commands
        public const string ENUMSQL = "select Enums.FileName, Enums.Enum, Enums.Include, Enums.Append, Enums.Exclude from Enums where Enums.Enum in (";

        public const string AVSJVSNOMAPPINGSQL = "select AVSJVSMapping.avsmethod, AVSJVSMapping.openjvsclass, AVSJVSMapping.openjvsmethod, jvsclasses.\"Include Statement\"  from AVSJVSMapping, jvsclasses where static='no' and jvsclasses.\"JVS Classes\"=AVSJVSMapping.openjvsclass";

        public const string AVSJVSYESMAPPINGSQL =
            "select AVSJVSMapping.avsmethod, AVSJVSMapping.openjvsclass, AVSJVSMapping.openjvsmethod, jvsclasses.\"Include Statement\"  from AVSJVSMapping, jvsclasses where static='yes' and jvsclasses.\"JVS Classes\"=AVSJVSMapping.openjvsclass";

        public const string INITIALIZESQL = "select AVS, JVS, Class, Import from Initialize where ShouldReplace = '1'";

        public const string JVSVARIABLELISTSQL = "select JVS from Initialize where IsVarType='1'";

        public const string AVSVARIABLELISTSQL = "select AVS from Initialize where IsVarType='1'";

        public const string INITIALIZEVARIABLESQL = "select JVS, InitializeWith, ClassForInitialize, ImportForInitialize from Initialize where IsVarType = '1'";

    }


}
