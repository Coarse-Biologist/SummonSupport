using System.Collections;
using TMPro;
using UnityEngine;

public class TitleDisplayHandler : MonoBehaviour
{
    [field: SerializeField] public TextMeshPro text;
    [field: SerializeField] public WaitForSeconds waitTime = new WaitForSeconds(.1f);

    [field: SerializeField] public string GameTitle = $"Summon Support";
    private string currenlyDisplayedString = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (text != null)
            StartCoroutine(DisplayTitleText());
    }

    private IEnumerator DisplayTitleText()
    {
        while (currenlyDisplayedString.Length < GameTitle.Length)
        {
            currenlyDisplayedString = GameTitle.Substring(0, currenlyDisplayedString.Length + 1);
            text.text = $"\n";
            text.text += $"\n";
            text.text += $"{currenlyDisplayedString}";
            yield return waitTime;
        }
    }
}
