using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MultiplayerMenuSetup
{
    private const string StartScenePath = "Assets/Scenes/Levels/StartScreen.unity";

    // The connection UI copies its look from this existing Start screen button.
    private const string StyleSourceButtonName = "StartButton";
    // The Quit button goes under this column, at the bottom of the start screen.
    private const string QuitButtonName = "QuitButton";
    // Top of the connection column, just under the Start button (screen centre = 0).
    private const float ColumnTop = -13f;
    private const float ColumnWidth = 360f;
    private const float ColumnSpacing = 10f;
    private const float StatusLabelHeight = 60f;

    [MenuItem("Tools/Multiplayer/Build Start Screen Connection UI")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(StartScenePath, OpenSceneMode.Single);
        MultiplayerMenuController controller = Object.FindFirstObjectByType<MultiplayerMenuController>();
        if (controller == null)
        {
            controller = CreateConnectionUI();
            Debug.Log("Built multiplayer connection UI into StartScreen.");
        }
        else
        {
            Debug.Log("Multiplayer menu already exists in StartScreen; restyling it.");
        }

        ApplyStartScreenStyle(controller);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static MultiplayerMenuController CreateConnectionUI()
    {
        GameObject networkGO = new GameObject("NetworkManager");
        NetworkManager networkManager = networkGO.AddComponent<NetworkManager>();
        UnityTransport transport = networkGO.AddComponent<UnityTransport>();
        networkManager.NetworkConfig.NetworkTransport = transport;
        networkGO.AddComponent<RelayConnectionManager>();

        GameObject canvasGO = new GameObject("MultiplayerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        GameObject panelGO = new GameObject("MultiplayerPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        panelGO.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420, 380);
        panelRect.anchoredPosition = new Vector2(0, -180);
        panelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        VerticalLayoutGroup layout = panelGO.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 12;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();

        TMP_Text statusLabel = CreateText("StatusLabel", panelGO.transform, resources, "");
        Button hostButton = CreateButton("HostButton", panelGO.transform, resources, "Host Game");
        TMP_InputField joinCodeInput = CreateInput("JoinCodeInput", panelGO.transform, resources);
        Button joinButton = CreateButton("JoinButton", panelGO.transform, resources, "Join Game");
        TMP_Text joinCodeLabel = CreateText("JoinCodeLabel", panelGO.transform, resources, "");

        GameObject controllerGO = new GameObject("MultiplayerMenuController");
        controllerGO.transform.SetParent(canvasGO.transform, false);
        MultiplayerMenuController controller = controllerGO.AddComponent<MultiplayerMenuController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("hostButton").objectReferenceValue = hostButton;
        so.FindProperty("joinButton").objectReferenceValue = joinButton;
        so.FindProperty("joinCodeInput").objectReferenceValue = joinCodeInput;
        so.FindProperty("joinCodeLabel").objectReferenceValue = joinCodeLabel;
        so.FindProperty("statusLabel").objectReferenceValue = statusLabel;
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddVoidPersistentListener(hostButton.onClick, controller.Host);
        UnityEventTools.AddVoidPersistentListener(joinButton.onClick, controller.Join);
        return controller;
    }

    // Makes the connection UI match the Start/Quit buttons: same sprite, tint, transition colours,
    // font asset and size, laid out as one centred column under Start, with Quit at the bottom.
    private static void ApplyStartScreenStyle(MultiplayerMenuController controller)
    {
        GameObject sourceGO = GameObject.Find(StyleSourceButtonName);
        Button source = sourceGO != null ? sourceGO.GetComponent<Button>() : null;
        if (source == null)
        {
            Debug.LogWarning($"No '{StyleSourceButtonName}' button in StartScreen; connection UI left unstyled.");
            return;
        }
        Image sourceImage = source.GetComponent<Image>();
        TMP_Text sourceText = source.GetComponentInChildren<TMP_Text>(true);
        Vector2 buttonSize = ((RectTransform)source.transform).sizeDelta;

        SerializedObject so = new SerializedObject(controller);
        Button hostButton = so.FindProperty("hostButton").objectReferenceValue as Button;
        Button joinButton = so.FindProperty("joinButton").objectReferenceValue as Button;
        TMP_InputField joinCodeInput = so.FindProperty("joinCodeInput").objectReferenceValue as TMP_InputField;
        TMP_Text joinCodeLabel = so.FindProperty("joinCodeLabel").objectReferenceValue as TMP_Text;
        TMP_Text statusLabel = so.FindProperty("statusLabel").objectReferenceValue as TMP_Text;
        if (hostButton == null || joinButton == null || joinCodeInput == null || joinCodeLabel == null || statusLabel == null)
        {
            Debug.LogWarning("MultiplayerMenuController is missing UI references; connection UI left unstyled.");
            return;
        }

        StyleButton(hostButton, source, sourceImage, sourceText, buttonSize);
        StyleInput(joinCodeInput, source, sourceImage, sourceText, buttonSize);
        StyleButton(joinButton, source, sourceImage, sourceText, buttonSize);
        StyleLabel(joinCodeLabel, sourceText, new Vector2(ColumnWidth, buttonSize.y));
        StyleLabel(statusLabel, sourceText, new Vector2(ColumnWidth, StatusLabelHeight));

        // Column order, top to bottom.
        Transform[] order = { hostButton.transform, joinCodeInput.transform, joinButton.transform, joinCodeLabel.transform, statusLabel.transform };
        for (int i = 0; i < order.Length; i++) order[i].SetSiblingIndex(i);

        RectTransform panel = (RectTransform)hostButton.transform.parent;
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0, ColumnTop);
        float columnHeight = buttonSize.y * 4 + StatusLabelHeight + ColumnSpacing * 4;
        panel.sizeDelta = new Vector2(ColumnWidth, columnHeight);

        // The Start/Quit buttons have no box of their own, so neither does this column.
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null) panelImage.enabled = false;

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = ColumnSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        PlaceQuitButtonBelow(panel, columnHeight);
    }

    // Both canvases are centred on the screen, so the column's bottom edge is ColumnTop - columnHeight from the centre.
    private static void PlaceQuitButtonBelow(RectTransform column, float columnHeight)
    {
        GameObject quitGO = GameObject.Find(QuitButtonName);
        if (quitGO == null)
        {
            Debug.LogWarning($"No '{QuitButtonName}' button in StartScreen; it was left where it was.");
            return;
        }
        RectTransform quit = (RectTransform)quitGO.transform;
        RectTransform parent = quit.parent as RectTransform;
        float parentOffset = parent != null ? parent.anchoredPosition.y : 0f;
        float centreY = ColumnTop - columnHeight - ColumnSpacing * 2 - quit.sizeDelta.y / 2;
        quit.anchoredPosition = new Vector2(0, centreY - parentOffset);
        EditorUtility.SetDirty(quit);
    }

    private static void StyleButton(Button button, Button source, Image sourceImage, TMP_Text sourceText, Vector2 size)
    {
        CopySelectable(source, button);
        CopyImage(sourceImage, button.GetComponent<Image>());
        CopyText(sourceText, button.GetComponentInChildren<TMP_Text>(true), 1f);
        SetSize(button.gameObject, size);
    }

    private static void StyleInput(TMP_InputField input, Button source, Image sourceImage, TMP_Text sourceText, Vector2 size)
    {
        CopySelectable(source, input);
        CopyImage(sourceImage, input.GetComponent<Image>());
        CopyText(sourceText, input.textComponent, 1f);
        if (input.placeholder is TMP_Text placeholder) CopyText(sourceText, placeholder, 0.5f);
        SetSize(input.gameObject, size);
        // The default text area is masked to 17px, which clips the larger button font.
        if (input.textViewport != null)
        {
            input.textViewport.offsetMin = new Vector2(10, 2);
            input.textViewport.offsetMax = new Vector2(-10, -2);
            EditorUtility.SetDirty(input.textViewport);
        }
    }

    private static void StyleLabel(TMP_Text label, TMP_Text sourceText, Vector2 size)
    {
        CopyText(sourceText, label, 1f);
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        SetSize(label.gameObject, size);
    }

    private static void CopySelectable(Selectable source, Selectable target)
    {
        target.transition = source.transition;
        target.colors = source.colors;
        target.spriteState = source.spriteState;
        EditorUtility.SetDirty(target);
    }

    private static void CopyImage(Image source, Image target)
    {
        if (source == null || target == null) return;
        target.sprite = source.sprite;
        target.type = source.type;
        target.color = source.color;
        target.material = source.material;
        target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        EditorUtility.SetDirty(target);
    }

    private static void CopyText(TMP_Text source, TMP_Text target, float alpha)
    {
        if (source == null || target == null) return;
        target.font = source.font;
        target.fontSharedMaterial = source.fontSharedMaterial;
        target.fontSize = source.fontSize;
        target.fontStyle = source.fontStyle;
        Color color = source.color;
        color.a *= alpha;
        target.color = color;
        EditorUtility.SetDirty(target);
    }

    private static void SetSize(GameObject go, Vector2 size)
    {
        ((RectTransform)go.transform).sizeDelta = size;
        LayoutElement element = go.GetComponent<LayoutElement>();
        if (element == null) element = go.AddComponent<LayoutElement>();
        element.preferredWidth = size.x;
        element.preferredHeight = size.y;
        EditorUtility.SetDirty(go);
    }

    public static void BuildAndExit()
    {
        Build();
        EditorApplication.Exit(0);
    }

    private static Button CreateButton(string name, Transform parent, TMP_DefaultControls.Resources resources, string label)
    {
        GameObject go = TMP_DefaultControls.CreateButton(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.GetComponentInChildren<TMP_Text>().text = label;
        go.AddComponent<LayoutElement>().preferredHeight = 50;
        return go.GetComponent<Button>();
    }

    private static TMP_InputField CreateInput(string name, Transform parent, TMP_DefaultControls.Resources resources)
    {
        GameObject go = TMP_DefaultControls.CreateInputField(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.GetComponent<TMP_InputField>().placeholder.GetComponent<TMP_Text>().text = "Enter join code";
        go.AddComponent<LayoutElement>().preferredHeight = 50;
        return go.GetComponent<TMP_InputField>();
    }

    private static TMP_Text CreateText(string name, Transform parent, TMP_DefaultControls.Resources resources, string text)
    {
        GameObject go = TMP_DefaultControls.CreateText(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        TMP_Text tmp = go.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        go.AddComponent<LayoutElement>().preferredHeight = 40;
        return tmp;
    }
}
