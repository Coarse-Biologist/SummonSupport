using UnityEngine;

public class GismoDisplay : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Draw the local X-axis (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 2);

        // Draw the local Y-axis (Green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 2);

        // Draw the local Z-axis (Blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
    }
}
