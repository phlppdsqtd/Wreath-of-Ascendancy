using UnityEngine;

public class EnemyUIFollow : MonoBehaviour
{
    public Transform target;   // set by EnemyUIController
    public Vector3 offset = new Vector3(0, 2.0f, 0);
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || cam == null)
        {
            // hide offscreen if something's wrong
            transform.position = new Vector3(-9999f, -9999f, 0f);
            return;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(target.position + offset);

        // If behind camera, move off screen
        if (screenPos.z < 0f)
            transform.position = new Vector3(-9999f, -9999f, 0f);
        else
            transform.position = screenPos;
    }
}
