using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class NPC_UI_Handler : MonoBehaviour, I_Interactable
{
    [SerializeField] NPC_SO npcData;
    [SerializeField] public UIDocument doc;
    [SerializeField] public Sprite buttonImage;
    private VisualElement root;
    private VisualElement dialoguePanel;
    private VisualElement imageSlot;
    private Label npc_text;
    private VisualElement playerOptions;
    [SerializeField] string interactString = "Tab to Interact";


    private List<Button> spawnedButtons = new List<Button>();


    void Awake()
    {
        root = doc.rootVisualElement;
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
    public void Interact()
    {
        if (dialoguePanel != null)
        {
            SetNPC_Text(npcData.Greeting);
            PresentResponseOptions(npcData.Greeting);
        }
    }

    private void PresentResponseOptions(string words)
    {
        List<string> playerResponses = npcData.Dialogue.GetPlayerResponses(words);
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
        new_npc_text = ParseNPCString(new_npc_text);
        if (npc_text != null && new_npc_text != interactString) npc_text.text = $"{npcData.npc_Name} says: " + new_npc_text;
        else if (npc_text != null) npc_text.text = new_npc_text;
        else Logging.Error("Npc text label is null.");
    }
    private void ClearNPC_Text()
    {
        if (npc_text != null) npc_text.text = "";
        else Logging.Error("Npc text label is null.");
    }

    private string ParseNPCString(string npc_text_string) //#TODO  belongs elsewhere or shouldnt extist
    {
        string questPattern = @"/Quest/";
        string XP_Pattern = @"/XP_Reward/";
        string goldPattern = @"/GoldReward/";
        if (Regex.Match(npc_text_string, questPattern).Success) npc_text_string = Regex.Replace(npc_text_string, questPattern, $"You have recieved the quest {npcData.GivesQuest}!");
        if (Regex.Match(npc_text_string, XP_Pattern).Success) npc_text_string = Regex.Replace(npc_text_string, XP_Pattern, $"You have gained {npcData.XP_Reward}xp!");
        if (Regex.Match(npc_text_string, goldPattern).Success) npc_text_string = Regex.Replace(npc_text_string, goldPattern, $"You have recieved {npcData.GoldReward} gold!");
        return npc_text_string;
    }
    #endregion

    #region Player response buttons

    public void CreateButtons(List<string> labels)
    {
        ClearButtons();
        foreach (string label in labels)
        {
            Button newButton = new Button { text = label };
            newButton.RegisterCallback<ClickEvent>(e => OnOptionSelected(label));
            spawnedButtons.Add(newButton);
            playerOptions.Add(newButton);
            //newButton.style.backgroundImage = new StyleBackground(buttonImage);
            //SetButtonSize(newButton, 5, 10);
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
        string NPC_Words = npcData.Dialogue.GetNPCResponseToPlayer(playerResponse);
        SetNPC_Text(NPC_Words);
        List<string> playerResponses = npcData.Dialogue.GetPlayerResponses(NPC_Words);
        CreateButtons(playerResponses);
    }
    #endregion

}
