using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    [Header("Death Effects")]
    [SerializeField] private float deathAnimationLength = 2.97f;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float shrinkToScale = 0.8f;

    private float currentHealth;
    private bool isDead;
    private bool isOnAttackCooldown;

    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private Renderer[] renderers;
    private Vector3 originalScale;

    private void Awake()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;

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

        renderers = GetComponentsInChildren<Renderer>();

        // Give every zombie its own material instance.
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(mats[i]);
            }

            r.materials = mats;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        DetectPlayer();
        UpdateBehaviourState();
    }

    private void DetectPlayer()
    {
        isPlayerVisible = Physics.CheckSphere(transform.position, visionRange, playerLayerMask);
        isPlayerInRange = Physics.CheckSphere(transform.position, engagementRange, playerLayerMask);
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
        navAgent.speed = patrolSpeed;

        if (!hasPatrolPoint)
            FindPatrolPoint();

        if (hasPatrolPoint)
            navAgent.SetDestination(currentPatrolPoint);

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
            hasPatrolPoint = false;
    }

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 point = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ);

        if (Physics.Raycast(point + Vector3.up, Vector3.down, 5f, terrainLayer))
        {
            currentPatrolPoint = point;
            hasPatrolPoint = true;
        }
    }

    private void Chase()
    {
        navAgent.speed = chaseSpeed;

        if (playerTransform != null)
            navAgent.SetDestination(playerTransform.position);
    }

    private void Attack()
    {
        if (playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > meleeRange)
        {
            navAgent.speed = chaseSpeed;
            navAgent.SetDestination(playerTransform.position);
        }
        else
        {
            navAgent.isStopped = true;

            Vector3 lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);

            if (!isOnAttackCooldown)
                StartCoroutine(MeleeAttackRoutine());
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        isOnAttackCooldown = true;

        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(meleeHitDelay);

        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            meleeRange,
            playerLayerMask);

        foreach (Collider hit in hits)
        {
            Debug.Log($"Hit {hit.name} for {meleeDamage} damage.");
            // Add player damage here
        }

        yield return new WaitForSeconds(attackCooldown - meleeHitDelay);

        navAgent.isStopped = false;
        isOnAttackCooldown = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log($"{gameObject.name} HP: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;

        StopAllCoroutines();

        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;

        foreach (Collider col in GetComponents<Collider>())
            col.enabled = false;

        animator.SetTrigger("Die");

        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(deathAnimationLength);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            float alpha = Mathf.Lerp(1f, 0f, t);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color c = mat.GetColor("_BaseColor");
                        c.a = alpha;
                        mat.SetColor("_BaseColor", c);
                    }
                }
            }

            float scale = Mathf.Lerp(1f, shrinkToScale, t);
            transform.localScale = originalScale * scale;

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Bullet"))
        {
            TakeDamage(100f);
            Destroy(other.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagementRange);

        Gizmos.color = Color.magenta;

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, meleeRange);
    }
}