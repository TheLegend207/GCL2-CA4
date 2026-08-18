using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;

    // Self-explainory
    public float walkSpeed = 15f;
    public float shiftWalkSpeed = 3f;
    public float jumpPower = 7f;
    public float gravity = 15f;
    public float currentSpeed;
    public int speedBoost = 0;
    public float lookSpeed = 2f;
    public float lookXLimit = 60f; // How far player can rotate up/down
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1.5f;

    // Health related stuff
    public int maxHealth = 100;
    public int currentHealth;
    public TMP_Text healthText;
    public Image healthBar;

    [Header("Hurt sound")]
    public AudioClip HurtSound;

    [Range(0f, 1f)]
    public float HurtVolume = 1f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    private CharacterController characterController;
    private AudioSource audioSource;

    private bool canMove = true; 
    private bool isDead = false;


    private void Start()
    {
        currentHealth = maxHealth;
        speedBoost = 0;
        characterController = GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateHealthUI();
    }

    private void Update()
    {
        Movement();
        Look();
    }

    // Player movment 
    private void Movement()
    {
        // Gets player's "wasd" input 
        float inputVertical = Input.GetAxis("Vertical");
        float inputHorizontal = Input.GetAxis("Horizontal");

        // Gets player direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Allow player to combine verticle and horizontal input
        Vector3 movement =(forward * inputVertical) + (right * inputHorizontal);
        
        // Prevents diagonal movement from being faster than normal movement
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }
        
        // Prevent player from moving
        if (!canMove)
        {
            currentSpeed = 0f;
        }

        // Crouch
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = crouchSpeed + speedBoost;
        }

        // Shift-walk
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = shiftWalkSpeed + speedBoost;
        }
        else
        {
            currentSpeed = walkSpeed + speedBoost;
        }

        // Apply horizontal movement on z and x axis
        moveDirection.x = movement.x * currentSpeed;
        moveDirection.z = movement.z * currentSpeed;

        // Jump
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }

        if (characterController.isGrounded)
        {
            // Prevent player from continously falling 
            if (moveDirection.y < 0f)
            {
                moveDirection.y = -2f;
            }
        }
        else
        {
            // Apply gravity over time
            moveDirection.y -= gravity * Time.deltaTime;
        }
        
        // To make camera go down as well when crouching
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = Mathf.MoveTowards(characterController.height, crouchHeight, crouchSpeed * Time.deltaTime);
        }
        else
        {
            characterController.height = Mathf.MoveTowards(characterController.height, defaultHeight, 6f * Time.deltaTime);
        }
        characterController.Move(moveDirection * Time.deltaTime);
    }

    //
    private void Look()
    {
        // Stops function when player cannot move
        if (!canMove)
        {
            return;
        }

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; // Change verticle movement based on mouse speed
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); // Prevenets player from looking too far up/down
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f); // Rotate camera up/down
        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f); // Rotate player left/right
    }

    public void TakeDamage(int damage)
    {
        // Nothing happens if player is alr dead
        if (isDead)
        {
            return;
        }

        currentHealth -= damage; // Subtract damage from player's current health
        
        // Prevent health from going beyond 0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        PlayHurtSound();
        UpdateHealthUI();
        
        // Kill player when health becomes 0
        if (currentHealth <= 0)
        {
            Die(); 
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI();
    }

    private void PlayHurtSound()
    {
        if (HurtSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            HurtSound,
            HurtVolume
        );
    }

    private void Die()
    {
        // Prevent player from dying multiple times by taking damage frequently after death
        if (isDead)
        {
            return;
        }

        isDead = true;
        canMove = false;

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.PlayerDied(); // Tell level manager that player died
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "+" + currentHealth; // Display current health with a + sign
        }
        
        // Convert health into percentage 
        // Change color for health bar and number 
        float healthPercentage = (float)currentHealth / maxHealth;

        // Store color that health UI should use
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
            healthText.color = healthColor; // Change number color
        }

        if (healthBar != null)
        {
            healthBar.fillAmount = healthPercentage; // Change how full health bar is
            healthBar.color = healthColor; // Change bar color
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

    private IEnumerator SpeedBoost() //speed boost given to player when picking up speed boots
    {
        speedBoost = 7;

        Debug.Log("Speed boost started.");

        yield return new WaitForSeconds(7f);

        speedBoost = 0;

        Debug.Log("Speed boost ended.");
    }



}