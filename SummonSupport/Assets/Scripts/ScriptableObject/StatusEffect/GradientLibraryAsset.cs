using UnityEngine;

[CreateAssetMenu(fileName = "GradientLibrary", menuName = "Gradient/Gradient Library")]
public class GradientLibraryAsset : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public Element Element;
        public Gradient gradient;
    }

    public Entry[] entries;

    // Lookup convenience
    public Gradient GetGradientByElement(Element element)
    {
        foreach (var entry in entries)
        {
            if (entry.Element == element)
            {
                //Debug.Log($"returning {entry.gradient} for the strongest element: {element}");
                return entry.gradient;
            }
        }
        //Debug.LogWarning($"GradientLibrary: No gradient found with name '{element}'");
        return null;
    }

}
