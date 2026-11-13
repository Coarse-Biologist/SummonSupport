using UnityEngine;

public class RotateToFace : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform LookAtLocation;
    bool initialized = false;
    public void Initialize(Transform goalTransform)
    {
        LookAtLocation = goalTransform;
        initialized = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (initialized && LookAtLocation != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - LookAtLocation.position);
            Destroy(gameObject, 2f);
        }
        else Destroy(this);
    }
}
