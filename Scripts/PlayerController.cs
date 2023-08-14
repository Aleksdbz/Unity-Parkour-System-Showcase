using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;
    float jumpForwardSpeed = 2f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadious = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    bool hasControl = true;
    float ySpeed;

    // Indicates whether the player is performing an action or not
    public bool InAction { get; private set; }

    Vector3 desiredMoveDir;
    Vector3 MoveDir;
    Vector3 velocity;

    CameraController camerController;
    Animator animator;
    CharacterController characterController;
    EnviromentScanner enviromentScanner;

    // Ledge related properties
    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }
    public bool isHang { get; set; }

    Quaternion targetRotation;

    private void Awake()
    {
        camerController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        enviromentScanner = GetComponent<EnviromentScanner>();
    }

    void Update()
    {
        if (!hasControl) return; // If the player doesn't have control, skip the update.
        OnMove();
        GroundCheck();
    }

    void OnMove()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        var moveInput = (new Vector3(horizontalInput, 0, verticalInput)).normalized;

        // Get the desired movement direction based on the camera's rotation
        desiredMoveDir = camerController.PlaneRotation * moveInput;
        MoveDir = desiredMoveDir;

        if (isHang) return;

        velocity = Vector3.zero;

        if (isGrounded)
        {
            ySpeed = -0.5f;
            velocity = desiredMoveDir * moveSpeed;
            animator.SetBool("isGrounded", isGrounded);


            // Check for obstacles in the movement direction
            IsOnLedge = enviromentScanner.ObstacleLedgeCheck(desiredMoveDir, out LedgeData ledgeData);
            if (IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }

            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            // Handle vertical movement due to gravity when airborne
            ySpeed += Physics.gravity.y * Time.deltaTime;

            velocity = transform.forward * moveSpeed / jumpForwardSpeed;
        }

        velocity.y = ySpeed;

        // Apply the calculated movement
        characterController.Move(velocity * Time.deltaTime);

        // Rotate the player towards the desired movement direction
        if (moveAmount > 0 && MoveDir.magnitude > 0.2f)
        {
            targetRotation = Quaternion.LookRotation(MoveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Check if the player is grounded
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadious, groundLayer);
    }

    // Adjust the movement while on a ledge
    void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if (Vector3.Angle(desiredMoveDir, transform.forward) >= 80)
        {
            velocity = Vector3.zero;
            return;
        }

        if (angle < 60)
        {
            velocity = Vector3.zero;
            desiredMoveDir = Vector3.zero;
        }
        else if (angle < 90)
        {
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            MoveDir = dir;
        }
    }

    // Enable or disable player control
    public void SetControl(bool hascontrol)
    {
        this.hasControl = hascontrol;
        characterController.enabled = hascontrol;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0);
            targetRotation = transform.rotation;
        }
    }
    public void EnableCharacterController(bool enabled)
    {
        characterController.enabled = enabled;  
    }

    public void ResetTargetRotation()
    {
        targetRotation = transform.rotation;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadious);
    }

    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
    }

    // Co-routine to handle parkour actions
    public IEnumerator DoAction(string animName, MatchTargetParams matchParams = null , Quaternion targetRotation = new Quaternion(), 
        bool rotate = false, float poastDelay = 0f, bool mirror = false)
    {
        InAction = true; // Player is now in action
        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(animName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(animName))
            Debug.LogError("The parkour animation is wrong!");

        float rotateStartTime = (matchParams != null)?matchParams.startTime : 0f;   

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (rotate && normalizedTime > rotateStartTime)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            if (matchParams != null)
            {
                MatchTarget(matchParams);
            }
            yield return null;
        }

        yield return new WaitForSeconds(poastDelay);
        InAction = false;
    }

    // Match the player's  position during an animation
    void MatchTarget(MatchTargetParams mp)
    {
        if (animator.isMatchingTarget) return;

        if (!animator.IsInTransition(0))
        {
            animator.MatchTarget(mp.pos, transform.rotation, mp.bodyPart,
                               new MatchTargetWeightMask(mp.posWeight, 0), mp.startTime, mp.targetTime);
        }
    }

    public float RotationSpeed => rotationSpeed;
}

public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodyPart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}
