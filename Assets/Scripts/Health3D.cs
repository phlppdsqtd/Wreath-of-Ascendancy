using UnityEngine;

public class Health3D : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float startingHealth = 100f;
    public float currentHealth { get; private set; }
    private Animator anim;
    private bool isDead;

    [Header("Optional References")]
    [SerializeField] private Behaviour[] componentsToDisableOnDeath;

    [Header("Sound Effects")]
    [Tooltip("Played when this entity is hurt")]
    [SerializeField] private AudioClip hurtSFX;
    [Tooltip("Played when this entity dies")]
    [SerializeField] private AudioClip deathSFX;

    public float MaxHealth => startingHealth;

    void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            if (anim) anim.SetTrigger("hurt");

            // Play hurt sound via SoundManager (non-positional)
            if (hurtSFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySound(hurtSFX);

            // If you want 3D positional sound instead:
            // if (hurtSFX != null) AudioSource.PlayClipAtPoint(hurtSFX, transform.position);
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Play death sound
        if (deathSFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySound(deathSFX);

        // Positional alternative:
        // if (deathSFX != null) AudioSource.PlayClipAtPoint(deathSFX, transform.position);

        // Disable attack scripts
        var enemyAttack = GetComponent<EnemyAttack>();
        if (enemyAttack != null) enemyAttack.enabled = false;

        var playerAttack = GetComponent<PlayerAttack3D>();
        if (playerAttack != null) playerAttack.enabled = false;

        // Notify manager (handles Game Over delay for player)
        TypingBattleManager.Instance?.OnEntityDied(this);

        // Trigger death animation
        if (anim) anim.SetTrigger("die");

        // Only despawn enemies after a delay so death animation plays
        if (CompareTag("Enemy"))
            Invoke(nameof(DisableAfterDeath), 2f);
    }

    void DisableAfterDeath()
    {
        gameObject.SetActive(false);
    }

    public void AddHealth(float value)
    {
        currentHealth = Mathf.Clamp(currentHealth + value, 0, startingHealth);
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = startingHealth;
        if (anim)
        {
            anim.ResetTrigger("die");
            anim.Play("Idle");
        }
        foreach (Behaviour component in componentsToDisableOnDeath)
            component.enabled = true;
    }
}
