using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class PlayerNetworkSetup
{
    private const string PlayerPrefabPath = "Assets/Prefabs/LivingBeings/PlayerPrefab/3DPlayer.prefab";
    private const string Map1Path = "Assets/Scenes/Levels/FirstLevel3D/Map1/Map1.unity";
    private const string StartScenePath = "Assets/Scenes/Levels/StartScreen.unity";

    [MenuItem("Tools/Multiplayer/Build Player Network Setup")]
    public static void Build()
    {
        if (!PrepareMap1(out string leftHandPath, out string rightHandPath)) return;
        GameObject prefab = ConfigurePrefab(leftHandPath, rightHandPath);
        RegisterWithStartScreen(prefab);
        Debug.Log("Player network setup complete.");
    }

    public static void BuildAndExit()
    {
        Build();
        EditorApplication.Exit(0);
    }

    private static GameObject ConfigurePrefab(string leftHandPath, string rightHandPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (root.GetComponent<NetworkObject>() == null) root.AddComponent<NetworkObject>();

        NetworkTransform networkTransform = root.GetComponent<NetworkTransform>();
        if (networkTransform == null) networkTransform = root.AddComponent<NetworkTransform>();
        networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;

        NetworkPlayer networkPlayer = root.GetComponent<NetworkPlayer>();
        if (networkPlayer == null) networkPlayer = root.AddComponent<NetworkPlayer>();

        SerializedObject serialized = new SerializedObject(networkPlayer);
        if (!string.IsNullOrEmpty(leftHandPath)) serialized.FindProperty("leftHandPath").stringValue = leftHandPath;
        if (!string.IsNullOrEmpty(rightHandPath)) serialized.FindProperty("rightHandPath").stringValue = rightHandPath;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
    }

    private static bool PrepareMap1(out string leftHandPath, out string rightHandPath)
    {
        leftHandPath = null;
        rightHandPath = null;
        Scene scene = EditorSceneManager.OpenScene(Map1Path, OpenSceneMode.Single);

        PlayerStats scenePlayer = Object.FindFirstObjectByType<PlayerStats>();
        if (scenePlayer == null)
        {
            if (Object.FindFirstObjectByType<PlayerSpawnPoint>() == null)
                Debug.LogWarning("Map1 has no placed player and no PlayerSpawnPoint; add one manually.");
            return true;
        }

        GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(scenePlayer.gameObject);
        if (root == null) root = scenePlayer.gameObject;

        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (!AllReferencesAreClearable(behaviours, root, out leftHandPath, out rightHandPath))
        {
            Debug.LogError("Map1 has scene references to the placed player that cannot be cleared automatically. Nothing was changed.");
            return false;
        }

        ClearClearableReferences(behaviours, root);

        Vector3 position = root.transform.position;
        Quaternion rotation = root.transform.rotation;
        Undo.DestroyObjectImmediate(root);
        if (Object.FindFirstObjectByType<PlayerSpawnPoint>() == null)
            CreateSpawnPoint(position, rotation);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return true;
    }

    private static bool AllReferencesAreClearable(MonoBehaviour[] behaviours, GameObject root, out string leftHandPath, out string rightHandPath)
    {
        leftHandPath = null;
        rightHandPath = null;
        bool clearable = true;

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || IsUnder(behaviour.gameObject, root)) continue;

            SerializedObject serialized = new SerializedObject(behaviour);
            SerializedProperty property = serialized.GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (!ReferencesPlayer(property.objectReferenceValue, root)) continue;

                if (behaviour is AlchemyObjectInteraction && property.name == "playerHand_L")
                    leftHandPath = PathFromPrefabSource(property.objectReferenceValue, root);
                else if (behaviour is AlchemyObjectInteraction && property.name == "playerHand_R")
                    rightHandPath = PathFromPrefabSource(property.objectReferenceValue, root);
                else if (!IsClearableReference(behaviour, property.propertyPath))
                {
                    Debug.LogWarning($"{behaviour.name} ({behaviour.GetType().Name}).{property.propertyPath} references the placed player.");
                    clearable = false;
                }
            }
        }
        return clearable;
    }

    private static void ClearClearableReferences(MonoBehaviour[] behaviours, GameObject root)
    {
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || IsUnder(behaviour.gameObject, root)) continue;

            SerializedObject serialized = new SerializedObject(behaviour);
            SerializedProperty property = serialized.GetIterator();
            bool changed = false;
            while (property.NextVisible(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (!ReferencesPlayer(property.objectReferenceValue, root)) continue;

                property.objectReferenceValue = null;
                changed = true;
            }
            if (changed) serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static bool IsClearableReference(MonoBehaviour behaviour, string propertyPath)
    {
        if (behaviour is CinemachineCamera) return propertyPath == "Target.TrackingTarget";
        if (behaviour is UniversalAdditionalCameraData) return propertyPath == "m_VolumeTrigger";
        if (behaviour is AlchemyObjectInteraction) return propertyPath == "playerHand_L" || propertyPath == "playerHand_R";
        return false;
    }

    private static bool ReferencesPlayer(Object target, GameObject root)
    {
        if (target == null) return false;
        GameObject targetGO = target as GameObject;
        if (targetGO == null && target is Component component) targetGO = component.gameObject;
        return targetGO != null && IsUnder(targetGO, root);
    }

    private static string PathFromPrefabSource(Object target, GameObject root)
    {
        Transform hand = target as Transform;
        if (hand == null) return null;

        Transform sourceHand = PrefabUtility.GetCorrespondingObjectFromSource(hand);
        Transform sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(root.transform);
        if (sourceHand == null || sourceRoot == null || !sourceHand.IsChildOf(sourceRoot))
        {
            Debug.LogWarning($"Hand transform {hand.name} is not inside the player prefab; its path cannot be recorded.");
            return null;
        }

        string path = sourceHand.name;
        Transform parent = sourceHand.parent;
        while (parent != null && parent != sourceRoot)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private static void CreateSpawnPoint(Vector3 position, Quaternion rotation)
    {
        GameObject spawn = new GameObject("PlayerSpawnPoint");
        spawn.transform.SetPositionAndRotation(position, rotation);
        spawn.AddComponent<PlayerSpawnPoint>();
        Undo.RegisterCreatedObjectUndo(spawn, "Create player spawn point");
    }

    private static bool IsUnder(GameObject go, GameObject root)
    {
        return go == root || go.transform.IsChildOf(root.transform);
    }

    private static void RegisterWithStartScreen(GameObject prefab)
    {
        Scene scene = EditorSceneManager.OpenScene(StartScenePath, OpenSceneMode.Single);

        NetworkManager networkManager = Object.FindFirstObjectByType<NetworkManager>();
        if (networkManager == null) throw new System.Exception("No NetworkManager in StartScreen. Run 'Build Start Screen Connection UI' first.");

        Undo.RecordObject(networkManager, "Register player prefab");
        if (!networkManager.NetworkConfig.Prefabs.Contains(prefab))
            networkManager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = prefab });
        EditorUtility.SetDirty(networkManager);

        GameObject managerGO = networkManager.gameObject;
        NetworkPlayerSpawner spawner = managerGO.GetComponent<NetworkPlayerSpawner>();
        if (spawner == null) spawner = Undo.AddComponent<NetworkPlayerSpawner>(managerGO);

        SerializedObject serialized = new SerializedObject(spawner);
        serialized.FindProperty("playerPrefab").objectReferenceValue = prefab.GetComponent<NetworkObject>();
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
