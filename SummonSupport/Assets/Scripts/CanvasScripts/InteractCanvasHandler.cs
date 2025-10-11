using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEditor.Rendering.Canvas.ShaderGraph;
using System;
using SummonSupportEvents;

public class InteractCanvasHandler : MonoBehaviour
{
    public static InteractCanvasHandler Instance { get; private set; }
    private GameObject canvasInstance;
    [SerializeField] GameObject interactCanvas;
    [field: SerializeField] public float slowDisplaySpeed = .8f;

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

    public void DisplayIncrementalText(Vector2 spawnLoc, string temporaryText)
    {
        StartCoroutine(SlowlyDisplayCanvasText(spawnLoc, temporaryText));
    }
    public IEnumerator SlowlyDisplayCanvasText(Vector2 spawnLoc, string temporaryText)
    {
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, spawnLoc, Quaternion.identity);
        else canvasInstance.SetActive(true);
        TextMeshProUGUI canvasGUI = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        int textUpdateProgress = 0;
        while (temporaryText.Length > textUpdateProgress)
        {
            string currentText = temporaryText.Substring(0, textUpdateProgress + 1);
            canvasGUI.text = $"{currentText}";
            textUpdateProgress++;
            yield return new WaitForSeconds(slowDisplaySpeed);
        }
        HideInteractionOption();
    }

    public void DisplayXPGain(EnemyStats enemyStats)
    {
        Vector2 loc = enemyStats.transform.position;
        loc = new Vector2(loc.x, loc.y + 1);
        int XPgained = (int)enemyStats.XP_OnDeath;
        if (canvasInstance == null) canvasInstance = Instantiate(interactCanvas, loc, Quaternion.identity);
        else canvasInstance.SetActive(true);
        TextMeshProUGUI canvasGUI = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        canvasGUI.text = $"{XPgained} XP gained!";

    }

}
