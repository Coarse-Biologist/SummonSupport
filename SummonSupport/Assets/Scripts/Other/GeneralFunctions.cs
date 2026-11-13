using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public static class GeneralFunctions
{
    public static string GetCleanEnumString<Enum>(Enum modEnum)
    {
        return System.Text.RegularExpressions.Regex.Replace(modEnum.ToString(), "(?<!^)([A-Z])", " $1");
    }
    public static Element GetRandomElement()
    {
        return (Element)UnityEngine.Random.Range(1, System.Enum.GetValues(typeof(Element)).Length); // not 0, which is element.None
    }

    public static List<Enum> GetIteratableEnum(Enum anEnum)
    {
        return Enum.GetValues(anEnum.GetType()).Cast<Enum>().ToList();
    }

}
