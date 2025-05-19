using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Quest;
using System.Collections;
using System;


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
    private AudioSource audioSource;

    private int textUpdateProgress;
    private float textUpdateSpeed = .1f;
    private Coroutine textCoroutine;


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
        audioSource = GetComponent<AudioSource>();
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
        //npc_text.text = 
        StartSlowTextCoroutine(npcData.Goodbye);
        Invoke("HideDialogueScreen", 1f);
    }
    public void Interact(GameObject unused)
    {
        if (dialoguePanel != null)
        {
            //SetNPC_Text(npcData.Greeting);        
            StartSlowTextCoroutine(npcData.Greeting);
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
        if (npc_text != null && new_npc_text != interactString)
            StartSlowTextCoroutine(new_npc_text);
        //
        //npc_text.text = $"{npcData.npc_Name} says: " + new_npc_text;
        else if (npc_text != null) npc_text.text = new_npc_text;
        else Logging.Error("Npc text label is null.");
    }

    private void StartSlowTextCoroutine(string new_npc_text)
    {
        textUpdateProgress = 0;
        textCoroutine = StartCoroutine(SlowlySetText(new_npc_text));
    }

    private IEnumerator SlowlySetText(string full_npc_text)
    {

        while (true)
        {
            string currentText = full_npc_text.Substring(0, textUpdateProgress + 1);
            yield return new WaitForSeconds(textUpdateSpeed);
            npc_text.text = $"{npcData.npc_Name}: " + currentText;
            textUpdateProgress++;
            if (textUpdateProgress == full_npc_text.Length)
                StopCoroutine(textCoroutine);
        }

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
            newButton.style.width = Length.Percent(30);
            newButton.style.height = Length.Percent(10);

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
        Tuple<string, AudioClip> NPC_Words = npcData.Dialogue.GetNPCResponseToPlayer(playerResponse, npcHandler.dialogueUnlocked);
        Logging.Info($"Option selected: {playerResponse}. npc RESPONSE = {NPC_Words.Item1}");

        audioSource.clip = NPC_Words.Item2;
        if (audioSource.clip != null)
            audioSource.Play();
        SetNPC_Text(NPC_Words.Item1);
        List<string> playerResponses = GetAllPlayerResponses(NPC_Words.Item1);
        CreateButtons(playerResponses);
    }

    private List<string> GetAllPlayerResponses(string NPC_Words)
    {
        List<string> playerResponses = npcData.Dialogue.GetPlayerResponses(NPC_Words, npcHandler.dialogueUnlocked);
        return playerResponses;
    }
    #endregion

}

