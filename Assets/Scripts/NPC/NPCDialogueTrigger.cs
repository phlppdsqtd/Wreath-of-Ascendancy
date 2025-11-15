using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] dialogueLines;

    private bool playerInRange = false;
    private UIManager uiManager;
    private Animator animator;
    private bool isTalking = false;

    private void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();

        if (animator != null)
            animator.SetBool("idle", true);
    }

    private void Update()
    {
        // Player presses E to start dialogue
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !uiManager.IsTalking())
        {
            uiManager.StartDialogue(dialogueLines);
            TriggerActivateAnimation();
            isTalking = true;
        }

        // Dialogue finished
        if (isTalking && !uiManager.IsTalking() && !uiManager.IsControlsUIActive())
        {
            ResetToIdleAnimation();
            isTalking = false;
        }
    }

    // -----------------------
    //  ⭐ 3D TRIGGER CALLBACKS
    // -----------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    // -----------------------
    //  Optional animations
    // -----------------------
    private void TriggerActivateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("idle", false);
            animator.SetTrigger("activate");
        }
    }

    private void ResetToIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("idle", true);
            animator.ResetTrigger("activate");
        }
    }
}
