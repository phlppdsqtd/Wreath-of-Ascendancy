using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Ranged Attack Parameters")]
    [SerializeField] private float rangedCooldown;
    [SerializeField] private float rangedRange;
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject[] projectiles;
    [SerializeField] private AudioClip projectileSound;

    [Header("Melee Attack Parameters")]
    [SerializeField] private float meleeCooldown;
    [SerializeField] private float meleeRange;
    [SerializeField] private int meleeDamage;
    [SerializeField] private AudioClip meleeSound;

    [Header("Collider Parameters")]
    [SerializeField] private float rangedColliderDistance;
    [SerializeField] private float meleeColliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;

    private float rangedTimer = Mathf.Infinity;
    private float meleeTimer = Mathf.Infinity;

    private Animator anim;
    private Health playerHealth;
    private BossChaseController chaseController;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        chaseController = GetComponent<BossChaseController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Transform playerTransform = playerObj.transform;
            chaseController.SetPlayer(playerTransform);
            playerHealth = playerTransform.GetComponent<Health>(); // <- Add this line
        }
    }

    private void Update()
    {
        rangedTimer += Time.deltaTime;
        meleeTimer += Time.deltaTime;

        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            bool inRanged = IsPlayerInRanged();
            bool inMelee = IsPlayerInMelee();

            if (inRanged && rangedTimer >= rangedCooldown)
            {
                rangedTimer = 0;
                anim.SetTrigger("rangedAttack");
                chaseController.CanMove = false;
            }
            else if (inMelee && meleeTimer >= meleeCooldown && (!inRanged || rangedTimer < rangedCooldown))
            {
                meleeTimer = 0;
                anim.SetTrigger("meleeAttack");
                SoundManager.instance.PlaySound(meleeSound);
                chaseController.CanMove = false;
            }
            else
            {
                chaseController.CanMove = true;
            }
        }
    }

    private void RangedAttack()
    {
        SoundManager.instance.PlaySound(projectileSound);
        GameObject projectile = projectiles[FindProjectile()];
        projectile.transform.position = firepoint.position;
        projectile.GetComponent<EnemyProjectile>().ActivateProjectile();
    }

    private void MeleeDamage()
    {
        if (IsPlayerInMelee())
        {
            playerHealth.TakeDamage(meleeDamage);
        }
    }

    private bool IsPlayerInRanged()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)boxCollider.bounds.center + direction * rangedRange * rangedColliderDistance;
        Vector2 size = new Vector2(boxCollider.bounds.size.x * rangedRange, boxCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.zero, 0, playerLayer);
        return hit.collider != null;
    }

    private bool IsPlayerInMelee()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)boxCollider.bounds.center + direction * meleeRange * meleeColliderDistance;
        Vector2 size = new Vector2(boxCollider.bounds.size.x * meleeRange, boxCollider.bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.zero, 0, playerLayer);
        return hit.collider != null;
    }

    private int FindProjectile()
    {
        for (int i = 0; i < projectiles.Length; i++)
        {
            if (!projectiles[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 offset = transform.right * Mathf.Sign(transform.localScale.x);
        Gizmos.DrawWireCube(boxCollider.bounds.center + offset * rangedRange * rangedColliderDistance,
            new Vector3(boxCollider.bounds.size.x * rangedRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCollider.bounds.center + offset * meleeRange * meleeColliderDistance,
            new Vector3(boxCollider.bounds.size.x * meleeRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }

    public bool IsRangedAttackReady()
    {
        return rangedTimer >= rangedCooldown;
    }
}