using UnityEngine;

public class BossChaseController : MonoBehaviour
{
    [SerializeField] private Transform bossBody;
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask playerLayer;

    [Header("Combat References")]
    [SerializeField] private BossEnemy bossEnemy;
    [SerializeField] private float stopDistance;
    [SerializeField] private float meleeRange;

    private Transform player;
    private Animator anim;
    private float scaleX;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        anim = bossBody.GetComponent<Animator>();
        scaleX = Mathf.Abs(bossBody.localScale.x);
    }

    private void Update()
    {
        if (!CanMove || player == null || bossEnemy == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool isRangedReady = bossEnemy.IsRangedAttackReady();

        float targetStopDistance = isRangedReady ? stopDistance : meleeRange;

        if (distance > targetStopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = new Vector2(transform.position.x + direction.x * moveSpeed * Time.deltaTime, transform.position.y);

            if (direction.x != 0)
                bossBody.localScale = new Vector3(-scaleX * Mathf.Sign(direction.x), bossBody.localScale.y, bossBody.localScale.z);
            anim.SetBool("moving", true);
        }
        else
        {
            anim.SetBool("moving", false);
        }
    }

    public void SetPlayer(Transform target)
    {
        player = target;
    }
}
