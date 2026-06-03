using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKey(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private bool ShouldSlide => Input.GetKeyDown(slideKey) && IsSprinting && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canSlide = true;
    [SerializeField] private bool canUseHeaadbob = true;


    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;


    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("HeadBob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobspeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobspeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Slide Parameters")]
    [SerializeField] private float slideSpeed = 10.0f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float slideDeceleration = 0.2f;
    private float currentSlideSpeed;
    private float slideTimer;
    private bool isSliding;
    private Vector3 slideDirection;
    private bool forceCrouchForSlide;
    private bool slideCrouched;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0f;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
                HandleJump();

            if (canCrouch)
                HandleCrouch();

            if (isCrouching && !Input.GetKey(crouchKey) && !duringCrouchAnimation)
            {
                if(!Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
                {
                    StartCoroutine(CrouchStand());
                }
            }
            if (canSlide)
                HandleSlide();

            if (canUseHeaadbob)
                HandleHeadbob();

            ApplyFinalMovements();
        }
    }
    private void HandleMovementInput()
    {
        if (isSliding)
            return;
        
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }


    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump()
    {
        if (ShouldJump)
            moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {       
        if (forceCrouchForSlide)
            return;
        
        if (ShouldCrouch && !isCrouching)
            StartCoroutine(CrouchStand());

        if (Input.GetKeyUp(crouchKey) && isCrouching && !duringCrouchAnimation)
            StartCoroutine(CrouchStand());
    }
    private void ResetSlideStates()
    {
        isSliding = false;
        forceCrouchForSlide = false;
        slideCrouched = false;
        currentSlideSpeed = 0f;
        slideDirection = Vector3.zero;
    }
    private void HandleSlide()
    {
        if (ShouldSlide && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            currentSlideSpeed = slideSpeed;
            slideDirection = transform.forward;

            forceCrouchForSlide = true;

            if (!isCrouching)
            {
                slideCrouched = true;
                StartCoroutine(CrouchStand());
            }
            else
            {
                slideCrouched = true;
            }

            return;
        }

        if (!isSliding)
            return;

        slideTimer -= Time.deltaTime;

        float moveDirectionY = moveDirection.y;

        moveDirection = slideDirection * currentSlideSpeed;
        moveDirection.y = moveDirectionY;

        currentSlideSpeed -= slideDeceleration * Time.deltaTime;

        if (currentSlideSpeed < 0f)
            currentSlideSpeed = 0f;

        if (slideTimer <= 0f || currentSlideSpeed <= 0f)
        {
            isSliding = false;
            forceCrouchForSlide = false;
            slideCrouched = false;

            currentSlideSpeed = 0f;
            slideDirection = Vector3.zero;

            moveDirection.x = 0f;
            moveDirection.z = 0f;

            if (!Input.GetKey(crouchKey))
            {
                if (!Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
                {
                    StartCoroutine(CrouchStand());
                }
            }
        }
    }
    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f) 
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobspeed : IsSprinting ? sprintBobspeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }
    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded) 
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
    private IEnumerator CrouchStand()
    {
        if (forceCrouchForSlide && !isCrouching)
            yield break;
        
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;
        
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter  = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        if (slideCrouched && isCrouching == false)
            slideCrouched = false;

        isCrouching = !isCrouching;
        
        duringCrouchAnimation = false;
    }
    
}
