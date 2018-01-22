using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVSToJVSConversion.Common
{
    public class Constants
    {
        //SQL Commands
        public const string ENUMSQL = "select Enums.FileName, Enums.Enum, Enums.Include, Enums.Append, Enums.Exclude from Enums";

        public const string AVSJVSNOMAPPINGSQL = "select AVSJVSMapping.avsmethod, AVSJVSMapping.openjvsclass, AVSJVSMapping.openjvsmethod, jvsclasses.\"Include Statement\"  from AVSJVSMapping, jvsclasses where static='no' and jvsclasses.\"JVS Classes\"=AVSJVSMapping.openjvsclass";

        public const string AVSJVSYESMAPPINGSQL =
            "select AVSJVSMapping.avsmethod, AVSJVSMapping.openjvsclass, AVSJVSMapping.openjvsmethod, jvsclasses.\"Include Statement\"  from AVSJVSMapping, jvsclasses where static='yes' and jvsclasses.\"JVS Classes\"=AVSJVSMapping.openjvsclass";

        public const string INITIALIZESQL = "select * from Initialize";

    
    
    }


}
