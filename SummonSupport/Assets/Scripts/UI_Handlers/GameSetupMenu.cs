using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Start screen panel shown before starting or hosting a game: pick one of the save slots (an empty one starts
/// a new game), pick the difficulty, then begin. It lists the used slots and one empty slot, if there is one.
/// Built from a copy of an existing start screen button, so it matches the rest of the menu.
/// </summary>
public class GameSetupMenu : MonoBehaviour
{
    private const float SlotWidth = 360f;
    private const float ButtonWidth = 240f;
    private const float Spacing = 8f;

    private static readonly Difficulty[] Difficulties = (Difficulty[])Enum.GetValues(typeof(Difficulty));

    private Button template;
    private string heading;
    private Action<int, Difficulty, SessionSaveData> onBegin;
    private Action onBack;

    private readonly SessionSaveData[] saves = new SessionSaveData[SaveHandler.SlotCount + 1];
    private int selectedSlot = 1;
    private Difficulty difficulty = Difficulty.Novice;
    private bool confirmingErase;

    /// <summary>Opens the panel next to <paramref name="template"/>. onBegin gets the slot, the difficulty and the save to continue (null for a new game).</summary>
    public static GameSetupMenu Open(Button template, string heading, Action<int, Difficulty, SessionSaveData> onBegin, Action onBack)
    {
        GameObject go = new GameObject("GameSetupMenu", typeof(RectTransform), typeof(VerticalLayoutGroup));
        go.transform.SetParent(template.transform.parent, false);
        // Grow down from where the Start button sits, like the rest of the start screen column.
        RectTransform templateRect = (RectTransform)template.transform;
        float rowHeight = templateRect.sizeDelta.y;
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = templateRect.anchorMin;
        rect.anchorMax = templateRect.anchorMax;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0, rowHeight / 2);

        VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = Spacing;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        GameSetupMenu menu = go.AddComponent<GameSetupMenu>();
        menu.template = template;
        menu.heading = heading;
        menu.onBegin = onBegin;
        menu.onBack = onBack;
        menu.ReadSaves();
        menu.SelectFirstSlot();
        menu.Build();
        return menu;
    }

    private void ReadSaves()
    {
        for (int slot = 1; slot <= SaveHandler.SlotCount; slot++) saves[slot] = SaveHandler.ReadSlot(slot);
    }

    // Start on the most recently saved slot, or slot 1.
    private void SelectFirstSlot()
    {
        string newest = null;
        for (int slot = 1; slot <= SaveHandler.SlotCount; slot++)
        {
            if (saves[slot] == null || (newest != null && string.CompareOrdinal(saves[slot].saveTime, newest) <= 0)) continue;
            newest = saves[slot].saveTime;
            selectedSlot = slot;
        }
        if (saves[selectedSlot] != null) difficulty = saves[selectedSlot].difficulty;
        else selectedSlot = VisibleSlots()[0];
    }

    // Every used slot, and the first empty one (up to SaveHandler.SlotCount in all), in slot order.
    private List<int> VisibleSlots()
    {
        List<int> slots = new();
        bool emptyShown = false;
        for (int slot = 1; slot <= SaveHandler.SlotCount; slot++)
        {
            if (saves[slot] != null) slots.Add(slot);
            else if (!emptyShown)
            {
                slots.Add(slot);
                emptyShown = true;
            }
        }
        return slots;
    }

    private void Build()
    {
        // Detach the old rows first: Destroy waits for the end of the frame, and the panel is sized from its rows.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }

        AddLabel(heading);
        List<int> visibleSlots = VisibleSlots();
        // An erased slot can drop out of the list; fall back to the empty slot that is shown.
        if (!visibleSlots.Contains(selectedSlot)) selectedSlot = visibleSlots.Find(slot => saves[slot] == null);
        foreach (int slot in visibleSlots)
        {
            int slotNumber = slot;
            string text = SaveHandler.DescribeSlot(slotNumber);
            AddButton(slotNumber == selectedSlot ? $"> {text} <" : text, SlotWidth, () => SelectSlot(slotNumber));
        }

        SessionSaveData selected = saves[selectedSlot];
        AddButton($"Difficulty: {difficulty}", ButtonWidth, CycleDifficulty);
        // Name the slot, since one is already selected when the panel opens (the newest save).
        AddButton(selected == null ? $"New Game in Slot {selectedSlot}" : $"Continue Slot {selectedSlot}", ButtonWidth, Begin);
        if (selected != null) AddButton(confirmingErase ? "Click again to erase" : "Erase Slot", ButtonWidth, Erase);
        AddButton("Back", ButtonWidth, Back);

        int rows = transform.childCount;
        float rowHeight = ((RectTransform)template.transform).sizeDelta.y;
        ((RectTransform)transform).sizeDelta = new Vector2(SlotWidth, rows * rowHeight + (rows - 1) * Spacing);
    }

    private void SelectSlot(int slot)
    {
        selectedSlot = slot;
        confirmingErase = false;
        if (saves[slot] != null) difficulty = saves[slot].difficulty;
        Build();
    }

    private void CycleDifficulty()
    {
        difficulty = Difficulties[(Array.IndexOf(Difficulties, difficulty) + 1) % Difficulties.Length];
        Build();
    }

    private void Erase()
    {
        if (!confirmingErase)
        {
            confirmingErase = true;
            Build();
            return;
        }
        confirmingErase = false;
        SaveHandler.EraseSlot(selectedSlot);
        saves[selectedSlot] = null;
        Build();
    }

    private void Begin()
    {
        int slot = selectedSlot;
        Difficulty chosen = difficulty;
        SessionSaveData save = saves[slot];
        Destroy(gameObject);
        onBegin?.Invoke(slot, chosen, save);
    }

    private void Back()
    {
        Destroy(gameObject);
        onBack?.Invoke();
    }

    #region Building rows from the template button

    private void AddButton(string text, float width, Action onClick)
    {
        Button button = Instantiate(template, transform);
        button.gameObject.name = text;
        button.gameObject.SetActive(true);
        button.interactable = true;
        button.onClick = new Button.ButtonClickedEvent(); // drop the template's own click calls
        button.onClick.AddListener(() => onClick());
        SetRow(button.gameObject, text, width);
    }

    private void AddLabel(string text)
    {
        Button copy = Instantiate(template, transform);
        copy.gameObject.name = "Heading";
        copy.gameObject.SetActive(true);
        if (copy.TryGetComponent(out Image image)) image.enabled = false;
        copy.enabled = false;
        SetRow(copy.gameObject, text, SlotWidth);
    }

    private void SetRow(GameObject row, string text, float width)
    {
        RectTransform rect = (RectTransform)row.transform;
        rect.sizeDelta = new Vector2(width, ((RectTransform)template.transform).sizeDelta.y);
        TMP_Text label = row.GetComponentInChildren<TMP_Text>(true);
        if (label == null) return;
        label.text = text;
        // Long slot descriptions shrink to fit instead of spilling out of the button.
        label.enableAutoSizing = true;
        label.fontSizeMin = 12;
        label.fontSizeMax = template.GetComponentInChildren<TMP_Text>(true).fontSize;
    }

    #endregion
}
