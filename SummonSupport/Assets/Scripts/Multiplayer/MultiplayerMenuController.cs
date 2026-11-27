using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MultiplayerMenuController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Map1";
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text joinCodeLabel;
    [SerializeField] private TMP_Text statusLabel;

    private bool busy;

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void Start()
    {
        SetStatus("");
        SetJoinCode("");
        SetBusy(false);

        RelayConnectionManager relay = RelayConnectionManager.Instance;
        if (relay != null && !string.IsNullOrEmpty(relay.LastDisconnectReason))
        {
            SetStatus(relay.LastDisconnectReason);
            relay.LastDisconnectReason = null;
        }
    }

    public async void Host()
    {
        if (busy) return;
        SetBusy(true);
        SetStatus("Creating session...");
        try
        {
            await RelayConnectionManager.Instance.HostAsync();
            SetJoinCode(RelayConnectionManager.Instance.JoinCode);
            SetStatus("Hosting. Share the code with your friends.");
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Fail("Could not host", e);
        }
    }

    public async void Join()
    {
        if (busy) return;
        string code = joinCodeInput != null ? joinCodeInput.text.Trim() : "";
        if (string.IsNullOrEmpty(code))
        {
            SetStatus("Enter a join code first.");
            return;
        }
        SetBusy(true);
        SetStatus("Joining...");
        try
        {
            await RelayConnectionManager.Instance.JoinAsync(code);
        }
        catch (Exception e)
        {
            Fail("Could not join", e);
        }
    }

    public void Cancel()
    {
        RelayConnectionManager.Instance.Disconnect();
        SetJoinCode("");
        SetStatus("");
        SetBusy(false);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            SetStatus("Connected. Waiting for the host to start the game...");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        SetStatus("Disconnected from session.");
        SetJoinCode("");
        SetBusy(false);
    }

    private void Fail(string message, Exception e)
    {
        Debug.LogError($"{message}: {e}");
        SetStatus($"{message}. Check the code and try again.");
        RelayConnectionManager.Instance.Disconnect();
        SetBusy(false);
    }

    private void SetBusy(bool value)
    {
        busy = value;
        if (hostButton != null) hostButton.interactable = !value;
        if (joinButton != null) joinButton.interactable = !value;
        if (cancelButton != null) cancelButton.interactable = value;
    }

    private void SetStatus(string text)
    {
        if (statusLabel != null) statusLabel.text = text;
    }

    private void SetJoinCode(string code)
    {
        if (joinCodeLabel != null) joinCodeLabel.text = string.IsNullOrEmpty(code) ? "" : $"Join code: {code}";
    }
}
