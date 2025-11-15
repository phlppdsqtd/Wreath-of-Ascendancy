using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}