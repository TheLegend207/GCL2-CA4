using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;

    // Player movement and stuff
    public float walkSpeed = 15f;
    public float shiftWalkSpeed = 3f;
    public float jumpPower = 7f;
    public float gravity = 15f;
    public float currentSpeed;
    public int speedBoost = 0;
    public float lookSpeed = 2f;
    public float lookXLimit = 60f;
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
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        float inputVertical =
            Input.GetAxis("Vertical");

        float inputHorizontal =
            Input.GetAxis("Horizontal");

        Vector3 forward =
            transform.TransformDirection(
                Vector3.forward
            );

        Vector3 right =
            transform.TransformDirection(
                Vector3.right
            );

        Vector3 movement =
            (forward * inputVertical) +
            (right * inputHorizontal);

        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        if (!canMove)
        {
            currentSpeed = 0f;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed =
                crouchSpeed + speedBoost;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed =
                shiftWalkSpeed + speedBoost;
        }
        else
        {
            currentSpeed =
                walkSpeed + speedBoost;
        }

        moveDirection.x =
            movement.x * currentSpeed;

        moveDirection.z =
            movement.z * currentSpeed;

        if (Input.GetButton("Jump") &&
            canMove &&
            characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }

        if (characterController.isGrounded)
        {
            if (moveDirection.y < 0f)
            {
                moveDirection.y = -2f;
            }
        }
        else
        {
            moveDirection.y -=
                gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) &&
            canMove)
        {
            characterController.height =
                Mathf.MoveTowards(
                    characterController.height,
                    crouchHeight,
                    crouchSpeed *
                    Time.deltaTime
                );
        }
        else
        {
            characterController.height =
                Mathf.MoveTowards(
                    characterController.height,
                    defaultHeight,
                    6f * Time.deltaTime
                );
        }

        characterController.Move(
            moveDirection * Time.deltaTime
        );
    }

    private void HandleLook()
    {
        if (!canMove)
        {
            return;
        }

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0f, Input.GetAxis("Mouse X") * lookSpeed, 0f);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        PlayHurtSound();
        UpdateHealthUI();

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
        if (isDead)
        {
            return;
        }

        isDead = true;
        canMove = false;

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.PlayerDied();
        }
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
            healthBar.fillAmount =
                healthPercentage;

            healthBar.color =
                healthColor;
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

    private IEnumerator SpeedBoost()
    {
        speedBoost = 7;

        Debug.Log("Speed boost started.");

        yield return new WaitForSeconds(7f);

        speedBoost = 0;

        Debug.Log("Speed boost ended.");
    }



}