using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevsRule.Tests.SharedDataAndFixtures.Utils
{
    public static class DataHelper
    {
        public static string GetJsonRuleFolderPath()

            => Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "DevsRule.Tests.SharedDataAndFixtures", "JsonRules"));
     
       public static string GetJsonRuleFilePath(string jsonFileName)

            => Path.GetFullPath(Path.Combine(GetJsonRuleFolderPath(), jsonFileName));
    }
}
