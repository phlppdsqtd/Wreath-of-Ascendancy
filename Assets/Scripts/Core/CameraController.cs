using UnityEngine;

public class CameraController : MonoBehaviour
{   
    [SerializeField] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float cameraSpeed;
    private float lookAhead;

    private void Update()
    {
        transform.position = new Vector3(player.position.x, player.position.y+2.5f, transform.position.z);
    }

    public void MoveToNewRoom(Transform _newRoom) {
        currentPosX = _newRoom.position.x + 4.43f;
    }
}
