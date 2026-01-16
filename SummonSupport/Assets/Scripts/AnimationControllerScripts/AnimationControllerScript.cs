using System;
using System.Collections;
using Unity.InferenceEngine;
using UnityEditor.Animations;
using UnityEngine;

public class AnimationControllerScript : MonoBehaviour
{
    public Animator anim { get; private set; }
    [field: SerializeField] string AnimatorStringLine;
    public string currentMovementAnimation;


    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null) throw new System.Exception($"Animation controller is null. it was not found among children objects.");
anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }
    private void PrintStateMachine(AnimatorStateMachine sm, string layerName)
    {
        foreach (var state in sm.states)
        {
            Debug.Log($"{gameObject.name} Layer: {layerName} | State: {state.state.name}");
        }

        foreach (var sub in sm.stateMachines)
        {
            PrintStateMachine(sub.stateMachine, layerName);
        }
    }


    public void ChangeAnimation(string animationName, float crossFadeDuration = .2f)
    {
        if (anim != null)
            if (currentMovementAnimation != animationName)
            {

                currentMovementAnimation = animationName;
                anim.CrossFade(animationName, crossFadeDuration);
            }
    }
    public void ChangeMovementAnimation(float moveInputX, float moveInputY)
    {
        if (moveInputY == 1 && moveInputX == 0)
        {
            ChangeAnimation("RunForward");
        }
        else if (moveInputY == -1 && moveInputX == 0)
        {
            ChangeAnimation("RunBackward");
        }
        else if (moveInputX == -1 && moveInputY == 0)
        {
            ChangeAnimation("StrafeLeft");
        }
        else if (moveInputX == 1 && moveInputY == 0)
        {
            ChangeAnimation("StrafeRight"); // good
        }
        else if (moveInputX > 0 && moveInputY < 0)
        {
            ChangeAnimation("RunBackwardRight");
        }
        else if (moveInputX < 0 && moveInputY < 0)
        {
            ChangeAnimation("RunBackwardLeft");
        }
        else if (moveInputX < 0 && moveInputY > 0)
        {
            ChangeAnimation("RunRight");
        }
        else if (moveInputX > 0 && moveInputY > 0)
        {
            ChangeAnimation("RunLeft");
        }
    }

    public void ChangeLayerAnimation(string animationName, int layerIndex, float animationDuration = 1f, bool holdPose = false)
    {
        if (anim.layerCount != 1)
            StartCoroutine(ChangeLayerRoutine(layerIndex, animationName, animationDuration, holdPose));
    }
    public void SeLayerWeight(int layerIndex, float weight)
    {
        anim.SetLayerWeight(layerIndex, weight);

    }

    private IEnumerator ChangeLayerRoutine(int layerIndex, string animationName, float duration, bool holdPose = false)
    {
        anim.SetLayerWeight(layerIndex, 1);

        anim.Play(animationName, layerIndex, 0);
        //if (holdPose) StartCoroutine(GoStopGo(layerIndex, duration));

        //anim.CrossFade(animationName, 0.2f, layerIndex);

        yield return new WaitForSeconds(duration);

        if (holdPose == false) anim.SetLayerWeight(layerIndex, 0);
    }
}
