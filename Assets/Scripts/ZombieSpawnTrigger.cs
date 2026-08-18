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
        //activate when the player enters
        if (!other.CompareTag("Player"))
            return;

        // prevent the trigger from activating more than once
        if (spawnOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        PlayHordeAudio();

        // Show the horde text
        if (hordeText != null)
        {
            StartCoroutine(FadeHordeText());
        }

        // Spawn the zombies
        SpawnZombies();
    }


    private void PlayHordeAudio() //sound plaeyd when zombies horde spawn
    {
        if (hordeAudio == null)
        {
            Debug.LogWarning("Zombie Spawn Trigger: No horde audio assigned.");
            return;
        }

        audioSource.PlayOneShot(hordeAudio);
    }



    private IEnumerator FadeHordeText()     //message fades away
    {
        hordeText.gameObject.SetActive(true);

        hordeText.text = "The Horde is coming..";

        // Start completely transparent.
        Color textColor = hordeText.color;
        textColor.a = 0f;
        hordeText.color = textColor;

        //zombies fade into existence
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

        // Make absolutely sure it's fully visible
        textColor.a = 1f;
        hordeText.color = textColor;

        yield return new WaitForSeconds(textDisplayTime); //wait a few seconds

        timer = 0f;

        while (timer < fadeOutDuration) //text fades away
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

        // completely hide the text
        textColor.a = 0f;
        hordeText.color = textColor;

        hordeText.gameObject.SetActive(false);
    }


    private void SpawnZombies() //script for spawning zombie horde
    {
        if (zombiePrefab1 == null) //for errors
        {
            Debug.LogError(
                "Zombie Spawn Trigger: Zombie Prefab 1 is not assigned!"
            );

            return;
        }

        if (zombiePrefab2 == null) //for errors
        {
            Debug.LogError(
                "Zombie Spawn Trigger: Zombie Prefab 2 is not assigned!"
            );

            return;
        }

        if (spawnPoint == null) //for errors
        {
            Debug.LogError(
                "Zombie Spawn Trigger: Spawn Point is not assigned!"
            );

            return;
        }

        for (int i = 0; i < zombieCount; i++)
        {
            // Randomly select zombie type
            GameObject selectedZombie;

            if (Random.value < 0.5f)
            {
                selectedZombie = zombiePrefab1;
            }
            else
            {
                selectedZombie = zombiePrefab2;
            }

            // find a random position around the spawn point
            Vector3 randomPosition = GetRandomSpawnPosition();

            Instantiate( //spawn zombie
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


    private Vector3 GetRandomSpawnPosition() //randomise spawning position with a circle
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        Vector3 randomPosition = new Vector3(
            spawnPoint.position.x + randomCircle.x,
            spawnPoint.position.y,
            spawnPoint.position.z + randomCircle.y
        );

        return randomPosition;
    }


    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null)
            return;

        // Show the spawn area
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            spawnPoint.position,
            spawnRadius
        );

        // Show the center spawn point
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            spawnPoint.position,
            0.3f
        );

        // Show the direction zombies will face
        Gizmos.color = Color.blue;

        Gizmos.DrawLine(
            spawnPoint.position,
            spawnPoint.position + spawnPoint.forward * 2f
        );
    }
}