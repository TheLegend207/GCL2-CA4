using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class HostileAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform attackPoint;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float patrolRadius = 10f;

    [Header("Detection")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    [Header("Combat")]
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeHitDelay = 0.3f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Audio")]
    [Tooltip("The looping sound the zombie makes while alive.")]
    [SerializeField] private AudioClip zombieAudio;

    [Tooltip("The sound played when the zombie dies.")]
    [SerializeField] private AudioClip deathAudio;

    [Tooltip("Volume of the normal zombie sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float zombieAudioVolume = 1f;

    [Tooltip("Volume of the death sound.")]
    [Range(0f, 1f)]
    [SerializeField] private float deathAudioVolume = 1f;

    [Tooltip("How far away the zombie's sound can be heard.")]
    [SerializeField] private float audioMaxDistance = 20f;

    private AudioSource audioSource;

    private float currentHealth;
    private bool isDead;
    private bool isOnAttackCooldown;
    private bool destroyCalled;

    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    // --------------------------------------------------
    // AWAKE
    // --------------------------------------------------

    private void Awake()
    {
        currentHealth = maxHealth;

        // Get references automatically if they haven't
        // been assigned in the Inspector.

        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");

            if (player != null)
                playerTransform = player.transform;
        }

        if (attackPoint == null)
            attackPoint = transform;

        // Get AudioSource.
        audioSource = GetComponent<AudioSource>();

        // Configure the AudioSource.
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = audioMaxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    // --------------------------------------------------
    // START
    // --------------------------------------------------

    private void Start()
    {
        PlayZombieAudio();
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (isDead)
            return;

        DetectPlayer();
        UpdateBehaviourState();
    }

    // --------------------------------------------------
    // NORMAL ZOMBIE AUDIO
    // --------------------------------------------------

    private void PlayZombieAudio()
    {
        if (zombieAudio == null)
        {
            Debug.LogWarning(
                $"Zombie '{gameObject.name}' has no zombie audio assigned."
            );

            return;
        }

        audioSource.clip = zombieAudio;
        audioSource.volume = zombieAudioVolume;
        audioSource.loop = true;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // --------------------------------------------------
    // PLAYER DETECTION
    // --------------------------------------------------

    private void DetectPlayer()
    {
        isPlayerVisible = Physics.CheckSphere(
            transform.position,
            visionRange,
            playerLayerMask
        );

        isPlayerInRange = Physics.CheckSphere(
            transform.position,
            engagementRange,
            playerLayerMask
        );
    }

    // --------------------------------------------------
    // BEHAVIOUR
    // --------------------------------------------------

    private void UpdateBehaviourState()
    {
        if (!isPlayerVisible && !isPlayerInRange)
        {
            Patrol();
        }
        else if (isPlayerVisible && !isPlayerInRange)
        {
            Chase();
        }
        else
        {
            Attack();
        }
    }

    // --------------------------------------------------
    // PATROL
    // --------------------------------------------------

    private void Patrol()
    {
        if (navAgent == null || !navAgent.enabled)
            return;

        navAgent.isStopped = false;
        navAgent.speed = patrolSpeed;

        if (!hasPatrolPoint)
            FindPatrolPoint();

        if (hasPatrolPoint)
            navAgent.SetDestination(currentPatrolPoint);

        if (Vector3.Distance(
                transform.position,
                currentPatrolPoint) < 1f)
        {
            hasPatrolPoint = false;
        }
    }

    // --------------------------------------------------
    // FIND PATROL POINT
    // --------------------------------------------------

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(
            -patrolRadius,
            patrolRadius
        );

        float randomZ = Random.Range(
            -patrolRadius,
            patrolRadius
        );

        Vector3 point = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(
                point,
                Vector3.down,
                2f,
                terrainLayer))
        {
            currentPatrolPoint = point;
            hasPatrolPoint = true;
        }
    }

    // --------------------------------------------------
    // CHASE
    // --------------------------------------------------

    private void Chase()
    {
        if (navAgent == null || !navAgent.enabled)
            return;

        navAgent.isStopped = false;

        // Zombie becomes faster when it sees the player.
        navAgent.speed = chaseSpeed;

        if (playerTransform != null)
        {
            navAgent.SetDestination(
                playerTransform.position
            );
        }
    }

    // --------------------------------------------------
    // ATTACK
    // --------------------------------------------------

    private void Attack()
    {
        if (playerTransform == null)
            return;

        if (navAgent == null || !navAgent.enabled)
            return;

        float distance = Vector3.Distance(
            transform.position,
            playerTransform.position
        );

        if (distance > meleeRange)
        {
            navAgent.isStopped = false;
            navAgent.speed = chaseSpeed;

            navAgent.SetDestination(
                playerTransform.position
            );
        }
        else
        {
            navAgent.isStopped = true;

            Vector3 lookDirection =
                playerTransform.position -
                transform.position;

            lookDirection.y = 0f;

            if (lookDirection != Vector3.zero)
            {
                transform.rotation =
                    Quaternion.LookRotation(lookDirection);
            }

            if (!isOnAttackCooldown)
            {
                StartCoroutine(
                    MeleeAttackRoutine()
                );
            }
        }
    }

    // --------------------------------------------------
    // MELEE ATTACK
    // --------------------------------------------------

    private IEnumerator MeleeAttackRoutine()
    {
        isOnAttackCooldown = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(
            meleeHitDelay
        );

        if (isDead)
            yield break;

        if (attackPoint != null)
        {
            Collider[] hits = Physics.OverlapSphere(
                attackPoint.position,
                meleeRange,
                playerLayerMask
            );

            foreach (Collider hit in hits)
            {
                Debug.Log(
                    $"Hit {hit.name} for {meleeDamage} damage."
                );

                // Add your player's damage function here
                // if you want the zombie attack to actually
                // damage the player.
            }
        }

        float remainingCooldown =
            attackCooldown - meleeHitDelay;

        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(
                remainingCooldown
            );
        }

        if (!isDead)
        {
            if (navAgent != null &&
                navAgent.enabled)
            {
                navAgent.isStopped = false;
            }

            isOnAttackCooldown = false;
        }
    }

    // --------------------------------------------------
    // TAKE DAMAGE
    // --------------------------------------------------

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(
            $"{gameObject.name} HP: {currentHealth}"
        );

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        // ----------------------------------------------
        // STOP NAVMESH
        // ----------------------------------------------

        if (navAgent != null &&
            navAgent.enabled)
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;

            // Stop NavMeshAgent from moving the transform.
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        // ----------------------------------------------
        // DISABLE COLLIDERS
        // ----------------------------------------------

        Collider[] colliders =
            GetComponents<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // ----------------------------------------------
        // STOP NORMAL ZOMBIE AUDIO
        // ----------------------------------------------

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // ----------------------------------------------
        // PLAY DEATH AUDIO
        // ----------------------------------------------

        if (deathAudio != null)
        {
            audioSource.volume = deathAudioVolume;

            audioSource.PlayOneShot(
                deathAudio
            );
        }
        else
        {
            Debug.LogWarning(
                $"Zombie '{gameObject.name}' has no death audio assigned."
            );
        }

        // ----------------------------------------------
        // PLAY DEATH ANIMATION
        // ----------------------------------------------

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            // If there is somehow no Animator,
            // destroy the zombie after the audio.
            StartCoroutine(
                DestroyAfterAudio()
            );
        }
    }

    // --------------------------------------------------
    // ANIMATION EVENT
    // --------------------------------------------------
    //
    // Put an Animation Event on the LAST FRAME
    // of your Death animation.
    //
    // Function:
    // DestroyZombie
    //
    // --------------------------------------------------

    public void DestroyZombie()
    {
        if (destroyCalled)
            return;

        destroyCalled = true;

        // Disable NavMeshAgent.
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        // If the death sound is still playing,
        // wait for it before destroying the zombie.
        if (audioSource != null &&
            audioSource.isPlaying)
        {
            StartCoroutine(
                DestroyAfterAudio()
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --------------------------------------------------
    // WAIT FOR DEATH AUDIO
    // --------------------------------------------------

    private IEnumerator DestroyAfterAudio()
    {
        if (audioSource != null &&
            audioSource.isPlaying)
        {
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }

        Destroy(gameObject);
    }

    // --------------------------------------------------
    // BULLET HIT
    // --------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Bullet"))
        {
            // Every bullet does 30 damage.
            TakeDamage(30f);

            Destroy(other.gameObject);
        }
    }

    // --------------------------------------------------
    // GIZMOS
    // --------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            visionRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            engagementRange
        );

        Gizmos.color = Color.magenta;

        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(
                attackPoint.position,
                meleeRange
            );
        }
    }
}