using UnityEngine;
using System.Collections;

public class PlayerMovement2D : MonoBehaviour
{
    [Header ("Movement Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;

    [Header ("Multiple Jumps")]
    [SerializeField] private int extraJumps;
    private int jumpCounter;

    [Header("Jump Cooldown")]
    [SerializeField] private float jumpCooldown;
    private float lastJumpTime;

    [Header ("Wall Jumping")]
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;

    [Header ("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header ("SFX")]
    [SerializeField] private AudioClip jumpSound;

    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpCooldown;
    private float horizontalInput;
    private UIManager uiManager;
    private bool doubleJumpEnabled = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        uiManager = FindFirstObjectByType<UIManager>();
        extraJumps = 0;
    }

    private void Start()
    {
        if (PlayerSkillManager.instance != null && PlayerSkillManager.instance.HasSkill("Double Jump"))
        {
            EnableDoubleJump();
            doubleJumpEnabled = true;
        }
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (uiManager != null && (uiManager.IsTalking() || uiManager.IsControlsUIActive()))
        {
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
            anim.SetBool("run", false);
            anim.ResetTrigger("jump");
            return; 
        }

        wallJumpCooldown += Time.deltaTime;

        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z); //change 1 to eg 0.3f if scaled to 0.3
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z); //change 1 to eg 0.3f if scaled to 0.3
        }

        anim.SetBool("run", horizontalInput !=0);
        anim.SetBool("grounded", isGrounded());

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale != 0)
                Jump();
        }
        
        if (Input.GetKeyUp(KeyCode.Space) && body.linearVelocity.y > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y / 2);
        }

        if (onWall())
        {
            body.gravityScale = 0;
            body.linearVelocity = Vector2.zero;
        }
        else
        {
            body.gravityScale = 7;
            body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y);

            if (isGrounded())
            {
                jumpCounter = extraJumps;
            }
        }
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime < jumpCooldown)
            return;

        if (isGrounded())
        {
            PerformJump();
            jumpCounter = extraJumps;
            lastJumpTime = Time.time;
            return;
        }

        if (doubleJumpEnabled && jumpCounter > 0)
        {
            PerformJump();
            jumpCounter--;
            lastJumpTime = Time.time;
            return;
        }
    }

    private void PerformJump()
    {
        anim.SetTrigger("jump");
        SoundManager.instance.PlaySound(jumpSound);
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpPower);
    }
 
    private bool isGrounded() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool onWall() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack() {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }

    #region SKILL
    public void EnableDoubleJump()
    {
        extraJumps = 1;
    }

    private void OnEnable()
    {
        PlayerSkillManager.OnSkillUnlocked += HandleSkillUnlocked;
    }

    private void OnDisable()
    {
        PlayerSkillManager.OnSkillUnlocked -= HandleSkillUnlocked;
    }

    private void HandleSkillUnlocked(string skillName)
    {
        if (skillName == "Double Jump" && !doubleJumpEnabled)
        {
            EnableDoubleJump();
            doubleJumpEnabled = true;
        }
    }
    #endregion
}