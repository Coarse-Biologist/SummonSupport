using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Quest;

public class NPC_UI_Handler : MonoBehaviour, I_Interactable
{
    private UIDocument ui;
    private VisualTreeAsset UIPrefabAssets;
    [SerializeField] public Sprite buttonImage;
    private NPC_SO npcData;
    private NPC_Handler npcHandler;
    private VisualElement root;
    private VisualElement dialoguePanel;
    private VisualElement imageSlot;
    private Label npc_text;
    private VisualElement playerOptions;
    [SerializeField] string interactString = "Tab to Interact";


    private List<Button> spawnedButtons = new List<Button>();
    private NPC_Handler npcQuestProgress;


    void Start()
    {
        npcHandler = GetComponent<NPC_Handler>();
        npcData = npcHandler.npcData;
        npcQuestProgress = GetComponent<NPC_Handler>();
        ui = UI_DocHandler.Instance.ui;
        UIPrefabAssets = UI_DocHandler.Instance.UIPrefabAssets;
        root = ui.rootVisualElement;
        dialoguePanel = root.Q<VisualElement>("Dialogue");
        imageSlot = dialoguePanel.Q<VisualElement>("ImageSlot"); // ????????????
        npc_text = root.Q<Label>("NPC_Text");
        playerOptions = root.Q<VisualElement>("PlayerOptions");
    }
    public void ShowInteractionOption()
    {
        ShowDialogueScreen();
        SetNPC_Text(interactString);
        SetNPC_Image(npcData.NPC_Sprite);
    }

    public void HideInteractionOption()
    {
        ClearButtons();
        npc_text.text = npcData.Goodbye;
        Invoke("HideDialogueScreen", 1f);
    }
    public void Interact(GameObject unused)
    {
        if (dialoguePanel != null)
        {
            SetNPC_Text(npcData.Greeting);
            PresentResponseOptions(npcData.Greeting);
        }
    }

    private void PresentResponseOptions(string words)
    {
        List<string> playerResponses = GetAllPlayerResponses(words);
        CreateButtons(playerResponses);
    }

    #region Show/Hide screen

    public void ShowDialogueScreen()
    {
        dialoguePanel.style.display = DisplayStyle.Flex;
    }
    private void HideDialogueScreen()
    {
        dialoguePanel.style.display = DisplayStyle.None;
    }
    #endregion

    #region NPC image

    private void SetNPC_Image(Sprite npc_Sprite)
    {
        if (imageSlot != null && npc_Sprite != null) imageSlot.style.backgroundImage = new StyleBackground(npc_Sprite);
    }

    #endregion

    #region NPC dialogue


    private void SetNPC_Text(string new_npc_text)
    {
        if (npc_text != null && new_npc_text != interactString) npc_text.text = $"{npcData.npc_Name} says: " + new_npc_text;
        else if (npc_text != null) npc_text.text = new_npc_text;
        else Logging.Error("Npc text label is null.");
    }
    private void SetMetaInfo(string new_Info)
    {

    }
    private void ClearNPC_Text()
    {
        if (npc_text != null) npc_text.text = "";
        else Logging.Error("Npc text label is null.");
    }

    #endregion

    #region Player response buttons

    public void CreateButtons(List<string> labels)
    {
        ClearButtons();
        foreach (string label in labels)
        {
            TemplateContainer prefabContainer = UIPrefabAssets.Instantiate();
            Button newButton = prefabContainer.Q<Button>("ButtonPrefab");
            newButton.text = label;
            newButton.RegisterCallback<ClickEvent>(e => OnOptionSelected(label));
            spawnedButtons.Add(newButton);
            playerOptions.Add(newButton);

        }
    }
    private void SetButtonSize(Button button, int width = 10, int height = 5)
    {
        button.style.width = Length.Percent(width); // limit size of payment
        button.style.height = Length.Percent(height);
    }
    public void ClearButtons()
    {
        foreach (Button btn in spawnedButtons)
        {
            playerOptions.Remove(btn);
        }
        spawnedButtons.Clear();

    }
    private void OnOptionSelected(string playerResponse)
    {
        Logging.Info($"Option selected: {playerResponse}");
        string NPC_Words = npcData.Dialogue.GetNPCResponseToPlayer(playerResponse, npcHandler.dialogueUnlocked);
        SetNPC_Text(NPC_Words);
        List<string> playerResponses = GetAllPlayerResponses(NPC_Words);
        CreateButtons(playerResponses);
    }

    private List<string> GetAllPlayerResponses(string NPC_Words)
    {
        List<string> playerResponses = npcData.Dialogue.GetPlayerResponses(NPC_Words, npcHandler.dialogueUnlocked);
        return playerResponses;
    }
    #endregion

}

