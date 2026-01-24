using UnityEngine;
using TMPro;
using System.Collections;

using SummonSupportEvents;
using UnityEngine.Rendering;

public class FloatingInfoHandler : MonoBehaviour
{
    public static FloatingInfoHandler Instance { get; private set; }
    private GameObject canvasInstance;
    [SerializeField] GameObject interactCanvas;
    [SerializeField] GameObject playerDialogueCanvas;

    [field: SerializeField] public float slowDisplaySpeed = .8f;

    [field: SerializeField] GameObject xpTextGUI;

    public void Awake()
    {
        Instance = this;

    }

    private void OnEnable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(DisplayXPGain);
        EventDeclarer.PlayerDialogue?.AddListener(ShowPlayerDialogue);
    }
    private void OnDisable()
    {
        EventDeclarer.EnemyDefeated?.RemoveListener(DisplayXPGain);
        EventDeclarer.PlayerDialogue?.RemoveListener(ShowPlayerDialogue);

    }

    public void ShowInteractionOption(Vector3 spawnLoc, string interactMessage)
    {
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, spawnLoc, Quaternion.identity);
        else
        {
            canvasInstance.SetActive(true);
            canvasInstance.transform.position = spawnLoc;
        }

        canvasInstance.GetComponentInChildren<TextMeshProUGUI>().text = interactMessage;
    }

    public void HideInteractionOption()
    {
        Destroy(canvasInstance);
    }

    public void SetTemporaryCanvasText(Transform transform, string temporaryText)
    {
        Canvas canvas = transform.GetComponent<Canvas>();
        canvas.GetComponentInChildren<TextMeshProUGUI>().text = temporaryText;

    }

    public void DisplayIncrementalText(Transform spawnLoc, string temporaryText, float totalDuration = 4f)
    {
        StartCoroutine(SlowlyDisplayCanvasText(spawnLoc, temporaryText, totalDuration));
    }
    public IEnumerator SlowlyDisplayCanvasText(Transform spawnLoc, string temporaryText, float totalDuration = 4f)
    {
        int chars = temporaryText.Length;
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, spawnLoc.position + new Vector3(0, 2.5f, 0), Quaternion.identity, spawnLoc);
        else canvasInstance.SetActive(true);
        TextMeshProUGUI canvasGUI = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        int textUpdateProgress = 0;
        while (temporaryText.Length > textUpdateProgress)
        {
            string currentText = temporaryText.Substring(0, textUpdateProgress + 1);
            canvasGUI.text = $"{currentText}";
            textUpdateProgress++;
            yield return new WaitForSeconds(totalDuration / chars);
        }

        HideInteractionOption();
    }
    public IEnumerator DisplayPlayerDialogue(Transform spawnLoc, string temporaryText, float printSpeed, float stayDuration = 4f)
    {
        int chars = temporaryText.Length;
        if (canvasInstance == null) canvasInstance = Instantiate(playerDialogueCanvas, spawnLoc.position + new Vector3(0, 2.5f, 0), Quaternion.identity, spawnLoc);
        else canvasInstance.SetActive(true);
        TextMeshProUGUI canvasGUI = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        int textUpdateProgress = 0;
        while (temporaryText.Length > textUpdateProgress)
        {
            string currentText = temporaryText.Substring(0, textUpdateProgress + 1);
            canvasGUI.text = $"{currentText}";
            textUpdateProgress++;
            yield return new WaitForSeconds(printSpeed / chars);
        }
        yield return new WaitForSeconds(stayDuration);
        HideInteractionOption();
    }

    public void DisplayXPGain(EnemyStats enemyStats)
    {
        DisplayGoldenLetters($"+{enemyStats.XP_OnDeath} xp");
    }
    public void DisplayXPGain(float xpReward)
    {
        DisplayGoldenLetters($"+{xpReward} xp");
    }
    public void DisplayGoldenLetters(string words, float duration = .8f)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f));
        Transform playerTransform = PlayerStats.Instance.transform;
        Vector3 pos = playerTransform.position + randomOffset;
        GameObject xpCanvas = Instantiate(xpTextGUI, pos, Quaternion.identity);
        xpCanvas.transform.rotation = playerTransform.rotation;
        if (xpCanvas.TryGetComponent(out TextMeshProUGUI canvasGUI))
        {
            //Debug.Log($"font size = {canvasGUI.fontSize}");
            canvasGUI.text = $"{words}";
            canvasGUI.fontSize = .3f;
        }
        if (xpCanvas.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(randomOffset, ForceMode.Impulse);
        }
        Destroy(xpCanvas, duration);
    }

    public void ShowPlayerDialogue(DialogueAndAudio_SO dialogue)
    {
        Debug.Log($"Trying to show player dialogue: {dialogue.playerDialogue}");
        StartCoroutine(DisplayPlayerDialogue(PlayerStats.Instance.transform, dialogue.playerDialogue, dialogue.playerDialogue.Length * .1f));
    }




}
