using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class NPC_Handler : MonoBehaviour, I_Interactable
{
    [SerializeField] NPC_SO npcData;
    [SerializeField] GameObject interactCanvas;
    [SerializeField] public GameObject buttonPrefab;

    private GameObject canvasInstance;
    private TextMeshProUGUI dialogue;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private Transform buttonContainer;
    void Awake()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = npcData.NPC_Sprite;

    }

    public void ShowInteractionOption()
    {
        if (canvasInstance == null)
        {
            canvasInstance = Instantiate(interactCanvas, transform.position, Quaternion.identity);
            dialogue = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
            buttonContainer = canvasInstance.GetComponentInChildren<VerticalLayoutGroup>().transform;

        }
        else
        {
            canvasInstance.SetActive(true);
            dialogue.text = "Tab to interact";
        }
    }

    public void HideInteractionOption()
    {
        ClearButtons();
        dialogue.text = npcData.Goodbye;
        Invoke("HideandReset", 2f);
    }
    public void Interact()
    {
        if (dialogue != null)
        {
            SetNPC_Text(npcData.Greeting);
            PresentResponseOptions(npcData.Greeting);
        }
    }

    private void HideandReset()
    {
        canvasInstance.SetActive(false);
        SetNPC_Text(npcData.Greeting);
    }

    private void PresentResponseOptions(string words)
    {
        List<string> playerResponses = npcData.Dialogue.GetPlayerResponses(words);
        CreateButtons(playerResponses);
    }

    public void CreateButtons(List<string> labels)
    {
        ClearButtons();

        foreach (string label in labels)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer, false);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            Button myButton = newButton.GetComponent<Button>();
            myButton.onClick.AddListener(() => OnOptionSelected(label));

            if (buttonText != null)
            {
                buttonText.text = label;
            }
            spawnedButtons.Add(newButton);

        }
    }
    private void SetNPC_Text(string npc_Words)
    {
        dialogue.text = npc_Words;
    }
    private void OnOptionSelected(string playerResponse)
    {
        Logging.Info($"Option selected: {playerResponse}");
        string NPC_Words = npcData.Dialogue.GetNPCResponseToPlayer(playerResponse);
        SetNPC_Text(NPC_Words);
        List<string> playerResponses = npcData.Dialogue.GetPlayerResponses(NPC_Words);
        CreateButtons(playerResponses);
    }
    public void ClearButtons()
    {
        foreach (GameObject btn in spawnedButtons)
        {
            Destroy(btn);
        }
        spawnedButtons.Clear();
    }
}
