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
    [SerializeField] private AudioClip zombieAudio;
    [SerializeField] private AudioClip deathAudio;

    [Range(0f, 1f)]
    [SerializeField] private float zombieAudioVolume = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float deathAudioVolume = 0.5f;

    [SerializeField] private float audioMaxDistance = 20f;

    [Header("Death Fade")]
    [Tooltip("How long the death animation plays before the fade begins.")]
    [SerializeField] private float deathAnimationDuration = 2.5f;

    [Tooltip("How long the zombie takes to completely fade out.")]
    [SerializeField] private float fadeDuration = 2f;

    [Tooltip("How much the zombie rises while fading.")]
    [SerializeField] private float deathRiseAmount = 0.5f;

    private AudioSource audioSource;

    private float currentHealth;

    private bool isDead;
    private bool isOnAttackCooldown;
    private bool destroyCalled;

    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    // All renderers on the zombie and its children.
    private Renderer[] zombieRenderers;

    // Used to change material transparency without creating
    // a new material every frame.
    private MaterialPropertyBlock propertyBlock;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        currentHealth = maxHealth;

        // -----------------------------------------------------
        // NAVMESH
        // -----------------------------------------------------

        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            navAgent.updatePosition = true;
            navAgent.updateRotation = true;
        }

        // -----------------------------------------------------
        // ANIMATOR
        // -----------------------------------------------------

        if (animator == null)
            animator = GetComponent<Animator>();

        // -----------------------------------------------------
        // PLAYER
        // -----------------------------------------------------

        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");

            if (player != null)
                playerTransform = player.transform;
        }

        // -----------------------------------------------------
        // ATTACK POINT
        // -----------------------------------------------------

        if (attackPoint == null)
            attackPoint = transform;

        // -----------------------------------------------------
        // AUDIO
        // -----------------------------------------------------

        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // Make the audio 3D.
        audioSource.spatialBlend = 1f;

        audioSource.maxDistance = audioMaxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // -----------------------------------------------------
        // RENDERERS
        // -----------------------------------------------------

        zombieRenderers =
            GetComponentsInChildren<Renderer>();

        propertyBlock =
            new MaterialPropertyBlock();
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        PlayZombieAudio();

        FindPatrolPoint();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (isDead)
            return;

        DetectPlayer();

        UpdateBehaviourState();
    }


    // =========================================================
    // AUDIO
    // =========================================================

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


    // =========================================================
    // PLAYER DETECTION
    // =========================================================

    private void DetectPlayer()
    {
        if (playerTransform == null)
        {
            isPlayerVisible = false;
            isPlayerInRange = false;
            return;
        }

        isPlayerVisible =
            Physics.CheckSphere(
                transform.position,
                visionRange,
                playerLayerMask
            );

        isPlayerInRange =
            Physics.CheckSphere(
                transform.position,
                engagementRange,
                playerLayerMask
            );
    }


    // =========================================================
    // BEHAVIOUR
    // =========================================================

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


    // =========================================================
    // PATROL
    // =========================================================

    private void Patrol()
    {
        if (navAgent == null ||
            !navAgent.enabled)
            return;

        navAgent.isStopped = false;
        navAgent.speed = patrolSpeed;

        if (!hasPatrolPoint)
        {
            FindPatrolPoint();
        }

        if (hasPatrolPoint)
        {
            navAgent.SetDestination(
                currentPatrolPoint
            );

            if (!navAgent.pathPending &&
                navAgent.remainingDistance <= 1f)
            {
                hasPatrolPoint = false;
            }
        }
    }


    // =========================================================
    // FIND PATROL POINT
    // =========================================================

    private void FindPatrolPoint()
    {
        if (navAgent == null ||
            !navAgent.enabled)
            return;

        for (int i = 0; i < 20; i++)
        {
            Vector3 randomDirection =
                Random.insideUnitSphere *
                patrolRadius;

            randomDirection.y = 0f;

            Vector3 randomPoint =
                transform.position +
                randomDirection;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPoint,
                out hit,
                patrolRadius,
                NavMesh.AllAreas))
            {
                currentPatrolPoint =
                    hit.position;

                hasPatrolPoint = true;

                return;
            }
        }

        // Fallback to current position.

        NavMeshHit currentHit;

        if (NavMesh.SamplePosition(
            transform.position,
            out currentHit,
            5f,
            NavMesh.AllAreas))
        {
            currentPatrolPoint =
                currentHit.position;

            hasPatrolPoint = true;
        }
    }


    // =========================================================
    // CHASE
    // =========================================================

    private void Chase()
    {
        if (navAgent == null ||
            !navAgent.enabled ||
            playerTransform == null)
            return;

        navAgent.isStopped = false;

        navAgent.speed = chaseSpeed;

        navAgent.SetDestination(
            playerTransform.position
        );
    }


    // =========================================================
    // ATTACK
    // =========================================================

    private void Attack()
    {
        if (playerTransform == null)
            return;

        if (navAgent == null ||
            !navAgent.enabled)
            return;

        float distance =
            Vector3.Distance(
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

            return;
        }

        navAgent.isStopped = true;

        Vector3 lookDirection =
            playerTransform.position -
            transform.position;

        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    lookDirection
                );
        }

        if (!isOnAttackCooldown)
        {
            StartCoroutine(
                MeleeAttackRoutine()
            );
        }
    }


    // =========================================================
    // MELEE ATTACK
    // =========================================================

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
            Collider[] hits =
                Physics.OverlapSphere(
                    attackPoint.position,
                    meleeRange,
                    playerLayerMask
                );

            foreach (Collider hit in hits)
            {
                Debug.Log(
                    $"Hit {hit.name} for {meleeDamage} damage."
                );
            }
        }

        float remainingCooldown =
            attackCooldown -
            meleeHitDelay;

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


    // =========================================================
    // TAKE DAMAGE
    // =========================================================

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


    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        // -----------------------------------------------------
        // STOP NAVMESH
        // -----------------------------------------------------

        if (navAgent != null &&
            navAgent.enabled)
        {
            navAgent.isStopped = true;

            navAgent.velocity =
                Vector3.zero;

            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        // -----------------------------------------------------
        // DISABLE COLLIDERS
        // -----------------------------------------------------

        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // -----------------------------------------------------
        // STOP NORMAL AUDIO
        // -----------------------------------------------------

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // -----------------------------------------------------
        // DEATH AUDIO
        // -----------------------------------------------------

        if (deathAudio != null)
        {
            audioSource.volume =
                deathAudioVolume;

            audioSource.PlayOneShot(
                deathAudio
            );
        }

        // -----------------------------------------------------
        // DEATH ANIMATION
        // -----------------------------------------------------

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // -----------------------------------------------------
        // START FADE
        // -----------------------------------------------------

        StartCoroutine(
            DeathFadeRoutine()
        );
    }


    // =========================================================
    // DEATH FADE
    // =========================================================

    private IEnumerator DeathFadeRoutine()
    {
        // -----------------------------------------------------
        // WAIT FOR DEATH ANIMATION
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            deathAnimationDuration
        );


        // -----------------------------------------------------
        // STORE START/END POSITIONS
        // -----------------------------------------------------

        Vector3 startPosition =
            transform.position;

        Vector3 endPosition =
            startPosition +
            Vector3.up *
            deathRiseAmount;


        float elapsed = 0f;


        // -----------------------------------------------------
        // FADE
        // -----------------------------------------------------

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );


            // -------------------------------------------------
            // SLOWLY RISE
            // -------------------------------------------------

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );


            // -------------------------------------------------
            // SLOWLY FADE
            // -------------------------------------------------

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );


            SetZombieAlpha(alpha);


            yield return null;
        }


        // -----------------------------------------------------
        // FULLY INVISIBLE
        // -----------------------------------------------------

        SetZombieAlpha(0f);


        // -----------------------------------------------------
        // DISABLE NAVMESH
        // -----------------------------------------------------

        if (navAgent != null)
        {
            navAgent.enabled = false;
        }


        // -----------------------------------------------------
        // DESTROY
        // -----------------------------------------------------

        Destroy(gameObject);
    }


    // =========================================================
    // SET ZOMBIE TRANSPARENCY
    // =========================================================

    private void SetZombieAlpha(float alpha)
    {
        if (zombieRenderers == null)
            return;


        foreach (Renderer rend in zombieRenderers)
        {
            if (rend == null)
                continue;


            rend.GetPropertyBlock(
                propertyBlock
            );


            // -------------------------------------------------
            // URP LIT BASE COLOR
            // -------------------------------------------------

            Color baseColor =
                Color.white;


            if (rend.sharedMaterial != null &&
                rend.sharedMaterial.HasProperty(
                    "_BaseColor"))
            {
                baseColor =
                    rend.sharedMaterial.GetColor(
                        "_BaseColor"
                    );
            }


            baseColor.a = alpha;


            propertyBlock.SetColor(
                "_BaseColor",
                baseColor
            );


            rend.SetPropertyBlock(
                propertyBlock
            );
        }
    }


    // =========================================================
    // BULLET DAMAGE
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (isDead)
            return;


        if (other.CompareTag("Bullet"))
        {
            TakeDamage(30f);

            Destroy(
                other.gameObject
            );
        }
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            visionRange
        );


        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            engagementRange
        );


        Gizmos.color =
            Color.magenta;

        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(
                attackPoint.position,
                meleeRange
            );
        }


        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            patrolRadius
        );
    }
}