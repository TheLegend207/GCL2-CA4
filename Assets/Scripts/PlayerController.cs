using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player References")]
    public Camera playerCamera;

    [Header("Movement")]
    public float walkSpeed = 15f;
    public float shiftWalkSpeed = 3f;
    public float jumpPower = 7f;
    public float gravity = 15f;

    [Header("Look")]
    public float lookSpeed = 2f;
    public float lookXLimit = 60f;

    [Header("Crouching")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1.5f;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public TMP_Text healthText;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    private CharacterController characterController;

    private bool canMove = true;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxHealth;

        UpdateHealthUI();
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        // --------------------------------------------------
        // GET MOVEMENT INPUT
        // --------------------------------------------------

        float inputVertical = Input.GetAxis("Vertical");
        float inputHorizontal = Input.GetAxis("Horizontal");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 movement = (forward * inputVertical) + (right * inputHorizontal);

        // Prevent diagonal movement from being faster.
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        // --------------------------------------------------
        // DETERMINE CURRENT MOVEMENT SPEED
        // --------------------------------------------------

        float currentSpeed;

        if (!canMove)
        {
            currentSpeed = 0f;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            // Crouching
            currentSpeed = crouchSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            // Slow walking
            currentSpeed = shiftWalkSpeed;
        }
        else
        {
            // Normal walking
            currentSpeed = walkSpeed;
        }

        // Apply horizontal movement.
        moveDirection.x = movement.x * currentSpeed;
        moveDirection.z = movement.z * currentSpeed;

        // --------------------------------------------------
        // JUMPING
        // --------------------------------------------------

        if (Input.GetButton("Jump") &&
            canMove &&
            characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }

        // --------------------------------------------------
        // GRAVITY
        // --------------------------------------------------

        if (characterController.isGrounded)
        {
            // Keep the player grounded instead of accumulating
            // negative velocity.
            if (moveDirection.y < 0f)
            {
                moveDirection.y = -2f;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // --------------------------------------------------
        // CROUCHING
        // --------------------------------------------------

        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = Mathf.MoveTowards(
                characterController.height,
                crouchHeight,
                crouchSpeed * Time.deltaTime
            );
        }
        else
        {
            characterController.height = Mathf.MoveTowards(
                characterController.height,
                defaultHeight,
                6f * Time.deltaTime
            );
        }

        // --------------------------------------------------
        // MOVE PLAYER
        // --------------------------------------------------

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!canMove)
            return;

        // Vertical camera movement.
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;

        // Clamp vertical looking.
        rotationX = Mathf.Clamp(
            rotationX,
            -lookXLimit,
            lookXLimit
        );

        // Apply vertical camera rotation.
        playerCamera.transform.localRotation =
            Quaternion.Euler(rotationX, 0f, 0f);

        // Horizontal player rotation.
        transform.rotation *= Quaternion.Euler(
            0f,
            Input.GetAxis("Mouse X") * lookSpeed,
            0f
        );
    }

    // --------------------------------------------------
    // HEALTH
    // --------------------------------------------------

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log($"{gameObject.name} HP: {currentHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");

        canMove = false;

        // Add your death behaviour here later.
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "+" + currentHealth;
        }
    }
}