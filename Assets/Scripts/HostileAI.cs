using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class HostileAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayerMask;

    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 9f;
    [SerializeField] private float patrolRadius = 10f;

    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeHitDelay = 0.3f;
    [SerializeField] private float attackCooldown = 1f;

    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private AudioClip zombieAudio;
    [SerializeField] private AudioClip deathAudio;

    [Range(0f, 1f)]
    [SerializeField] private float zombieAudioVolume = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float deathAudioVolume = 0.5f;

    [SerializeField] private float audioMaxDistance = 20f;
    [SerializeField] private float deathAnimationDuration = 2.5f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float deathRiseAmount = 0.5f;

    // How quickly the zombie turns while chasing.
    [SerializeField] private float chaseTurnSpeed = 720f;

    private AudioSource audioSource;

    private float currentHealth;

    private bool isDead;
    private bool isOnAttackCooldown;
    private bool destroyCalled;

    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private Renderer[] zombieRenderers;
    private MaterialPropertyBlock propertyBlock;

    private int dmgtaken;
    private float slowdown;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            navAgent.updatePosition = true;
            navAgent.updateRotation = true;

            navAgent.angularSpeed = 720f;
            navAgent.acceleration = 30f;
            navAgent.autoBraking = false;
        }

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

        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = audioMaxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        zombieRenderers =
            GetComponentsInChildren<Renderer>();

        propertyBlock =
            new MaterialPropertyBlock();
    }

    private void Start()
    {
        PlayZombieAudio();

        FindPatrolPoint();
    }

    private void Update()
    {
        if (isDead)
            return;

        DetectPlayer();

        UpdateBehaviourState();
    }

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

    private void DetectPlayer()
    {
        if (playerTransform == null)
        {
            isPlayerVisible = false;
            isPlayerInRange = false;
            return;
        }

        // Check if the player is nearby.
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

    private void Patrol()
    {
        if (navAgent == null ||
            !navAgent.enabled)
            return;

        navAgent.isStopped = false;
        navAgent.updateRotation = true;
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

    // Find a random point nearby.
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

    private void Chase()
    {
        if (navAgent == null ||
            !navAgent.enabled ||
            playerTransform == null)
            return;

        navAgent.isStopped = false;
        navAgent.speed = chaseSpeed - slowdown;

        // Let the script handle turning while chasing.
        navAgent.updateRotation = false;

        navAgent.SetDestination(
            playerTransform.position
        );

        Vector3 lookDirection =
            playerTransform.position -
            transform.position;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    lookDirection
                );

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    chaseTurnSpeed * Time.deltaTime
                );
        }
    }

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
            navAgent.speed = chaseSpeed - slowdown;

            // Keep manual turning active while closing in.
            navAgent.updateRotation = false;

            navAgent.SetDestination(
                playerTransform.position
            );

            Vector3 chaseDirection =
                playerTransform.position -
                transform.position;

            chaseDirection.y = 0f;

            if (chaseDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        chaseDirection
                    );

                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        chaseTurnSpeed * Time.deltaTime
                    );
            }

            return;
        }

        navAgent.isStopped = true;
        navAgent.updateRotation = false;

        // Face the player before attacking.
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
                PlayerController player =
                    hit.GetComponent<PlayerController>();

                if (player != null)
                {
                    player.TakeDamage(
                        (int)meleeDamage
                    );
                }
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

    public void Hit()
    {
        // Ignore damage after death.
        if (isDead)
            return;

        BulletClass bulletclass =
            FindFirstObjectByType<BulletClass>();

        dmgtaken = bulletclass.damage;
        slowdown = bulletclass.slow;

        currentHealth -= dmgtaken;

        StartCoroutine(
            SlowedTimer()
        );

        Debug.Log(
            $"{gameObject.name} HP: {currentHealth}"
        );

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;

            Die();
        }
    }

    private IEnumerator SlowedTimer()
    {
        Debug.Log("Zombie is slowed.");

        yield return new WaitForSeconds(3f);

        slowdown = 0f;

        Debug.Log("Slow has ended.");
    }

    private void Die()
    {
        // Stop everything when the zombie dies.
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        if (navAgent != null &&
            navAgent.enabled)
        {
            navAgent.isStopped = true;

            navAgent.velocity =
                Vector3.zero;

            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // Play the death sound once.
        if (deathAudio != null)
        {
            audioSource.volume =
                deathAudioVolume;

            audioSource.PlayOneShot(
                deathAudio
            );
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        StartCoroutine(
            DeathFadeRoutine()
        );
    }

    private IEnumerator DeathFadeRoutine()
    {
        yield return new WaitForSeconds(
            deathAnimationDuration
        );

        Vector3 startPosition =
            transform.position;

        Vector3 endPosition =
            startPosition +
            Vector3.up *
            deathRiseAmount;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    progress
                );

            // Fade while the zombie rises.
            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            SetZombieAlpha(alpha);

            yield return null;
        }

        SetZombieAlpha(0f);

        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        Destroy(gameObject);
    }

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

    private void OnTriggerEnter(Collider other)
    {
        // Ignore bullets after death.
        if (isDead)
            return;

        if (other.CompareTag("Bullet"))
        {
            Hit();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show detection range.
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            visionRange
        );

        // Show engagement range.
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            engagementRange
        );

        // Show attack range.
        Gizmos.color =
            Color.magenta;

        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(
                attackPoint.position,
                meleeRange
            );
        }

        // Show patrol range.
        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            patrolRadius
        );
    }
}