using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public static class ElementDict
{
    public static Dictionary<Element, T> GetElementDict<T>(T[] arrayOfType)
    {
        Dictionary<Element, T> elementDict = new();
        if (arrayOfType.Length != 15) throw new Exception($"The type array provided to GetElementDict must have a length of 15, but it has a length of {arrayOfType.Length}.");
        int i = 0;
        foreach (Element element in Enum.GetValues(typeof(Element)))
        {
            if (element == Element.None) continue;
            elementDict.Add(element, arrayOfType[i]);
            i++;
            Debug.Log("Added element " + element + " to elementDict with value " + arrayOfType[i - 1]);
        }
        return elementDict;
    }
}
