using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent ai;
    public Transform player;
    private PlayerHealth playerHealth;

    [Header("Patrol")]
    public List<Transform> destinations;
    public float walkSpeed = 2f;
    public float chaseSpeed = 4f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [Header("Chase Settings")]
    public float minChaseTime = 5f;
    public float maxChaseTime = 10f;
    public float sightDistance = 15f;
    public float catchDistance = 2f;
    public Vector3 raycastOffset = Vector3.up;

    [Header("Combat")]
    public float damageAmount = 10f;
    public float attackPauseTime = 5f;

    private bool walking = true;
    private bool chasing = false;
    private bool canMove = true;
    private bool hasDealtDamage = false;
    private bool isIdle = false;

    private Transform currentDest;
    private Coroutine chaseRoutine;
    private Coroutine idleRoutine;

    void Start()
    {
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        PickNewDestination();
        Debug.Log("[EnemyAI] State: START, now Walking");
    }

    void Update()
    {
        if (!canMove) return;

        bool playerVisible = CanSeePlayer();

        // --- CHASE START ---
        if (playerVisible && !chasing)
        {
            // Forcefully reset all coroutines and states
            ResetAllCoroutines();

            chasing = true;
            walking = false;
            isIdle = false;
            ai.isStopped = false;

            chaseRoutine = StartCoroutine(ChaseRoutine());
            Debug.Log("[EnemyAI] State: Player spotted, now Chasing (forced resume)");
        }

        // --- CHASING BEHAVIOR ---
        if (chasing && canMove)
        {
            ai.speed = chaseSpeed;
            ai.destination = player.position;

            if (!hasDealtDamage && ai.remainingDistance <= catchDistance)
            {
                StartCoroutine(AttackPlayer());
            }
        }

        // --- PATROLLING BEHAVIOR ---
        if (walking && canMove)
        {
            ai.speed = walkSpeed;
            ai.destination = currentDest.position;

            if (ai.remainingDistance <= ai.stoppingDistance && !isIdle)
            {
                if (Random.value < 0.5f)
                {
                    PickNewDestination();
                    Debug.Log("[EnemyAI] State: Walking, now New destination");
                }
                else
                {
                    idleRoutine = StartCoroutine(StayIdle());
                }
            }
        }
    }

    // --- Vision Check ---
    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 direction = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + raycastOffset, direction, out RaycastHit hit, sightDistance))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    // --- Idle Coroutine ---
    IEnumerator StayIdle()
    {
        isIdle = true;
        ai.isStopped = true;

        float idleTime = Random.Range(minIdleTime, maxIdleTime);
        Debug.Log("[EnemyAI] State: Walking, now Idle (" + idleTime + "s)");

        float timer = 0f;
        while (timer < idleTime)
        {
            if (chasing) yield break; // cancel idle immediately if chase starts
            timer += Time.deltaTime;
            yield return null;
        }

        ai.isStopped = false;
        isIdle = false;
        walking = true;
        PickNewDestination();

        Debug.Log("[EnemyAI] State: Idle finished, now Walking");
    }

    // --- Chase Coroutine ---
    IEnumerator ChaseRoutine()
    {
        float chaseTime = Random.Range(minChaseTime, maxChaseTime);
        float elapsed = 0f;

        while (elapsed < chaseTime)
        {
            if (!CanSeePlayer()) break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        chasing = false;
        walking = true;
        PickNewDestination();

        Debug.Log("[EnemyAI] State: Chase ended, now Walking");
    }

    // --- Attack Sequence ---
    IEnumerator AttackPlayer()
    {
        hasDealtDamage = true;
        canMove = false;
        ai.isStopped = true;

        Debug.Log("[EnemyAI] State: ATTACK, now Pause for " + attackPauseTime + "s");

        if (playerHealth != null)
            playerHealth.TakeDamage(damageAmount);

        yield return new WaitForSeconds(attackPauseTime);

        ai.isStopped = false;
        canMove = true;
        hasDealtDamage = false;

        // Resume correct state depending on visibility
        if (CanSeePlayer())
        {
            chasing = true;
            walking = false;
            ResetAllCoroutines();
            chaseRoutine = StartCoroutine(ChaseRoutine());
            Debug.Log("[EnemyAI] State: Attack pause over, now Chasing again");
        }
        else
        {
            chasing = false;
            walking = true;
            PickNewDestination();
            Debug.Log("[EnemyAI] State: Attack pause over, now Walking");
        }
    }

    // --- Reset all coroutines and movement ---
    private void ResetAllCoroutines()
    {
        StopAllCoroutines();
        isIdle = false;
        ai.isStopped = false;
    }

    // --- Pick Random Destination ---
    void PickNewDestination()
    {
        if (destinations == null || destinations.Count == 0) return;
        currentDest = destinations[Random.Range(0, destinations.Count)];
        ai.destination = currentDest.position;
    }
}
