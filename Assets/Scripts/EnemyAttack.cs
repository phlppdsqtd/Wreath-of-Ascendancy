using UnityEngine;
using TMPro;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text enemyText;

    [Header("Attack Settings")]
    public float attackInterval = 3f;
    public float damagePerAttack = 10f;
    private float timer;
    private Coroutine flashCoroutine;

    [Header("References")]
    public Health3D playerHealth;
    public Health3D enemyHealth;
    private Animator anim;  // cache animator

    [Header("Sound Effects")]
    [Tooltip("Attack sound for this enemy")]
    [SerializeField] private AudioClip attackSFX;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponent<Health3D>();

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Stop attacking if enemy or player dead/missing
        if (enemyHealth != null && enemyHealth.currentHealth <= 0f) return;
        if (playerHealth == null || playerHealth.currentHealth <= 0f) return;

        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            AttackPlayer();
            timer = 0f;
        }
    }

    void AttackPlayer()
    {
        if (playerHealth == null || playerHealth.currentHealth <= 0f) return;

        string attackerName = gameObject.name;

        // Trigger attack animation
        if (anim != null) anim.SetTrigger("attack");

        // Play attack sound via SoundManager (non-positional)
        if (attackSFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySound(attackSFX);

        // Positional alternative:
        // if (attackSFX != null) AudioSource.PlayClipAtPoint(attackSFX, transform.position);

        // Deal damage to player
        playerHealth.TakeDamage(damagePerAttack);

        CombatLogManager.Instance?.AddLog($"<color=red>{attackerName}</color> attacks <color=yellow>The Barbarian</color> for <b>{damagePerAttack}</b> damage!");
    }

    public void TakeDamage(float damage)
    {
        Health3D h = GetComponent<Health3D>();
        if (h != null)
            h.TakeDamage(damage);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
    }
}
