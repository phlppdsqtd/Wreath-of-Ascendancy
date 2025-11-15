using UnityEngine;

public class LevelUnlockTrigger : MonoBehaviour
{
    [SerializeField] private int levelNumberToUnlock;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }
}
