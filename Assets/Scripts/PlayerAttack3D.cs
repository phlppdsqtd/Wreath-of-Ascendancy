using UnityEngine;

public class PlayerAttack3D : MonoBehaviour
{
    [Header("Attack Settings")]
    public int baseDamage;              // default damage per correct word
    public float difficultyMultiplier = 1f;     // can scale based on word length

    [Header("References")]
    public Health3D playerHealth;               // optional: for self effects later

    [Header("Sound Effects")]
    [Tooltip("Attack sound for the player")]
    [SerializeField] private AudioClip attackSFX;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Deals damage to a specific enemy target.
    /// </summary>
    public void DealDamageTo(GameObject target, float damageOverride = -1f)
    {
        if (target == null) return;

        // Trigger attack animation
        if (anim != null) anim.SetTrigger("attack");

        // Play attack sound (non-positional through SoundManager)
        if (attackSFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySound(attackSFX);

        // Positional alternative:
        // if (attackSFX != null) AudioSource.PlayClipAtPoint(attackSFX, transform.position);

        float damageToDeal = (damageOverride > 0f)
            ? damageOverride
            : baseDamage * difficultyMultiplier;

        EnemyAttack enemy = target.GetComponent<EnemyAttack>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageToDeal);
            return;
        }

        Health3D h = target.GetComponent<Health3D>();
        if (h != null)
        {
            h.TakeDamage(damageToDeal);
            //Debug.Log($"{target.name} takes {damageToDeal} damage (direct).");
        }
    }

    /// <summary>
    /// Optional helper to calculate damage based on word difficulty.
    /// Example: longer words = more damage.
    /// </summary>
    public int DamageForWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return baseDamage;

        // Base damage scales with word length
        float lengthFactor = Mathf.Sqrt(word.Length) * 0.6f;

        // Add controlled random variance (±20%)
        float variance = Random.Range(0.8f, 1.2f);

        // Bonus damage for longer words, capped
        float bonus = Mathf.Min(lengthFactor, 4f);

        // Combine everything
        float finalDamage = (baseDamage + bonus) * variance;

        return Mathf.RoundToInt(finalDamage);
    }
}
