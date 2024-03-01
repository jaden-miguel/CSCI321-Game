using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Patroller : MonoBehaviour
{

    public float detectionRange = 10f;
    public float chaseStopDistance = 8f;
    public float attackDistance = 0.5f;
    public float chaseDuration = 0.95f;
    public float randomness = 1.05f;
    public float runAwayDistance = 13.73f;

    private Transform[] waypoints;
    private Transform targetBell;
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
                    targetBell = null;
                    StartChasing();
                }
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f && !isChasing)
        {
            SetDestinationToWaypoint();
        }

        if (distanceToPlayer <= attackDistance && !hasAttacked)
        {
            AttackPlayer();
        }

        if (isChasing && !hasAttacked)
        {
            if (targetBell != null)
            {
                SetDestination(new Vector3(targetBell.position.x, 0f, targetBell.position.z));
                print(new Vector3(targetBell.position.x, 0f, targetBell.position.z));

                // Check if the enemy has reached the bell position
                if (Vector3.Distance(transform.position, targetBell.position) <= chaseStopDistance)
                {
                    isChasing = false;
                    agent.speed = 1.25f; // Reset the speed to regular patrol speed
                    targetBell = null; // Reset the target bell
                    SetDestinationToWaypoint(); // Go back to regular patrol behavior
                }
            } else
            {
                SetDestination(player.position);
            }
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

        StartCoroutine(AttackAndRunAway());
    }

    IEnumerator UpdateRotationDuringAttack()
    {
        while (animator.GetBool("IsAttacking"))
        {
            // Make enemy face the player during attack
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z)); // Only rotate around Y-axis
            transform.rotation = lookRotation;
            yield return null; // Wait for the next frame
        }
    }

    IEnumerator AttackAndRunAway()
    {
        yield return new WaitForSeconds(2.5f); // Wait for the attack animation to finish
        agent.isStopped = false;

        Vector3 directionToPlayer = player.position - transform.position;
        Vector3 runAwayDestination = transform.position - directionToPlayer.normalized * runAwayDistance;
        SetDestination(runAwayDestination);

        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsWalking", true);

        //yield return new WaitForSeconds(5f); // Wait for the enemy to run away
        hasAttacked = false; // Reset the attack state
        agent.speed = 0.5f; // Start with a lower speed

        StartCoroutine(IncreaseSpeedOverTime(2f, 3f)); // Gradually increase speed over 2 seconds to 3 units/second
    }


    IEnumerator IncreaseSpeedOverTime(float duration, float targetSpeed)
    {
        float startSpeed = agent.speed;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            agent.speed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agent.speed = targetSpeed;
    }

    // Function to go to the recently rung bell
    public void GoToBell(Transform bellTransform)
    {
        targetBell = bellTransform;
        isChasing = true;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsChasing", true);
        agent.speed = 3.5f; // Set the speed to chase the bell faster if needed
    }

    private void SetDestinationToWaypoint()
    {
        agent.speed = 1.25f;
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}
