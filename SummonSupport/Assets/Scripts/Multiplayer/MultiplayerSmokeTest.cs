using System.Collections;
using System.IO;
using System.Linq;
using SummonSupportEvents;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerSmokeTest : MonoBehaviour
{
    private static string role;
    private static string logPath;
    private static float clientDelay = 8f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        role = ArgValue(args, "-mpRole");
        if (string.IsNullOrEmpty(role)) return;

        logPath = ArgValue(args, "-mpLog") ?? "mp-smoke.log";
        string delay = ArgValue(args, "-mpDelay");
        if (!string.IsNullOrEmpty(delay)) float.TryParse(delay, out clientDelay);

        DontDestroyOnLoad(new GameObject("MultiplayerSmokeTest").AddComponent<MultiplayerSmokeTest>().gameObject);
    }

    private IEnumerator Start()
    {
        float timeout = 10f;
        while (NetworkManager.Singleton == null && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
        if (NetworkManager.Singleton == null)
        {
            Log("FAIL no NetworkManager");
            Application.Quit(1);
            yield break;
        }

        NetworkManager nm = NetworkManager.Singleton;
        nm.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
        nm.LogLevel = Unity.Netcode.LogLevel.Developer;
        nm.OnClientConnectedCallback += id => Log($"connected client={id} local={nm.LocalClientId}");
        nm.OnClientDisconnectCallback += id => Log($"disconnect client={id} local={nm.LocalClientId} reason={nm.DisconnectReason}");
        StartCoroutine(StatusLoop(nm));

        if (role == "host")
        {
            nm.StartHost();
            Log("host started");
            yield return new WaitForSeconds(3f);
            nm.SceneManager.LoadScene("Map1", LoadSceneMode.Single);
            Log("host requested Map1");
            yield return new WaitForSeconds(8f);
            EventDeclarer.SpawnEnemies?.Invoke(null);
            Log("host triggered enemy spawn");
            yield return new WaitForSeconds(7f);
            Report(nm);
        }
        else
        {
            yield return new WaitForSeconds(clientDelay);
            nm.StartClient();
            Log("client started");
            yield return new WaitForSeconds(15f);
            Report(nm);
        }

        Application.Quit(0);
    }

    private IEnumerator StatusLoop(NetworkManager nm)
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            Log($"status listening={nm.IsListening} client={nm.IsClient} server={nm.IsServer} connected={nm.IsConnectedClient} scene={SceneManager.GetActiveScene().name}");
        }
    }

    private void Report(NetworkManager nm)
    {
        Log($"scene={SceneManager.GetActiveScene().name} listening={nm.IsListening} localClient={nm.LocalClientId} isHost={nm.IsHost}");
        NetworkPlayer[] players = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        int owned = 0;
        foreach (NetworkPlayer player in players)
        {
            if (player.IsOwner) owned++;
            Log($"player owner={player.OwnerClientId} isOwner={player.IsOwner} pos={player.transform.position}");
        }
        NetworkEnemy[] enemies = FindObjectsByType<NetworkEnemy>(FindObjectsSortMode.None);
        int aiEnabled = 0;
        foreach (NetworkEnemy enemy in enemies)
            foreach (MonoBehaviour behaviour in enemy.GetComponents<MonoBehaviour>())
                if (behaviour.enabled && behaviour.GetType().Name.StartsWith("AI")) aiEnabled++;
        Log($"enemies={enemies.Length} spawned={enemies.Count(e => e.IsSpawned)} aiComponentsEnabled={aiEnabled}");
        Log($"RESULT role={role} players={players.Length} localOwned={owned} enemies={enemies.Length} aiEnabled={aiEnabled}");
    }

    private static void Log(string message)
    {
        Debug.Log("[SMOKE " + role + "] " + message);
        File.AppendAllText(logPath, $"[{role}] {message}\n");
    }

    private static string ArgValue(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == name) return args[i + 1];
        return null;
    }
}
