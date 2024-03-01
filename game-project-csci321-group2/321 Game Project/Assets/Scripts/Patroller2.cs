using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Patroller2 : MonoBehaviour
{
    public float detectionRange = 10f;
    public float chaseStopDistance = 5.1f;
    public float attackDistance = 1.31f;
    public float chaseDuration = 0.95f;
    public float randomness = 1.05f;
    public float attackCooldown = 2.31f; // Cooldown time between attacks
    public int health = 3;

    private Transform[] waypoints;
    private Transform player;
    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private Animator animator;
    private bool isChasing;
    private bool hasAttacked;

    void Start()
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        waypoints = new Transform[waypointObjects.Length];
        for (int i = 0; i < waypointObjects.Length; i++)
        {
            waypoints[i] = waypointObjects[i].transform;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        currentWaypointIndex = 0;
        isChasing = false;
        hasAttacked = false;

        animator.SetBool("IsWalking", true);

        PlayerMovement.OnPlayerRespawn += StopChasing; // Subscribe to the OnPlayerRespawn event

        SetDestinationToWaypoint();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        RaycastHit hit;
        if (distanceToPlayer <= detectionRange && !isChasing && !hasAttacked)
        {
            if (Physics.Raycast(transform.position, player.position - transform.position, out hit, detectionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    StartChasing();
                }
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f && !isChasing)
        {
            SetDestinationToWaypoint();
        }

        // Check if player is close enough to attack and if the enemy is not already attacking
        if (distanceToPlayer <= attackDistance && !animator.GetBool("IsAttacking"))
        {
            AttackPlayer();
        }

        // If the enemy is attacking and the attack has ended, start the cooldown before next attack
        if (animator.GetBool("IsAttacking") && hasAttacked)
        {
            hasAttacked = false;
            StartCoroutine(AttackCooldown());
        }

        if (isChasing && !hasAttacked)
        {
            SetDestination(player.position);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void StartChasing()
    {
        isChasing = true;
        agent.speed = 2.25f;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsChasing", true);
        StartCoroutine(ChaseForSeconds(chaseDuration));
    }

    IEnumerator ChaseForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (!hasAttacked)
        {
            isChasing = false;
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsChasing", false);
        }
    }

    void SetDestination(Vector3 destination)
    {
        if (agent != null)
        {
            destination += new Vector3(Random.Range(-randomness, randomness), 0, Random.Range(-randomness, randomness));
            agent.SetDestination(destination);
        }
    }

    void AttackPlayer()
    {
        isChasing = false;
        hasAttacked = true;
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsAttacking", true);
        agent.isStopped = true;

        // Start continuously updating enemy's rotation to face the player during the attack animation
        StartCoroutine(UpdateRotationDuringAttack());

        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(1);
        }
    }

    IEnumerator UpdateRotationDuringAttack()
    {
        while (animator.GetBool("IsAttacking"))
        {
            // Make enemy face the player during attack
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z)); // Only rotate around Y-axis
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Smoothly rotate over time
            yield return null; // Wait for the next frame
        }
    }


    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        animator.SetBool("IsAttacking", false);
        agent.isStopped = false;
        isChasing = true;
        animator.SetBool("IsChasing", true);
    }

    void SetDestinationToWaypoint()
    {
        if (waypoints.Length == 0)
            return;

        SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }


    void OnDestroy()
    {
        PlayerMovement.OnPlayerRespawn -= StopChasing; // Unsubscribe when this object is destroyed
    }

    private void StopChasing()
    {
        isChasing = false;
        hasAttacked = false;
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsAttacking", false);
        agent.isStopped = false;
        SetDestinationToWaypoint();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Patroller took " + damage + " damage. Remaining health: " + health);

        // If health falls below or equals zero after taking damage, the patroller dies
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Play the death animation
        animator.SetTrigger("IsDead");

        // Destroy the patroller after it dies
        Destroy(gameObject, 3f); // Wait for 3 seconds to allow the death animation to play
    }
}
