using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Gives the player the profile's starter minions (by default one element-less minion that only knows Slash).
/// Each is described as a minion profile and brought in the way a saved minion is (AlchemyHandler.RestoreSavedMinion),
/// so online the host spawns it and it belongs to this player, like a crafted one.
/// </summary>
public class StarterMinionStep : ICharacterCreationStep
{
    public void Apply(CharacterCreationContext context)
    {
        AlchemyHandler alchemy = AlchemyHandler.Instance;
        if (context.Profile.StarterMinions.Count == 0) return;
        if (alchemy == null || alchemy.minionPrefab == null || !alchemy.minionPrefab.TryGetComponent(out MinionStats prefabStats))
            throw new Exception("No AlchemyHandler with a minion prefab in the level, so there are no starter minions.");

        int spawned = 0;
        foreach (StarterMinionDefinition definition in context.Profile.StarterMinions)
        {
            MinionProfileData profile = BuildProfile(prefabStats, definition);
            for (int i = 0; i < definition.Count; i++)
            {
                Vector3 position = SpawnPosition(context.Player.transform, definition.SpawnDistanceFromPlayer, spawned++);
                alchemy.RestoreSavedMinion(new MinionData { profile = profile, location = position, rotation = context.Player.transform.rotation });
            }
        }
    }

    // The minion prefab's own maximums and regeneration, no affinities, and only the given abilities.
    private static MinionProfileData BuildProfile(MinionStats prefabStats, StarterMinionDefinition definition)
    {
        int[] abilities = ResolveAbilities(definition).Select(AbilityRegistry.IndexOf).Where(i => i >= 0).ToArray();
        return new MinionProfileData
        {
            maxHitpoints = prefabStats.MaxHP,
            maxPower = prefabStats.MaxPower,
            healthRegeneration = prefabStats.HealthRegeneration,
            powerRegeneration = prefabStats.PowerRegeneration,
            bodyScale = 1f,
            name = definition.DisplayName ?? "",
            affinities = new float[MinionProfileData.ProfileElements.Length],
            knownAbilities = abilities,
            slotNumbers = Enumerable.Range(0, abilities.Length).ToArray(),
            slottedAbilities = abilities
        };
    }

    private static IEnumerable<Ability> ResolveAbilities(StarterMinionDefinition definition)
    {
        if (definition.Abilities.Count > 0) return definition.Abilities.Where(a => a != null);

        Ability defaultAttack = AbilityLibrary.abilityLibrary != null ? AbilityLibrary.abilityLibrary.defaultAttack : null;
        if (defaultAttack == null) throw new Exception("The starter minion has no abilities and the ability library has no default attack.");
        return new[] { defaultAttack };
    }

    // Beside the player, fanning out for each extra minion, on the navmesh if there is one nearby.
    private static Vector3 SpawnPosition(Transform player, float distance, int index)
    {
        Vector3 offset = Quaternion.AngleAxis(90f + index * 45f, Vector3.up) * player.forward * distance;
        Vector3 position = player.position + offset;
        return NavMesh.SamplePosition(position, out NavMeshHit hit, distance * 2f, NavMesh.AllAreas) ? hit.position : position;
    }
}
