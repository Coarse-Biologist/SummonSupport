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

    [MenuItem("Tools/Multiplayer/Build Start Screen Connection UI")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(StartScenePath, OpenSceneMode.Single);
        if (Object.FindFirstObjectByType<MultiplayerMenuController>() != null)
        {
            Debug.Log("Multiplayer menu already exists in StartScreen; nothing changed.");
            return;
        }

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
        Button cancelButton = CreateButton("CancelButton", panelGO.transform, resources, "Cancel");
        TMP_Text joinCodeLabel = CreateText("JoinCodeLabel", panelGO.transform, resources, "");

        GameObject controllerGO = new GameObject("MultiplayerMenuController");
        controllerGO.transform.SetParent(canvasGO.transform, false);
        MultiplayerMenuController controller = controllerGO.AddComponent<MultiplayerMenuController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("hostButton").objectReferenceValue = hostButton;
        so.FindProperty("joinButton").objectReferenceValue = joinButton;
        so.FindProperty("cancelButton").objectReferenceValue = cancelButton;
        so.FindProperty("joinCodeInput").objectReferenceValue = joinCodeInput;
        so.FindProperty("joinCodeLabel").objectReferenceValue = joinCodeLabel;
        so.FindProperty("statusLabel").objectReferenceValue = statusLabel;
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddVoidPersistentListener(hostButton.onClick, controller.Host);
        UnityEventTools.AddVoidPersistentListener(joinButton.onClick, controller.Join);
        UnityEventTools.AddVoidPersistentListener(cancelButton.onClick, controller.Cancel);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Built multiplayer connection UI into StartScreen.");
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
