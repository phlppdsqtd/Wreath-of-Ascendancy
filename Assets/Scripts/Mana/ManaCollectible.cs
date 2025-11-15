using UnityEngine;

public class ManaCollectible : MonoBehaviour
{
    [SerializeField] private float manaValue;

    [Header("VFX")]
    [SerializeField] private GameObject pickupVFX;
    [Header("SFX")]
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Mana playerMana = collision.GetComponent<Mana>();
            if (playerMana != null)
                playerMana.AddMana(manaValue);

            if (SoundManager.instance != null && pickupSound != null)
                SoundManager.instance.PlaySound(pickupSound);

            if (pickupVFX != null)
            {
                GameObject vfx = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            }
                
            gameObject.SetActive(false);
            
            if (UIManager.Instance != null && gameObject.CompareTag("Collectible"))
                UIManager.Instance.UnregisterCollectible(gameObject);
        }
    }
}
