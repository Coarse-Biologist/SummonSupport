using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds start screen panels out of copies of an existing start screen button, so every panel matches the
/// rest of the menu (see GameSetupMenu and CharacterCreationMenu).
/// </summary>
public static class StartMenuRows
{
    /// <summary>A column that grows down from where <paramref name="template"/> sits, like the start screen buttons.</summary>
    public static GameObject CreateColumn(string name, Button template, float spacing)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        go.transform.SetParent(template.transform.parent, false);
        RectTransform templateRect = (RectTransform)template.transform;
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = templateRect.anchorMin;
        rect.anchorMax = templateRect.anchorMax;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0, RowHeight(template) / 2);

        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = spacing;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return go;
    }

    public static float RowHeight(Button template) => ((RectTransform)template.transform).sizeDelta.y;

    /// <summary>Sizes the column to its rows; call after adding them.</summary>
    public static void FitColumn(Transform column, Button template, float width, float spacing)
    {
        int rows = column.childCount;
        ((RectTransform)column).sizeDelta = new Vector2(width, rows * RowHeight(template) + Mathf.Max(0, rows - 1) * spacing);
    }

    /// <summary>Removes every row. Detaches them first: Destroy waits for the end of the frame, and the column is sized from its rows.</summary>
    public static void Clear(Transform column)
    {
        for (int i = column.childCount - 1; i >= 0; i--)
        {
            Transform child = column.GetChild(i);
            child.SetParent(null, false);
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static Button AddButton(Transform column, Button template, string text, float width, Action onClick)
    {
        Button button = UnityEngine.Object.Instantiate(template, column);
        button.gameObject.name = text;
        button.gameObject.SetActive(true);
        button.interactable = true;
        button.onClick = new Button.ButtonClickedEvent(); // drop the template's own click calls
        button.onClick.AddListener(() => onClick());
        SetRow(button.gameObject, template, text, width);
        return button;
    }

    public static TMP_Text AddLabel(Transform column, Button template, string text, float width)
    {
        Button copy = UnityEngine.Object.Instantiate(template, column);
        copy.gameObject.name = "Label";
        copy.gameObject.SetActive(true);
        if (copy.TryGetComponent(out Image image)) image.enabled = false;
        copy.enabled = false;
        return SetRow(copy.gameObject, template, text, width);
    }

    public static TMP_Text SetRow(GameObject row, Button template, string text, float width)
    {
        RectTransform rect = (RectTransform)row.transform;
        rect.sizeDelta = new Vector2(width, RowHeight(template));
        TMP_Text label = row.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return null;
        label.text = text;
        // Long texts shrink to fit instead of spilling out of the row.
        label.enableAutoSizing = true;
        label.fontSizeMin = 12;
        label.fontSizeMax = template.GetComponentInChildren<TMP_Text>(true).fontSize;
        return label;
    }
}
