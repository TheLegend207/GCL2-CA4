using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;

    // Most of these are pretty self-explainotory
    public float walkSpeed = 15f;
    public float shiftWalkSpeed = 3f;
    public float jumpPower = 7f;
    public float gravity = 15f;
    public float currentSpeed;
    public int speedBoost = 0;

    public float lookSpeed = 2f;
    public float lookXLimit = 60f; // Prevent player from rotating camera too far up/down
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1.5f;
    public int maxHealth = 100;
    public int currentHealth;

    public TMP_Text healthText; // To change health UI
    public UnityEngine.UI.Image healthBar;

    private Vector3 moveDirection = Vector3.zero; // Store player's movements
    private float rotationX = 0f; // Store how far the player has look up/down

    private CharacterController characterController; // Use character controller for height adjustments

    private bool canMove = true; // Ref to allow player to move
    private bool isDead = false;


    private void Start()
    {
        speedBoost = 0;
        characterController = GetComponent<CharacterController>(); // Gets a character controller attached to player

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor in the middle of screen
        Cursor.visible = false; // Hides it

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
        // When changing directions, key inputs will also change to that direction
        float inputVertical = Input.GetAxis("Vertical");
        float inputHorizontal = Input.GetAxis("Horizontal");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 movement = (forward * inputVertical) + (right * inputHorizontal); // Allow diagonal movement

        // Prevent diagonal movement from being faster
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        // To determine player movement speed
        if (!canMove)
        {
            currentSpeed = 0f;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            // Crouching
            currentSpeed = crouchSpeed + speedBoost;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            // Shift-walk
            currentSpeed = shiftWalkSpeed + speedBoost;
        }
        else
        {
            // Normal 
            currentSpeed = walkSpeed + speedBoost;
        }

        // Movement direction and speed of player
        moveDirection.x = movement.x * currentSpeed;
        moveDirection.z = movement.z * currentSpeed;

        // Jumping

        if (Input.GetButton("Jump") &&
            canMove &&
            characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }

        // Gravity

        if (characterController.isGrounded)
        {
            if (moveDirection.y < 0f)
            {
                moveDirection.y = -2f;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Crouching

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

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!canMove)
            return;

        // Vertical camera movement
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;

        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // Verticle camera rotation
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Horizontal player rotation
        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
    }

    // Health
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        canMove = false;
        FindFirstObjectByType<LevelManager>().PlayerDied();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "+" + currentHealth;
        }

        float healthPercentage = (float)currentHealth / maxHealth;
        Color healthColor;

        if (healthPercentage > 0.6f)
        {
            healthColor = Color.green;
        }
        else if (healthPercentage > 0.3f)
        {
            healthColor = Color.yellow;
        }
        else
        {
            healthColor = Color.red;
        }

        if (healthText != null)
        {
            healthText.color = healthColor;
        }

        if (healthBar != null)
        {
            healthBar.fillAmount = healthPercentage;
            healthBar.color = healthColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("SpeedBoost"))
        {

            StartCoroutine(SpeedBoost());
            Destroy(other.gameObject);
        }
    }

    IEnumerator SpeedBoost()
    {
        speedBoost = 7;
        print("boost start");
        yield return new WaitForSeconds(7f);
        speedBoost = 0;
        print("boost end");
    }
}