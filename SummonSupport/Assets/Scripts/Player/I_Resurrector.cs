using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class I_Resurrector : MonoBehaviour
{
    #region Ressurrection Variables
    [SerializeField] public float ResurrectTime { private set; get; } = 5f;
    [SerializeField] public float ResurrectRange { private set; get; } = 7f;
    private WaitForSeconds resurrectionIncrement = new WaitForSeconds(.5f);
    #endregion
    AnimationControllerScript anim;
    bool IsPlayer = false;
    AIStateHandler stateHandler = null;



    void Start()
    {
        if (gameObject.TryGetComponent(out LivingBeing stats))
        {
            if (stats is PlayerStats) IsPlayer = true;
            anim = stats.abilityHandler.anim;
        }
        else
        {
            throw new System.Exception($"You placed {this} on a thing which doesn't have a LivingBeing mono.");
        }
    }
    public void ResurrectMinion(GameObject minion, I_InteractMinionResurrect interactable)
    {
        StartCoroutine(CheckResurrection(minion, interactable));
        if (TryGetComponent(out AIStateHandler sH))
        {
            stateHandler = sH;
            stateHandler.SetStuckInAbilityAnimation(true);
        }
    }
    private IEnumerator CheckResurrection(GameObject minion, I_InteractMinionResurrect interactable)
    {

        if (!minion.TryGetComponent(out MinionStats minionStats))
        {
            Debug.LogWarning("Trying to resurrect a minion without minion stats component. cancelling res");
            yield break;
        }
        bool resurrecting = true;
        float timeWaited = 0;
        float distance;

        if (IsPlayer)
            anim.ChangeLayerAnimation("Crafting", 1, ResurrectTime, true);
        else anim.ChangeLayerAnimation("Buff", 1, ResurrectTime, true);

        while (resurrecting)
        {
            yield return resurrectionIncrement;
            timeWaited += .5f;
            distance = (minion.transform.position - gameObject.transform.position).magnitude;
            if (distance >= ResurrectRange)
            {
                Debug.Log($"Distance to minion = {distance}. setting res succeeding to false.");
                interactable.SetResurrecting(false);
                resurrecting = false;
            }
            if (timeWaited >= ResurrectTime)
            {
                anim.SeLayerWeight(1, 0);
                Debug.Log("Breaking loop because res time has been successfully waited");
                minionStats.Resurrect();
                interactable.SetResurrecting(false);
                resurrecting = false;

            }
        }
        anim.SeLayerWeight(1, 0);
        if (!IsPlayer && stateHandler != null) stateHandler.SetStuckInAbilityAnimation(false);
    }

}
