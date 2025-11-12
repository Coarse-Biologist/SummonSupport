using UnityEngine;
using TMPro;
using System.Collections;

using SummonSupportEvents;
using UnityEngine.Rendering;

public class InteractCanvasHandler : MonoBehaviour
{
    public static InteractCanvasHandler Instance { get; private set; }
    private GameObject canvasInstance;
    [SerializeField] GameObject interactCanvas;
    [field: SerializeField] public float slowDisplaySpeed = .8f;

    [field: SerializeField] GameObject xpTextGUI;

    public void Awake()
    {
        Instance = this;

    }

    private void OnEnable()
    {
        EventDeclarer.EnemyDefeated?.AddListener(DisplayXPGain);
    }
    private void OnDisable()
    {
        EventDeclarer.EnemyDefeated?.RemoveListener(DisplayXPGain);
    }

    public void ShowInteractionOption(Vector2 spawnLoc, string interactMessage)
    {
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, spawnLoc, Quaternion.identity);
        else canvasInstance.SetActive(true);

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

    public void DisplayIncrementalText(Vector2 spawnLoc, string temporaryText, float duration)
    {
        StartCoroutine(SlowlyDisplayCanvasText(spawnLoc, temporaryText, duration));
    }
    public IEnumerator SlowlyDisplayCanvasText(Vector2 spawnLoc, string temporaryText, float totalDuration = 4f)
    {
        int chars = temporaryText.Length;
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, spawnLoc, Quaternion.identity);
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

    public void DisplayXPGain(EnemyStats enemyStats)
    {
        DisplayGoldenLetters($"+{enemyStats.XP_OnDeath} xp");
    }

    public void DisplayGoldenLetters(string loot, float duration = .8f)
    {
        Transform playerTransform = PlayerStats.Instance.transform;
        Vector2 pos = new Vector2(playerTransform.position.x, playerTransform.position.y + 1);
        GameObject xpCanvas = Instantiate(xpTextGUI, pos, Quaternion.identity);
        xpCanvas.transform.rotation = playerTransform.rotation;
        if (xpCanvas.TryGetComponent(out TextMeshProUGUI canvasGUI))
            canvasGUI.text = $"{loot}";
        if (xpCanvas.TryGetComponent(out Rigidbody2D rb))
        {
            rb.AddForce(new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)), ForceMode2D.Impulse);
        }
        Destroy(xpCanvas, duration);
    }



}
