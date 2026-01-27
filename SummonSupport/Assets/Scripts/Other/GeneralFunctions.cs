using UnityEngine;

public static class GeneralFunctions
{
    public static string GetCleanEnumString<Enum>(Enum modEnum)
    {
        return System.Text.RegularExpressions.Regex.Replace(modEnum.ToString(), "(?<!^)([A-Z])", " $1");
    }
}
