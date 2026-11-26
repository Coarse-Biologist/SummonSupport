using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreatureNetworkSetup
{
    private const string StartScenePath = "Assets/Scenes/Levels/StartScreen.unity";

    [MenuItem("Tools/Multiplayer/Build Creature Network Setup")]
    public static void Build()
    {
        List<GameObject> creatures = new();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("PlayerPrefab")) continue;

            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null || asset.GetComponent<LivingBeing>() == null || asset.GetComponent<PlayerStats>() != null) continue;

            if (asset.GetComponentsInChildren<NetworkObject>(true).Length > 0 && asset.GetComponent<NetworkObject>() == null)
            {
                Debug.LogWarning($"Skipped {path}: contains nested NetworkObjects.");
                continue;
            }

            ConfigureCreature(path);
            creatures.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        }

        RegisterWithStartScreen(creatures);
        Debug.Log($"Creature network setup complete: {creatures.Count} prefabs.");
    }

    public static void BuildAndExit()
    {
        Build();
        EditorApplication.Exit(0);
    }

    private static void ConfigureCreature(string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root.GetComponent<NetworkObject>() == null) root.AddComponent<NetworkObject>();
        if (root.GetComponent<NetworkTransform>() == null) root.AddComponent<NetworkTransform>();
        if (root.GetComponent<NetworkEnemy>() == null) root.AddComponent<NetworkEnemy>();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void RegisterWithStartScreen(List<GameObject> creatures)
    {
        Scene scene = EditorSceneManager.OpenScene(StartScenePath, OpenSceneMode.Single);
        NetworkManager networkManager = Object.FindFirstObjectByType<NetworkManager>();
        if (networkManager == null) throw new System.Exception("No NetworkManager in StartScreen. Run 'Build Start Screen Connection UI' first.");

        Undo.RecordObject(networkManager, "Register creature prefabs");
        foreach (GameObject creature in creatures)
        {
            if (!networkManager.NetworkConfig.Prefabs.Contains(creature))
                networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = creature });
        }
        EditorUtility.SetDirty(networkManager);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
