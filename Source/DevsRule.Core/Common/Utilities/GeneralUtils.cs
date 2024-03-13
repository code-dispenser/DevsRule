using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Seeds;
using System.Text.RegularExpressions;

namespace DevsRule.Core.Common.Utilities;

internal static class GeneralUtils
{
    public static List<(string assemblyQualifiedName, string fullName)> AssemblyTypeNames  { get; }
    public static List<(string assemblyQualifiedName, string fullName)>  EventTypeNames     { get; }


    static GeneralUtils()
    {
        string[] excludedNamespaces = new string[] { "System.", "Microsoft." };

        var filteredAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !excludedNamespaces.Any(exclude => a.GetName().Name!.StartsWith(exclude))).ToList();

        AssemblyTypeNames = filteredAssemblies.SelectMany(assembly => assembly.GetTypes().Select(t => (t.AssemblyQualifiedName, t.FullName)))
                                              .Where(t => !excludedNamespaces.Any(exclude => t.AssemblyQualifiedName!.StartsWith(exclude)))
                                              .ToList()!;

        EventTypeNames = filteredAssemblies.SelectMany(assembly => assembly.GetTypes())
                                           .Where(t => (typeof(ConditionEventBase).IsAssignableFrom(t) && t != typeof(ConditionEventBase)) || (typeof(RuleEventBase).IsAssignableFrom(t) && t != typeof(RuleEventBase)))
                                           .Select(t => (t.AssemblyQualifiedName, t.FullName))
                                           .ToList()!;
    }

    public static Dictionary<string, string> CreateDictionaryForRegex(string pattern, RegexOptions options, params (string key, string value)[] extrakeyValuePairs)
    {
        Dictionary<string, string> additionalInfo = new();

        additionalInfo[GlobalStrings.Regex_Pattern_Key] = pattern;

        if ((options & RegexOptions.Multiline)                  == RegexOptions.Multiline) additionalInfo[GlobalStrings.Regex_Multiline_Key]                            =  "true";
        if ((options & RegexOptions.Singleline)                 == RegexOptions.Singleline) additionalInfo[GlobalStrings.Regex_Singleline_Key]                          =  "true";
        if ((options & RegexOptions.IgnoreCase)                 == RegexOptions.IgnoreCase) additionalInfo[GlobalStrings.Regex_IgnoreCase_Key]                          =  "true";
        if ((options & RegexOptions.CultureInvariant)           == RegexOptions.CultureInvariant) additionalInfo[GlobalStrings.Regex_CultureInvariant_Key]              =  "true";
        if ((options & RegexOptions.NonBacktracking)            == RegexOptions.NonBacktracking) additionalInfo[GlobalStrings.Regex_NonBacktracking_Key]                =  "true";
        if ((options & RegexOptions.IgnorePatternWhitespace)    == RegexOptions.IgnorePatternWhitespace) additionalInfo[GlobalStrings.Regex_IgnorePatternWhitespace_Key]=  "true";
        if ((options & RegexOptions.RightToLeft)                == RegexOptions.RightToLeft) additionalInfo[GlobalStrings.Regex_RightToLeft_Key]                        =  "true";
        if ((options & RegexOptions.Compiled)                   == RegexOptions.Compiled) additionalInfo[GlobalStrings.Regex_Compiled_Key]                              =  "true";
        if ((options & RegexOptions.ExplicitCapture)            == RegexOptions.ExplicitCapture) additionalInfo[GlobalStrings.Regex_ExplicitCapture_Key]                =  "true";
        if ((options & RegexOptions.ECMAScript)                 == RegexOptions.ECMAScript) additionalInfo[GlobalStrings.Regex_ECMAScript_Key]                          =  "true";

        foreach(var keyPair in extrakeyValuePairs)
        {
            additionalInfo[keyPair.key] = keyPair.value;
        }

        return additionalInfo;
    }




}








