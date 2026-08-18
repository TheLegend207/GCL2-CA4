using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Medkit : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode pickupKey = KeyCode.E;
    public float holdTime = 3f;

    [Header("Healing")]
    public int healAmount = 50;

    [Header("Progress UI")]
    public Image progressCircle;

    [Header("Healing sounds")]
    public AudioClip healingHoldSound;

    [Range(0f, 1f)]
    public float healingHoldVolume = 1f;

    public AudioClip healedSound;

    [Range(0f, 1f)]
    public float healedVolume = 1f;

    private PlayerController playerInRange;
    private float holdTimer = 0f;
    private AudioSource audioSource;
    private bool healingSoundPlaying;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        transform.Rotate(
            0f,
            45f * Time.deltaTime,
            0f
        );

        if (playerInRange == null)
        {
            ResetHealing();
            return;
        }

        if (playerInRange.currentHealth >=
            playerInRange.maxHealth)
        {
            ResetHealing();
            return;
        }

        if (Input.GetKey(pickupKey))
        {
            holdTimer += Time.deltaTime;

            StartHealingSound();

            if (progressCircle != null)
            {
                progressCircle.gameObject.SetActive(true);

                progressCircle.fillAmount =
                    Mathf.Clamp01(
                        holdTimer / holdTime
                    );
            }

            if (holdTimer >= holdTime)
            {
                UseMedkit();
            }
        }
        else
        {
            ResetHealing();
        }
    }

    private void UseMedkit()
    {
        if (playerInRange == null)
        {
            return;
        }

        if (playerInRange.currentHealth >=
            playerInRange.maxHealth)
        {
            ResetHealing();
            return;
        }

        StopHealingSound();

        playerInRange.Heal(healAmount);

        PlayHealedSound();

        Debug.Log(
            "Medkit used. Player healed for " +
            healAmount +
            " HP."
        );

        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }

    private void StartHealingSound()
    {
        if (healingSoundPlaying)
        {
            return;
        }

        if (healingHoldSound == null)
        {
            return;
        }

        audioSource.clip = healingHoldSound;
        audioSource.loop = true;
        audioSource.volume = healingHoldVolume;
        audioSource.Play();

        healingSoundPlaying = true;
    }

    private void StopHealingSound()
    {
        if (!healingSoundPlaying)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
        healingSoundPlaying = false;
    }

    private void PlayHealedSound()
    {
        if (healedSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            healedSound,
            healedVolume
        );
    }

    private void ResetHealing()
    {
        holdTimer = 0f;

        StopHealingSound();

        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            playerInRange = player;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player != null &&
            player == playerInRange)
        {
            playerInRange = null;
            ResetHealing();
        }
    }
}