using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Camera Components")]
    public float crouchCameraHeight = 0.5f; 
    public float cameraTransitionSpeed = 8f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float proneHeight = 0.5f;
    public Camera playerCamera;

    [Header("Speeds")]
    public float proneSpeed = 1.5f;
    public float crouchSpeed = 3f;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;

    [Header ("GravityContents")]
    public float jumpPower = 7f;
    public float extraJumps = 1;
    public float gravity = 10f;
    public float airSteering = 3f;

    [Header("Sliding")]
    public float slideSpeed = 16f;
    public float slideFriction = 8f;
    public float minimumSlideSpeed = 2f;
    public float slideSteering = 5f;

    [Header("Slope Movement")]
    public float slopeAcceleration = 12f;
    public float maxSlopeSpeed = 24f;

    private Vector3 slopeMomentum;
    private float defaultCameraHeight;
    private bool isSliding = false;
    private float currentSlideSpeed;
    private Vector3 airMomentum;
    private Vector3 slideDirection;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;
    private bool wasGrounded;

    void Start()
    {
        defaultCameraHeight = playerCamera.transform.localPosition.y;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        Vector3 foward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.C);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        float currentSpeed = isCrouching ? crouchSpeed : isRunning ? runSpeed : walkSpeed;
        float movementDirectionY = moveDirection.y;

        Vector3 inputDirection = (foward * vertical) + (right * horizontal);
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        // if the key C is pressed, the player is 
        if (Input.GetKeyDown(KeyCode.C) && isRunning && !isSliding && canMove)
        {
            isSliding = true;
            currentSlideSpeed = slideSpeed;
            slideDirection = inputDirection.magnitude > 0.1f ? inputDirection.normalized : foward;
        }

        // if sliding and not crouching
        if (isSliding && !isCrouching)
        {
            isSliding = false;
        }
        Vector3 horizontalMovement = Vector3.zero;

        // if the boolean that determines if the player is sliding is true, run this function
        if (isSliding)
        {
            currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, 0f, slideFriction * Time.deltaTime);

            Vector3 targetDirection = transform.forward;

            slideDirection = Vector3.Slerp(slideDirection, targetDirection, slideSteering * Time.deltaTime).normalized;
            horizontalMovement = slideDirection * currentSlideSpeed;

            if (currentSlideSpeed <= minimumSlideSpeed)
            {
                currentSlideSpeed = minimumSlideSpeed;
            }
        }
        else
        {
            horizontalMovement = canMove ? inputDirection * currentSpeed : Vector3.zero;
        }

        if (characterController.isGrounded)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 + 0.5f))
            {
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

                if (slopeDirection.y < -0.01f)
                {
                    slopeMomentum += slopeDirection * slopeAcceleration * Time.deltaTime;
                }
                else
                {
                    slopeMomentum = Vector3.MoveTowards(slopeMomentum, Vector3.zero, slopeAcceleration * Time.deltaTime);
                }
            }
            else
            {
                slopeMomentum = Vector3.MoveTowards(slopeMomentum, Vector3.zero, slopeAcceleration * Time.deltaTime);
            }
        }
        else
        {
            slopeMomentum = Vector3.MoveTowards(slopeMomentum, Vector3.zero, slopeAcceleration * Time.deltaTime);
        }
        horizontalMovement += new Vector3(slopeMomentum.x, 0, slopeMomentum.z);

        horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, maxSlopeSpeed);

        // if the player is on the floor, and the jump button is pressed, run this function
        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;

        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        
        if(Input.GetButtonDown("Jump") && canMove && !characterController.isGrounded && extraJumps > 0)
        {
            moveDirection.y = jumpPower;
            extraJumps--;
        }

        if (isSliding || (isCrouching && canMove))
        {
            characterController.height = crouchHeight;
        }
        else
        {
            characterController.height = defaultHeight;
        }

        float targetCameraHeight = (isSliding || isCrouching) ? crouchCameraHeight : defaultCameraHeight;
        Vector3 cameraPosition = playerCamera.transform.localPosition;

        cameraPosition.y = targetCameraHeight;

        playerCamera.transform.localPosition = cameraPosition;
        
        
        if (characterController.isGrounded)
        {
            moveDirection.x = horizontalMovement.x;
            moveDirection.z = horizontalMovement.z;

            airMomentum = horizontalMovement;
        }
        else
        {
            Vector3 targetDirection = transform.forward;

            airMomentum = Vector3.Slerp(airMomentum.normalized, targetDirection, airSteering * Time.deltaTime) * airMomentum.magnitude;

            moveDirection.x = airMomentum.x;
            moveDirection.z = airMomentum.z;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (characterController.isGrounded && !wasGrounded)
        {
            extraJumps = 1;
        }

        wasGrounded = characterController.isGrounded;
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
