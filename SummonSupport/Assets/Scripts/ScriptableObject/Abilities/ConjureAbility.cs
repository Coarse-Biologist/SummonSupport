using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Conjure Ability")]
public class ConjureAbility : Ability
{
    [field: Header("Conjure settings"), Tooltip("Ability prefab")]
    [field: SerializeField] public GameObject   ObjectToSpawn  { get; protected set; }
    [field: SerializeField] public Vector2      SpawnOffset    { get; protected set; }

    [field: Tooltip("Activate this if ability should only last a specific amount of time")] 
    [field: SerializeField] public bool         IsDecaying     { get; protected set; }

    [field: Tooltip("in seconds")] 
    [field: SerializeField] public float        TimeAlive      { get; protected set; }


    public override void Activate(GameObject user)
    {
        Vector3 spawnPosition = user.transform.position + (Vector3)SpawnOffset;
        Quaternion rotation = user.transform.rotation;
        Activate(user, spawnPosition, rotation);
    }

    public void Activate(GameObject user, Vector3 spawnPosition, Quaternion rotation)
    {
        GameObject spawnedObject = Instantiate(ObjectToSpawn, spawnPosition, rotation);

        if (IsDecaying)
            AddDecayToObject(spawnedObject);
    }

    public void AddDecayToObject(GameObject spawnedObject)
    {
        SelfDestructTimer selfDestructTimer = spawnedObject.AddComponent<SelfDestructTimer>();
        selfDestructTimer.StartTimer(TimeAlive);
    }


}