using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set;}
    private Animator anim;
    private bool dead;

    [Header ("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;
    
    [Header ("Components")]
    [SerializeField] private Behaviour[] components;
    private bool invulnerable;

    [Header ("Sound")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;

    [Header("Mana Drop (Enemy Only)")]
    [SerializeField] private GameObject manaDropPrefab;
    [SerializeField] private bool isEnemy = false;
    [SerializeField] private Transform dropPoint;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();

        if (isEnemy && UIManager.Instance != null)
        {
            UIManager.Instance.RegisterEnemy(gameObject);
        }
    }

    public void TakeDamage(float _damage)
    {
        if (invulnerable) return;
        
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0) {
            anim.SetTrigger("hurt");
            StartCoroutine(Invulnerability());
            SoundManager.instance.PlaySound(hurtSound);
        }
        else {
            if (!dead)
            {
                foreach (Behaviour component in components)
                {
                    component.enabled = false;
                }

                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                if (isEnemy && manaDropPrefab != null)
                {
                    Instantiate(manaDropPrefab, dropPoint != null ? dropPoint.position : transform.position, Quaternion.identity);
                }
                
                if (gameObject.CompareTag("Player"))
                {
                    anim.SetBool("grounded", true);
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UnregisterEnemy(gameObject);
                }

                anim.SetTrigger("die");
                dead = true;
                SoundManager.instance.PlaySound(deathSound);
            }
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
    
    public void Respawn()
    {
        dead = false;
        AddHealth(startingHealth);
        anim.ResetTrigger("die");
        anim.Play("Idle");
        StartCoroutine(Invulnerability());

        foreach (Behaviour component in components)
        {
            component.enabled = true;
        }
    }

    private IEnumerator Invulnerability()
    {
        invulnerable = true;
        Physics2D.IgnoreLayerCollision(10,11,true);
        for (int i=0; i < numberOfFlashes; i++) {
            spriteRend.color = new Color (1,0,0,0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        Physics2D.IgnoreLayerCollision(10,11,false);
        invulnerable = false;
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
