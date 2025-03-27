using UnityEngine;
using System.Collections;
[CreateAssetMenu(menuName = "Abilities/Conjure Ability")]
public class ConjureAbility : Ability
{
    [field: SerializeField, Header("Conjure settings"), Tooltip("Ability prefab")]  
    public GameObject   ObjectToSpawn  { get; protected set; }
    [field: SerializeField]
    public Vector2      SpawnOffset    { get; protected set; }
    [field: SerializeField, Tooltip("Activate this if ability should only last a specific amount of time")] 
    public bool         IsDecaying     { get; protected set; }
    [field: SerializeField, Tooltip("in seconds")] public float        TimeAlive      { get; protected set; }

    public override void Activate(GameObject user)
    {
        Vector3 spawnPosition = user.transform.position + (Vector3)SpawnOffset;
        GameObject spawnedObject = Instantiate(ObjectToSpawn, spawnPosition, Quaternion.identity);

        if (IsDecaying)
            AddDecayToObject(spawnedObject);
    }

    public void AddDecayToObject(GameObject spawnedObject)
    {
        SelfDestructTimer selfDestructTimer = spawnedObject.AddComponent<SelfDestructTimer>();
        selfDestructTimer.StartTimer(TimeAlive);
    }


}