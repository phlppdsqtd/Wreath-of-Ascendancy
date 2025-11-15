using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    [Header("Base Movement")]
    public float runAcceleration = 0.25f;
    public float runSpeed = 4f;
    public float drag = 0.1f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    [Header("Animation & Audio")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource runAudioSource;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private float runThreshold = 0.1f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private Vector2 _cameraRotation = Vector2.zero;

    // gravity
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;

    private bool isRunningState = false;

    // ----------------------------
    // ⭐ Grounded Buffer System
    // ----------------------------
    private bool bufferedGrounded = false;
    private float groundedBufferTimer = 0f;
    private float groundedBufferDuration = 0.18f; // tweak between 0.15–0.25 for stairs
    // ----------------------------

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (runAudioSource == null)
            runAudioSource = GetComponent<AudioSource>();

        if (runAudioSource != null)
        {
            runAudioSource.loop = true;
            runAudioSource.playOnAwake = false;

            if (runClip != null)
                runAudioSource.clip = runClip;
        }
    }

    private void Start()
    {
        float startY = transform.eulerAngles.y;

        // Initialize camera and rotation tracking to match inspector-facing direction
        _cameraRotation.x = startY;

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
    }
    
    private void Update()
    {
        // ----------------------------
        // ⭐ Grounded Buffer Update
        // ----------------------------
        if (_characterController.isGrounded)
        {
            groundedBufferTimer = groundedBufferDuration;
            bufferedGrounded = true;
        }
        else
        {
            groundedBufferTimer -= Time.deltaTime;

            if (groundedBufferTimer <= 0)
                bufferedGrounded = false;
        }
        // ----------------------------

        // --- Calculate movement direction based on camera ---
        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;

        Vector3 movementDirection =
            cameraRightXZ * _playerLocomotionInput.MovementInput.x +
            cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        // --- Apply acceleration ---
        Vector3 movementDelta = movementDirection * runAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // --- Apply drag ---
        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;

        // --- Speed limit ---
        newVelocity = Vector3.ClampMagnitude(newVelocity, runSpeed);

        // --- Gravity ---
        if (bufferedGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;

        // --- Final movement ---
        Vector3 finalMove = (newVelocity + verticalVelocity) * Time.deltaTime;
        _characterController.Move(finalMove);

        // --- Rotate player toward movement direction ---
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
        }

        // ----------------------------------------------------
        // ⭐ Running detection (uses bufferedGrounded instead)
        // ----------------------------------------------------
        float inputMagnitude = _playerLocomotionInput.MovementInput.magnitude;
        bool shouldRun = inputMagnitude > runThreshold && bufferedGrounded;

        // --- Animator update ---
        if (animator != null)
        {
            if (isRunningState != shouldRun)
            {
                isRunningState = shouldRun;
                animator.SetBool("isRunning", isRunningState);
            }
        }

        // --- Running audio ---
        if (runAudioSource != null)
        {
            if (shouldRun)
            {
                if (!runAudioSource.isPlaying && runAudioSource.clip != null)
                    runAudioSource.Play();
            }
            else
            {
                if (runAudioSource.isPlaying)
                    runAudioSource.Stop();
            }
        }
    }

    private void LateUpdate()
    {
        // Camera rotation only
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
    }

    public void ForceStopRunAudio()
    {
        if (runAudioSource != null && runAudioSource.isPlaying)
            runAudioSource.Stop();

        if (animator != null)
            animator.SetBool("isRunning", false);

        isRunningState = false;
    }
}
