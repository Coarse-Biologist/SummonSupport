using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour, I_Destruction, IElementalModelListener
{
    [field: SerializeField] public Animator animator;
    private List<Quaternion> startingRotations = new();
    private List<Vector3> startingPositions = new();
    [field: SerializeField] public float ragdollFreedom = 2f;
    [field: SerializeField] public Collider mainCollider;
    [field: SerializeField] public List<Rigidbody> rigidbodies = new();
    [field: SerializeField] public List<Collider> colliders = new();

    private Animator defaultAnimator;
    private List<Rigidbody> defaultRigidbodies;
    private List<Collider> defaultColliders;

    public void OnElementalModelChanged(ElementalModel current, bool isBaseModel)
    {
        if (current == null) return;
        if (defaultRigidbodies == null)
        {
            defaultAnimator = animator;
            defaultRigidbodies = new List<Rigidbody>(rigidbodies);
            defaultColliders = new List<Collider>(colliders);
        }

        if (current.RagdollBodies.Count > 0)
        {
            animator = current.Animator;
            rigidbodies = new List<Rigidbody>(current.RagdollBodies);
            colliders = new List<Collider>(current.RagdollColliders);
        }
        else if (isBaseModel)
        {
            animator = defaultAnimator;
            rigidbodies = new List<Rigidbody>(defaultRigidbodies);
            colliders = new List<Collider>(defaultColliders);
        }
        else
        {
            animator = current.Animator;
            rigidbodies.Clear();
            colliders.Clear();
        }
        startingPositions.Clear();
        startingRotations.Clear();
    }

    public void CauseDestruction(bool SaveTransformData = false)
    {
        //Debug.Log($"causing destruction");
        if (SaveTransformData) SavePreRagdollTransformData();

        RagDoll(true);
    }

    private void RagDoll(bool isRagdoll)
    {
        if (animator != null)
            animator.enabled = !isRagdoll;
        if (mainCollider != null) mainCollider.enabled = false;

        Invoke("EnableMainCollider", ragdollFreedom);

        if (colliders.Count != 0)
            foreach (Collider collider in colliders)
            {
                collider.enabled = isRagdoll;
            }
        if (rigidbodies.Count == 0) return;

        else foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !isRagdoll;
            rb.useGravity = isRagdoll;
            if (isRagdoll)
            {
                Vector3 randomDirection = Random.insideUnitSphere.normalized;
                rb.AddForce(randomDirection * 3, ForceMode.Impulse);
            }
        }
    }
    private void EnableMainCollider()
    {
        if (mainCollider != null)
            mainCollider.enabled = true;
    }
    public void SavePreRagdollTransformData()
    {
        startingRotations.Clear();
        startingPositions.Clear();
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            startingRotations.Add(rigidbodies[i].transform.localRotation);
            startingPositions.Add(rigidbodies[i].transform.localPosition);
        }
    }
    public void ReverseDestruction()
    {
        if (rigidbodies != null && rigidbodies.Count > 0)
        {
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                rigidbodies[i].transform.localPosition = startingPositions[i];
                rigidbodies[i].transform.localRotation = startingRotations[i];
            }
            RagDoll(false);
        }
    }
}
