using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerScript : MonoBehaviour
{
    public Animator anim { get; private set; }
    public string currentMovementAnimation;
    private List<int> layersToChange = new();
    private List<float> animationDurations = new();

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null) throw new System.Exception($"Animation controller is null. it was not found among children objects.");
    }
    public void ChangeAnimation(string animationName, float crossFadeDuration = .2f)
    {
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


    public void ChangeLayerAnimation(string animationName, int layerIndex, float animationDuration = 1f)
    {
        StartCoroutine(ChangeLayerRoutine(layerIndex, animationName, animationDuration));
    }

    private IEnumerator ChangeLayerRoutine(int layerIndex, string animationName, float duration)
    {
        anim.SetLayerWeight(layerIndex, 1);

        anim.Play(animationName, layerIndex, 0f);

        //anim.CrossFade(animationName, 0.2f, layerIndex);

        yield return new WaitForSeconds(duration);

        anim.SetLayerWeight(layerIndex, 0);
    }
}
