using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class AnimationControllerScript : NetworkBehaviour
{
    public Animator anim { get; private set; }
    [field: SerializeField] string AnimatorStringLine;
    public string currentMovementAnimation;

    private readonly NetworkVariable<FixedString64Bytes> networkMovement = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<LayerAnimation> networkLayer = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct LayerAnimation : IEquatable<LayerAnimation>, INetworkSerializeByMemcpy
    {
        public FixedString64Bytes clip;
        public int layer;
        public float duration;
        public bool hold;
        public int seq;

        public bool Equals(LayerAnimation other) => seq == other.seq && clip == other.clip && layer == other.layer;
    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null) throw new System.Exception($"Animation controller is null. it was not found among children objects.");
    }

    public override void OnNetworkSpawn()
    {
        networkMovement.OnValueChanged += OnRemoteMovement;
        networkLayer.OnValueChanged += OnRemoteLayer;
        if (!IsOwner && networkMovement.Value.Length > 0) PlayMovementLocal(networkMovement.Value.ToString());
    }

    public override void OnNetworkDespawn()
    {
        networkMovement.OnValueChanged -= OnRemoteMovement;
        networkLayer.OnValueChanged -= OnRemoteLayer;
    }

    public void SetUpdateMode(AnimatorUpdateMode mode)
    {
        anim.updateMode = mode;
    }

    public void ChangeAnimation(string animationName, float crossFadeDuration = .2f)
    {
        if (anim != null)
            if (currentMovementAnimation != animationName)
            {
                currentMovementAnimation = animationName;
                anim.CrossFade(animationName, crossFadeDuration);
                if (IsSpawned && IsOwner) networkMovement.Value = animationName;
            }
    }

    public void ChangeMovementAnimation(float moveInputX, float moveInputY)
    {
        // Pick the nearest of the eight run directions. Comparing exact values only worked for keys: a stick
        // pushed forward reads e.g. (0.03, 0.98), which flickered between the two forward-diagonal runs.
        if (moveInputX * moveInputX + moveInputY * moveInputY < 0.0001f) return;
        int direction = Mathf.RoundToInt(Mathf.Atan2(moveInputY, moveInputX) * Mathf.Rad2Deg / 45f); // 0 = right, 2 = forward, -2 = back
        switch (direction)
        {
            case 2: ChangeAnimation("RunForward"); break;
            case -2: ChangeAnimation("RunBackward"); break;
            case 4:
            case -4: ChangeAnimation("StrafeLeft"); break;
            case 0: ChangeAnimation("StrafeRight"); break;
            case -1: ChangeAnimation("RunBackwardRight"); break;
            case -3: ChangeAnimation("RunBackwardLeft"); break;
            case 3: ChangeAnimation("RunRight"); break; // forward-left
            case 1: ChangeAnimation("RunLeft"); break; // forward-right
        }
    }

    public void ChangeLayerAnimation(string animationName, int layerIndex, float animationDuration = 1f, bool holdPose = false)
    {
        if (anim.layerCount == 1) return;
        PlayLayerLocal(animationName, layerIndex, animationDuration, holdPose);
        if (IsSpawned && IsOwner)
        {
            networkLayer.Value = new LayerAnimation
            {
                clip = animationName,
                layer = layerIndex,
                duration = animationDuration,
                hold = holdPose,
                seq = networkLayer.Value.seq + 1
            };
        }
    }

    public void SeLayerWeight(int layerIndex, float weight)
    {
        anim.SetLayerWeight(layerIndex, weight);
    }

    private void OnRemoteMovement(FixedString64Bytes previous, FixedString64Bytes next)
    {
        if (IsOwner) return;
        PlayMovementLocal(next.ToString());
    }

    private void OnRemoteLayer(LayerAnimation previous, LayerAnimation next)
    {
        if (IsOwner || anim == null || anim.layerCount == 1) return;
        PlayLayerLocal(next.clip.ToString(), next.layer, next.duration, next.hold);
    }

    private void PlayMovementLocal(string animationName)
    {
        if (anim == null || currentMovementAnimation == animationName) return;
        currentMovementAnimation = animationName;
        anim.CrossFade(animationName, .2f);
    }

    private void PlayLayerLocal(string animationName, int layerIndex, float animationDuration, bool holdPose)
    {
        StartCoroutine(ChangeLayerRoutine(layerIndex, animationName, animationDuration, holdPose));
    }

    private IEnumerator ChangeLayerRoutine(int layerIndex, string animationName, float duration, bool holdPose = false)
    {
        anim.SetLayerWeight(layerIndex, 1);

        anim.Play(animationName, layerIndex, 0);

        yield return new WaitForSeconds(duration);

        if (holdPose == false) anim.SetLayerWeight(layerIndex, 0);
    }
}
