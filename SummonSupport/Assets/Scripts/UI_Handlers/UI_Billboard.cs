using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("FacePlayer", 0f, .1f);
    }

    // Update is called once per frame
    private void FacePlayer()
    {

        Quaternion cameraRotation = Camera.main.transform.rotation;
        //cameraRotation.y = 0; // ignore vertical difference
        transform.rotation = cameraRotation;

    }
}
