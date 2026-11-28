using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        // Grow down from where the Start button sits, like the rest of the start screen column.
        GameObject go = StartMenuRows.CreateColumn("GameSetupMenu", template, Spacing);
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

    void Update()
    {
        if (MenuNavigation.CancelPressedThisFrame()) Back();
    }

    private void Build()
    {
        // Rebuilding replaces every row; keep a controller on the row it was on (e.g. while cycling the difficulty).
        int focusedRow = FocusedRow();
        StartMenuRows.Clear(transform);

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

        StartMenuRows.FitColumn(transform, template, SlotWidth, Spacing);
        FocusRow(focusedRow);
    }

    private int FocusedRow()
    {
        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return selected != null && selected.transform.parent == transform ? selected.transform.GetSiblingIndex() : -1;
    }

    // The row at that index, or the nearest button above it if the rows changed (e.g. Erase went away).
    private void FocusRow(int row)
    {
        for (int i = Mathf.Min(row, transform.childCount - 1); i >= 0; i--)
        {
            if (!transform.GetChild(i).TryGetComponent(out Button button) || !button.enabled) continue;
            MenuNavigation.Select(button);
            return;
        }
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

    private void AddButton(string text, float width, Action onClick) => StartMenuRows.AddButton(transform, template, text, width, onClick);

    private void AddLabel(string text) => StartMenuRows.AddLabel(transform, template, text, SlotWidth);

    #endregion
}
