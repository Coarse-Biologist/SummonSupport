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
    /// <summary>Resurrects a dead minion or a downed player, if this being stays close for ResurrectTime.</summary>
    public void Resurrect(GameObject target, I_InteractResurrect interactable)
    {
        StartCoroutine(CheckResurrection(target, interactable));
        if (TryGetComponent(out AIStateHandler sH))
        {
            stateHandler = sH;
            stateHandler.SetStuckInAbilityAnimation(true);
        }
    }
    private IEnumerator CheckResurrection(GameObject target, I_InteractResurrect interactable)
    {

        if (!target.TryGetComponent(out LivingBeing targetStats))
        {
            Debug.LogWarning("Trying to resurrect something without a LivingBeing component. cancelling res");
            interactable.SetResurrecting(false);
            yield break;
        }
        LivingBeing ownStats = GetComponent<LivingBeing>();
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
            if (target == null || interactable == null) break;
            distance = (target.transform.position - gameObject.transform.position).magnitude;
            // Stop if this being moved away or fell, or someone else already brought the target back.
            if (distance >= ResurrectRange || ownStats.Dead || !targetStats.Dead)
            {
                Debug.Log($"Distance to minion = {distance}. setting res succeeding to false.");
                interactable.SetResurrecting(false);
                resurrecting = false;
            }
            if (timeWaited >= ResurrectTime)
            {
                anim.SeLayerWeight(1, 0);
                Debug.Log("Breaking loop because res time has been successfully waited");
                if (targetStats is MinionStats minionStats) minionStats.Resurrect();
                else if (targetStats is PlayerStats playerStats) playerStats.Resurrect();
                interactable.SetResurrecting(false);
                resurrecting = false;

            }
        }
        anim.SeLayerWeight(1, 0);
        if (!IsPlayer && stateHandler != null) stateHandler.SetStuckInAbilityAnimation(false);
    }

}
