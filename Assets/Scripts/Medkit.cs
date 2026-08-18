using UnityEngine;
using UnityEngine.UI;

public class Medkit : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode pickupKey = KeyCode.E;
    public float holdTime = 3f;

    [Header("Healing")]
    public int healAmount = 50;

    [Header("Progress UI")]
    public Image progressCircle;

    private PlayerController playerInRange;
    private float holdTimer = 0f;

    private void Start()
    {
        // Hide the progress circle when the game starts.
        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // No player is inside the medkit's interaction area.
        if (playerInRange == null)
        {
            ResetHealing();
            return;
        }

        // Player is holding E.
        if (Input.GetKey(pickupKey))
        {
            holdTimer += Time.deltaTime;

            // Show the progress circle.
            if (progressCircle != null)
            {
                progressCircle.gameObject.SetActive(true);

                progressCircle.fillAmount =
                    Mathf.Clamp01(holdTimer / holdTime);
            }

            // Hold E for the required amount of time.
            if (holdTimer >= holdTime)
            {
                UseMedkit();
            }
        }
        else
        {
            // Player released E before finishing.
            ResetHealing();
        }
    }

    private void UseMedkit()
    {
        if (playerInRange == null)
        {
            return;
        }

        // Don't use the medkit if health is already full.
        if (playerInRange.currentHealth >= playerInRange.maxHealth)
        {
            ResetHealing();
            return;
        }

        playerInRange.Heal(healAmount);

        Debug.Log(
            "Medkit used. Player healed for " +
            healAmount +
            " HP."
        );

        // Remove the medkit after use.
        Destroy(gameObject);
    }

    private void ResetHealing()
    {
        holdTimer = 0f;

        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player != null)
        {
            playerInRange = player;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player != null &&
            player == playerInRange)
        {
            playerInRange = null;

            ResetHealing();
        }
    }
}
