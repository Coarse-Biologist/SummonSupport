#region  Imports

using UnityEngine;

#endregion
public class SpawnHandler : MonoBehaviour
{
    #region Class Variables
    private Vector2 WorkBenchLoc = new Vector2(0, 0);
    private AlchemyHandler alchemyHandler;

    #endregion
    #region SetUp
    void Awake()
    {

        if (GameObject.FindWithTag("AlchemyBench") != null)
        {
            WorkBenchLoc = GameObject.FindWithTag("AlchemyBench").transform.position;
            alchemyHandler = GameObject.FindWithTag("AlchemyBench").GetComponent<AlchemyHandler>();
        }
        else Logging.Error("Spawn Handler script is trying to get access to a tag which is not attached to the workbench, or there is no workbench");

    }

    #endregion

    #region  Enable Disable Event Listeners
    void OnEnable()
    {
        if (alchemyHandler != null)
            alchemyHandler.requestInstantiation.AddListener(SpawnMinionAtWorkBench);
    }
    void OnDisable()
    {
        if (alchemyHandler != null)
            alchemyHandler.requestInstantiation.RemoveListener(SpawnMinionAtWorkBench);
    }

    #endregion

    #region Spawn Minion

    private void SpawnMinionAtWorkBench(GameObject minion)
    {
        GameObject theMinion = Instantiate(minion, WorkBenchLoc, Quaternion.identity);
    }

    #endregion



}
