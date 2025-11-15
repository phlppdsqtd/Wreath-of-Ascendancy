using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private float parallaxFactor;

    private Vector3 startPosition;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        startPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 camMovement = cam.position * parallaxFactor;
        transform.position = new Vector3(startPosition.x + camMovement.x, startPosition.y + camMovement.y, startPosition.z);
    }
}