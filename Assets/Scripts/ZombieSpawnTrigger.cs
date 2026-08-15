using UnityEngine;
using TMPro;
using System.Collections;

public class ZombieSpawnTrigger : MonoBehaviour
{
    [Header("Zombie Settings")]
    [SerializeField] private GameObject zombiePrefab1;
    [SerializeField] private GameObject zombiePrefab2;
    [SerializeField] private int zombieCount = 10;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("How far away from the spawn point zombies can appear.")]
    [SerializeField] private float spawnRadius = 5f;

    [Header("Trigger Settings")]
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Horde Warning")]
    [SerializeField] private AudioClip hordeAudio;
    [SerializeField] private TMP_Text hordeText;

    [Tooltip("How long the text takes to fade in.")]
    [SerializeField] private float fadeInDuration = 1f;

    [Tooltip("How long the text stays fully visible.")]
    [SerializeField] private float textDisplayTime = 3f;

    [Tooltip("How long the text takes to fade out.")]
    [SerializeField] private float fadeOutDuration = 1f;

    private bool hasTriggered = false;

    private AudioSource audioSource;

    private void Awake()
    {
        // Find or create AudioSource.
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Make sure the text starts invisible.
        if (hordeText != null)
        {
            Color textColor = hordeText.color;
            textColor.a = 0f;
            hordeText.color = textColor;

            hordeText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only activate when the Player enters.
        if (!other.CompareTag("Player"))
            return;

        // Prevent the trigger from activating more than once.
        if (spawnOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        // Play the warning audio.
        PlayHordeAudio();

        // Show the warning text.
        if (hordeText != null)
        {
            StartCoroutine(FadeHordeText());
        }

        // Spawn the zombies.
        SpawnZombies();
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayHordeAudio()
    {
        if (hordeAudio == null)
        {
            Debug.LogWarning("Zombie Spawn Trigger: No horde audio assigned.");
            return;
        }

        audioSource.PlayOneShot(hordeAudio);
    }

    // =========================================================
    // TEXT FADE
    // =========================================================

    private IEnumerator FadeHordeText()
    {
        hordeText.gameObject.SetActive(true);

        hordeText.text = "The Horde is coming..";

        // Start completely transparent.
        Color textColor = hordeText.color;
        textColor.a = 0f;
        hordeText.color = textColor;

        // -----------------------------------------------------
        // FADE IN
        // -----------------------------------------------------

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Clamp01(
                timer / fadeInDuration
            );

            textColor.a = alpha;
            hordeText.color = textColor;

            yield return null;
        }

        // Make absolutely sure it's fully visible.
        textColor.a = 1f;
        hordeText.color = textColor;

        // -----------------------------------------------------
        // STAY VISIBLE
        // -----------------------------------------------------

        yield return new WaitForSeconds(textDisplayTime);

        // -----------------------------------------------------
        // FADE OUT
        // -----------------------------------------------------

        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(
                1f,
                0f,
                timer / fadeOutDuration
            );

            textColor.a = alpha;
            hordeText.color = textColor;

            yield return null;
        }

        // Completely hide the text.
        textColor.a = 0f;
        hordeText.color = textColor;

        hordeText.gameObject.SetActive(false);
    }

    // =========================================================
    // ZOMBIE SPAWNING
    // =========================================================

    private void SpawnZombies()
    {
        if (zombiePrefab1 == null)
        {
            Debug.LogError(
                "Zombie Spawn Trigger: Zombie Prefab 1 is not assigned!"
            );

            return;
        }

        if (zombiePrefab2 == null)
        {
            Debug.LogError(
                "Zombie Spawn Trigger: Zombie Prefab 2 is not assigned!"
            );

            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError(
                "Zombie Spawn Trigger: Spawn Point is not assigned!"
            );

            return;
        }

        for (int i = 0; i < zombieCount; i++)
        {
            // Randomly select zombie type.
            GameObject selectedZombie;

            if (Random.value < 0.5f)
            {
                selectedZombie = zombiePrefab1;
            }
            else
            {
                selectedZombie = zombiePrefab2;
            }

            // Find a random position around the spawn point.
            Vector3 randomPosition = GetRandomSpawnPosition();

            // Spawn the zombie.
            Instantiate(
                selectedZombie,
                randomPosition,
                spawnPoint.rotation
            );

            Debug.Log(
                "Spawned " + selectedZombie.name +
                " at " + randomPosition
            );
        }
    }

    // =========================================================
    // RANDOM SPAWN POSITION
    // =========================================================

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        Vector3 randomPosition = new Vector3(
            spawnPoint.position.x + randomCircle.x,
            spawnPoint.position.y,
            spawnPoint.position.z + randomCircle.y
        );

        return randomPosition;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null)
            return;

        // Show the spawn area.
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            spawnPoint.position,
            spawnRadius
        );

        // Show the center spawn point.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            spawnPoint.position,
            0.3f
        );

        // Show the direction zombies will face.
        Gizmos.color = Color.blue;

        Gizmos.DrawLine(
            spawnPoint.position,
            spawnPoint.position + spawnPoint.forward * 2f
        );
    }
}