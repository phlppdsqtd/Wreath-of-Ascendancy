using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;

    [Header ("Basic Attack")]
    [SerializeField] private GameObject[] lightballs;
    [SerializeField] private AudioClip lightballsSound;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float manaCost;

    [Header ("Skill")]
    [SerializeField] private GameObject[] fireballSkill;
    [SerializeField] private AudioClip fireballSkillSound;
    [SerializeField] private float fireballSkillCooldown;
    [SerializeField] private float fireballSkillManaCost;

    private PlayerSkillManager playerSkillManager;
    private Mana mana;
    private UIManager uiManager;
    private Animator anim;
    private PlayerMovement2D playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        mana = GetComponent<Mana>();
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement2D>();
        uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Start()
    {
        StartCoroutine(WaitForPlayerSkillManager());
    }


    private IEnumerator WaitForPlayerSkillManager()
    {
        while (PlayerSkillManager.instance == null)
            yield return null;

        playerSkillManager = PlayerSkillManager.instance;
    }

    private void Update()
    {
        if (uiManager != null && (uiManager.IsTalking() || uiManager.IsControlsUIActive()))
            return;

        if(Input.GetMouseButton(0) && cooldownTimer > attackCooldown && playerMovement.canAttack())
        {
            if(Time.timeScale != 0 && mana.UseMana(manaCost))
            {    
                Attack();
            }
            else
            {
                uiManager.FlashManaBar();
            }
        }

        if (Input.GetMouseButtonDown(1) && playerSkillManager != null && cooldownTimer > fireballSkillCooldown && playerMovement.canAttack())
        {
            if (Time.timeScale != 0 && playerSkillManager.HasSkill("Fireball"))
            {
                if (mana.GetCurrentMana() >= fireballSkillManaCost)
                {
                    mana.UseMana(fireballSkillManaCost);
                    FireballSkill();
                }
                else
                {
                    uiManager.FlashManaBar();
                }
            }
        }
        cooldownTimer += Time.deltaTime;
    }

    private void Attack() {
        SoundManager.instance.PlaySound(lightballsSound);
        
        anim.SetTrigger("attack");
        cooldownTimer = 0;

        lightballs[FindLightball()].transform.position = firePoint.position;
        lightballs[FindLightball()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
    }

    private void FireballSkill()
    {
        SoundManager.instance.PlaySound(fireballSkillSound);

        anim.SetTrigger("fireball");
        cooldownTimer = 0;

        int index = FindFireball();
        fireballSkill[index].transform.position = firePoint.position;
        fireballSkill[FindFireball()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x), true);
    }

    private int FindLightball() {
        for (int i = 0; i < lightballs.Length; i++) {
            if(!lightballs[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballSkill.Length; i++)
        {
            if (!fireballSkill[i].activeInHierarchy)
                return i;
        }
        return 0;
    }
}
