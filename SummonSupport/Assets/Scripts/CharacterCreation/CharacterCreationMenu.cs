using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Start screen panel for a new character: its name and element. Opened by StartMenuController before a new
/// game starts (offline or hosted) and before joining a game. Built like GameSetupMenu, from copies of the start
/// screen's button and of the join code field, so it matches the rest of the menu.
/// Confirming stores the choice in <see cref="CharacterSelection"/>.
/// </summary>
public class CharacterCreationMenu : MonoBehaviour
{
    private const float Width = 360f;
    private const float ButtonWidth = 240f;
    private const float Spacing = 8f;

    private Button template;
    private Action<CharacterCreationData> onConfirm;
    private Action onBack;

    private readonly List<Element> elements = new();
    private int elementIndex;
    private TMP_InputField nameInput;
    private TMP_Text elementLabel;
    private TMP_Text messageLabel;

    /// <summary>Opens the panel next to <paramref name="template"/>. <paramref name="inputTemplate"/> styles the name field; without one a default field is made.</summary>
    public static CharacterCreationMenu Open(Button template, TMP_InputField inputTemplate, CharacterCreationProfile_SO profile, string heading,
        Action<CharacterCreationData> onConfirm, Action onBack)
    {
        GameObject go = StartMenuRows.CreateColumn("CharacterCreationMenu", template, Spacing);
        CharacterCreationMenu menu = go.AddComponent<CharacterCreationMenu>();
        menu.template = template;
        menu.onConfirm = onConfirm;
        menu.onBack = onBack;
        menu.elements.AddRange((profile != null ? profile : CharacterCreationProfile_SO.CreateRuntimeDefault()).GetSelectableElements());
        menu.Build(heading, inputTemplate);
        return menu;
    }

    void Update()
    {
        // Escape while typing the name only leaves the field.
        if (nameInput != null && nameInput.isFocused) return;
        if (MenuNavigation.CancelPressedThisFrame()) Back();
    }

    private void Build(string heading, TMP_InputField inputTemplate)
    {
        CharacterCreationData previous = CharacterSelection.Current;
        elementIndex = Mathf.Max(0, elements.IndexOf(previous.Element));

        StartMenuRows.AddLabel(transform, template, heading, Width);
        StartMenuRows.AddLabel(transform, template, "Create your character", Width);
        nameInput = AddNameInput(inputTemplate, previous.PlayerName);
        elementLabel = StartMenuRows.AddButton(transform, template, "", ButtonWidth, CycleElement).GetComponentInChildren<TMP_Text>(true);
        messageLabel = StartMenuRows.AddLabel(transform, template, "", Width);
        StartMenuRows.AddButton(transform, template, "Create Character", ButtonWidth, Confirm);
        StartMenuRows.AddButton(transform, template, "Back", ButtonWidth, Back);
        StartMenuRows.FitColumn(transform, template, Width, Spacing);

        ShowElement();
    }

    private TMP_InputField AddNameInput(TMP_InputField inputTemplate, string text)
    {
        TMP_InputField input = inputTemplate != null
            ? Instantiate(inputTemplate, transform)
            : TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources()).GetComponent<TMP_InputField>();
        input.transform.SetParent(transform, false);
        input.gameObject.name = "NameInput";
        input.gameObject.SetActive(true);
        // Drop the template's own listeners and settings (e.g. the join code's).
        input.onValueChanged = new TMP_InputField.OnChangeEvent();
        input.onEndEdit = new TMP_InputField.SubmitEvent();
        input.onSubmit = new TMP_InputField.SubmitEvent();
        input.contentType = TMP_InputField.ContentType.Standard;
        input.characterLimit = CharacterCreationData.MaxNameLength;
        // Moving a controller through the field should not start typing in it; Submit (A / Cross) does.
        input.shouldActivateOnSelect = false;
        input.navigation = Navigation.defaultNavigation; // the template links to the start screen's own buttons
        input.text = text;
        if (input.placeholder is TMP_Text placeholder) placeholder.text = "Character name";
        input.onValueChanged.AddListener(_ => ShowMessage(""));
        ((RectTransform)input.transform).sizeDelta = new Vector2(Width, StartMenuRows.RowHeight(template));
        return input;
    }

    private void CycleElement()
    {
        if (elements.Count == 0) return;
        elementIndex = (elementIndex + 1) % elements.Count;
        ShowElement();
    }

    private void ShowElement()
    {
        if (elementLabel != null) elementLabel.text = elements.Count > 0 ? $"Element: {elements[elementIndex]}" : "Element: None";
    }

    private void ShowMessage(string text)
    {
        if (messageLabel != null) messageLabel.text = text;
    }

    private void Confirm()
    {
        string name = CharacterCreationData.SanitizeName(nameInput != null ? nameInput.text : "");
        if (string.IsNullOrEmpty(name))
        {
            ShowMessage("Enter a name for your character.");
            return;
        }

        CharacterCreationData data = new(name, elements.Count > 0 ? elements[elementIndex] : Element.None);
        CharacterSelection.Confirm(data);
        Destroy(gameObject);
        onConfirm?.Invoke(data);
    }

    private void Back()
    {
        Destroy(gameObject);
        onBack?.Invoke();
    }
}
