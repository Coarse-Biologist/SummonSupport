#region  Imports

using UnityEngine;

#endregion
public class SpawnHandler : MonoBehaviour
{
    #region Class Variables
    private Vector2 WorkBenchLoc;

    #endregion

    #region  Enable Disable Event Listeners
    void OnEnable()
    {
        AlchemyHandler.requestInstantiation.AddListener(SpawnMinionAtWorkBench);
    }
    void OnDisable()
    {
        AlchemyHandler.requestInstantiation.RemoveListener(SpawnMinionAtWorkBench);
    }

    #endregion

    #region Spawn Minion

    private void SpawnMinionAtWorkBench(GameObject minion)
    {
        GameObject theMinion = Instantiate(minion, WorkBenchLoc, Quaternion.identity);
    }

    #endregion

    #region SetUp
    void Start()
    {
        WorkBenchLoc = GameObject.FindWithTag("AlchemyBench").transform.position;
    }

    #endregion

}
