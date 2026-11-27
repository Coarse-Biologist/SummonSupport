using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps SetupManager.AllAbilities holding every Ability asset in the project.
///
/// Casts are sent over the network as an index into that list (see AbilityRegistry), so an ability missing
/// from it can't be shown to other players. Filling the list by hand kept leaving abilities out, so this fills it:
///  - when a scene is saved, so the saved scene stays complete;
///  - when a scene is processed for a build or for entering Play mode, so a stale saved scene still works;
///  - from the menu: SummonSupport > Refresh Ability Registry.
///
/// Existing entries keep their order and missing abilities are added at the end, sorted by asset path.
/// Every machine built from the same project therefore ends up with the same list.
/// </summary>
public class AbilityRegistrySync : IProcessSceneWithReport
{
    private const string AllAbilitiesProperty = "<AllAbilities>k__BackingField";

    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        SyncScene(scene, recordUndo: false);
    }

    [InitializeOnLoadMethod]
    private static void HookSceneSaving()
    {
        EditorSceneManager.sceneSaving += (scene, path) => SyncScene(scene, recordUndo: false);
    }

    [MenuItem("SummonSupport/Refresh Ability Registry")]
    private static void RefreshOpenScenes()
    {
        int added = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            added += SyncScene(SceneManager.GetSceneAt(i), recordUndo: true);
        Debug.Log($"Ability registry refreshed: {added} abilities added to SetupManager.AllAbilities in the open scenes.");
    }

    /// <summary>Returns how many abilities were added.</summary>
    private static int SyncScene(Scene scene, bool recordUndo)
    {
        if (!scene.IsValid() || !scene.isLoaded) return 0;

        List<Ability> allAbilities = null;
        int added = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (SetupManager setupManager in root.GetComponentsInChildren<SetupManager>(true))
            {
                allAbilities ??= FindAllAbilities();
                added += SyncSetupManager(setupManager, allAbilities, recordUndo);
            }
        }

        return added;
    }

    private static int SyncSetupManager(SetupManager setupManager, List<Ability> allAbilities, bool recordUndo)
    {
        SerializedObject serialized = new SerializedObject(setupManager);
        SerializedProperty list = serialized.FindProperty(AllAbilitiesProperty);
        if (list == null || !list.isArray)
        {
            Debug.LogWarning($"AbilityRegistrySync could not find {AllAbilitiesProperty} on {setupManager.name}.");
            return 0;
        }

        List<Ability> current = new List<Ability>();
        for (int i = 0; i < list.arraySize; i++)
        {
            Ability ability = list.GetArrayElementAtIndex(i).objectReferenceValue as Ability;
            if (ability != null && !current.Contains(ability)) current.Add(ability);
        }

        int added = 0;
        foreach (Ability ability in allAbilities)
        {
            if (current.Contains(ability)) continue;
            current.Add(ability);
            added++;
        }

        // Nothing added and no empty or duplicate entries removed: leave the scene untouched.
        if (added == 0 && current.Count == list.arraySize) return 0;

        list.arraySize = current.Count;
        for (int i = 0; i < current.Count; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = current[i];

        if (recordUndo) serialized.ApplyModifiedProperties();
        else serialized.ApplyModifiedPropertiesWithoutUndo();

        return added;
    }

    private static List<Ability> FindAllAbilities()
    {
        return AssetDatabase.FindAssets("t:" + nameof(Ability))
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<Ability>)
            .Where(ability => ability != null)
            .ToList();
    }
}
