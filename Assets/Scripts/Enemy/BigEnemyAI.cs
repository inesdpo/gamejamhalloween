using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BigEnemyAI : MonoBehaviour
{
    [Header("Core Components")]
    private NavMeshAgent agent;
    private AudioSource footstepAudio;

    [Header("References")]
    public GameObject target;

    [Header("Flee + ChaseFlee Settings")]
    public float triggerDistance = 15f;       // Distance to trigger flee/chase behaviors
    public float fleeSpeed = 10f;
    public float maxFleeTime = 5f;            // Max time to stay fleeing before stopping or switching states
    public float chaseFleeDuration = 10f;     // Duration to chase in ChaseFlee state
    public float chaseTimerSpeed = 18f;       // Speed during ChaseFlee or ChaseTimer state
    public float chaseFleeTriggerTime = 3f;   // Time player must be close before ChaseFlee triggers

    [Header("Wander Settings")]
    public float wanderCooldown = 3f;         // Time between wander moves
    public float movementConeAngle = 90f;

    [Header("ChaseTimer Settings")]
    public float timeBeforeChaseInMinutes = 10f;   // How long before the enemy starts chasing automatically
    public float chaseTimerDuration = 100f;        // Duration to chase during chase timer mode

    [Header("Audio")]
    public AudioClip roarClip;
    public AudioClip chaseStartClip;
    public float roarCooldown = 20f;         // Cooldown between roars
    public float roarMinDelay = 5f;          // Minimum delay before next roar
    public float roarMaxDelay = 15f;         // Maximum delay before next roar

    [Header("Lurker Settings")]
    public float lurkCooldownMin = 20f;        // Minimum cooldown between lurking attempts
    public float lurkCooldownMax = 40f;        // Maximum cooldown between lurking attempts
    public float lurkDistanceFromPlayer = 20f; // Distance to keep behind player while lurking
    public float lurkDuration = 5f;            // How long to lurk before switching state

    // AI states
    private enum EnemyState { Wandering, Fleeing, ApproachingLurk, Lurking, ChaseFlee, ChaseTimer }
    private EnemyState state = EnemyState.Wandering; // Start in Wandering state

    // Internal timers and state trackers
    private float originalSpeed;
    private float wanderTimer;
    private float fleeTimer;
    private float chaseFleeTimer;
    private float chaseTimerCountdown;
    private float chaseTimerRunTime;
    private float chaseFleeProximityTimer = 0f; // Timer tracking how long player stays close for ChaseFlee trigger

    private float nextRoarTime;
    private float lastRoarTime = -Mathf.Infinity; // Last time roar was played

    private float nextLurkTime = 0f;            // Next time to attempt lurking
    private float lurkStartTime;                // When lurking started
    private Vector3 lurkTargetPosition;         // Position behind player to move to when lurking

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        footstepAudio = GetComponent<AudioSource>();

        originalSpeed = agent.speed;
        chaseTimerCountdown = timeBeforeChaseInMinutes * 60f; // Convert minutes to seconds

        agent.acceleration = 100f;
        agent.angularSpeed = 720f;
        agent.stoppingDistance = 0.5f;

        ScheduleNextRoar();
        ScheduleNextLurk();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);

        // Check if conditions allow triggering ChaseFlee (not already chasing or lurking)
        bool canTriggerChaseFlee =
            state != EnemyState.ChaseFlee &&
            state != EnemyState.ChaseTimer &&
            state != EnemyState.Lurking;

        // Increment timer if player stays within trigger distance
        if (distanceToPlayer < triggerDistance && canTriggerChaseFlee)
        {
            chaseFleeProximityTimer += Time.deltaTime;

            // Trigger ChaseFlee after player stays close for required time
            if (chaseFleeProximityTimer >= chaseFleeTriggerTime)
            {
                state = EnemyState.ChaseFlee;
                agent.speed = chaseTimerSpeed;
                PlayChaseStartSound();

                chaseFleeTimer = 0f;
                chaseFleeProximityTimer = 0f;
                return;
            }
        }
        else
        {
            chaseFleeProximityTimer = 0f;
        }

        HandleChaseTimer();

        // Handle behavior based on current state
        switch (state)
        {
            case EnemyState.Lurking:
                HandleLurking();
                return;

            case EnemyState.ApproachingLurk:
                // Once close enough to lurk position, start lurking
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    state = EnemyState.Lurking;
                    lurkStartTime = Time.time;  // Track when lurking started
                    agent.ResetPath();          // Stop moving
                }
                FacePlayer();
                return;

            case EnemyState.Fleeing:
                HandleFlee(distanceToPlayer);
                return;

            case EnemyState.ChaseFlee:
                HandleChaseFlee(distanceToPlayer);
                return;

            case EnemyState.ChaseTimer:
                HandleChaseTimerRun();
                return;

            case EnemyState.Wandering:
                // If player is too close, switch to fleeing
                if (distanceToPlayer < triggerDistance)
                {
                    state = EnemyState.Fleeing;
                    fleeTimer = 0f;
                    return;
                }

                // Try to start lurking if cooldown expired
                if (Time.time >= nextLurkTime)
                    TryStartLurking();

                wanderTimer += Time.deltaTime;

                // Move to a new wander point periodically or if agent nearly reached destination
                if (wanderTimer >= wanderCooldown || agent.remainingDistance < 1f)
                {
                    Wander();
                    wanderTimer = 0f;
                }

                agent.speed = originalSpeed;
                break;
        }

        HandleFootsteps();
        HandleRoar();
    }

    // Handle fleeing behavior
    void HandleFlee(float distanceToPlayer)
    {
        fleeTimer += Time.deltaTime;

        if (fleeTimer >= maxFleeTime)
        {
            // If player still close after max flee time, reset timer and keep fleeing
            if (distanceToPlayer < triggerDistance)
            {
                fleeTimer = 0f;
                return;
            }
            else
            {
                // Player moved away, return to wandering
                state = EnemyState.Wandering;
                fleeTimer = 0f;
                return;
            }
        }

        // Calculate flee destination opposite the player, within triggerDistance
        Vector3 fleeDir = (transform.position - target.transform.position).normalized;
        Vector3 destination = transform.position + fleeDir * triggerDistance;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, triggerDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = fleeSpeed;
        }
    }

    // Handle ChaseFlee state where enemy chases player aggressively for a limited time
    void HandleChaseFlee(float distanceToPlayer)
    {
        agent.SetDestination(target.transform.position); // Chase player
        chaseFleeTimer += Time.deltaTime;

        if (chaseFleeTimer >= chaseFleeDuration)
        {
            // After chasing, fallback to Fleeing if player still close, else wander
            if (distanceToPlayer < triggerDistance)
            {
                state = EnemyState.Fleeing;
                fleeTimer = 0f;
            }
            else
            {
                state = EnemyState.Wandering;
            }

            chaseFleeTimer = 0f;
            agent.speed = originalSpeed;
        }
    }

    // Countdown before automatic chase timer triggers
    void HandleChaseTimer()
    {
        if (state == EnemyState.ChaseTimer)
            return;

        chaseTimerCountdown -= Time.deltaTime;

        if (chaseTimerCountdown <= 0f)
        {
            state = EnemyState.ChaseTimer;
            agent.speed = chaseTimerSpeed;
            PlayChaseStartSound();
        }
    }

    // During ChaseTimer state, chase the player for chaseTimerDuration seconds
    void HandleChaseTimerRun()
    {
        agent.SetDestination(target.transform.position);
        chaseTimerRunTime += Time.deltaTime;

        if (chaseTimerRunTime >= chaseTimerDuration)
        {
            // Return to wandering after chase ends, reset timers
            state = EnemyState.Wandering;
            chaseTimerRunTime = 0f;
            chaseTimerCountdown = timeBeforeChaseInMinutes * 60f;
            agent.speed = originalSpeed;
        }
    }

    // Attempt to start lurking if conditions met
    void TryStartLurking()
    {
        if (state != EnemyState.Wandering) return; // Only lurk from wandering

        // Calculate position to lurk
        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 lurkPos = target.transform.position - dir * lurkDistanceFromPlayer;

        // Validate lurk position on NavMesh
        if (NavMesh.SamplePosition(lurkPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            lurkTargetPosition = hit.position;
            agent.SetDestination(lurkTargetPosition);
            agent.speed = originalSpeed;
            state = EnemyState.ApproachingLurk; // Start moving to lurk position
        }

        ScheduleNextLurk();
    }

    // Handle behavior while lurking: face player and check angle or duration to stop lurking
    void HandleLurking()
    {
        FacePlayer();

        Vector3 toEnemy = (transform.position - target.transform.position).normalized;
        float angle = Vector3.Angle(target.transform.forward, toEnemy);

        // Stop lurking if player turns around (>90 degrees) or lurk duration exceeded
        if (angle > 90f || Time.time - lurkStartTime > lurkDuration)
        {
            // Move away from player after lurking ends
            Vector3 fleeDir = (transform.position - target.transform.position).normalized;
            Vector3 newPos = transform.position + fleeDir * triggerDistance;

            if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, triggerDistance, NavMesh.AllAreas))
                agent.SetDestination(hit.position);

            state = EnemyState.Fleeing;
            fleeTimer = 0f;
        }
    }

    // Wander by picking a random point in front of the enemy within movementConeAngle
    void Wander()
    {
        float radius = 20f;    // Max wander distance
        int attempts = 10;     // How many tries to find valid point

        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * radius;
            randomDir.y = 0;
            Vector3 candidate = transform.position + randomDir;

            Vector3 dirToCandidate = (candidate - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToCandidate);

            // Check if candidate is inside allowed cone and on NavMesh
            if (angle <= movementConeAngle / 2f &&
                NavMesh.SamplePosition(candidate, out NavMeshHit navHit, radius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                return;
            }
        }
    }

    // Smoothly rotate enemy to face player horizontally
    void FacePlayer()
    {
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 2f);
    }

    // Play footstep sounds if moving, stop if idle
    void HandleFootsteps()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f && agent.remainingDistance > 0.1f;

        if (isMoving && !footstepAudio.isPlaying)
            footstepAudio.Play();
        else if (!isMoving && footstepAudio.isPlaying)
            footstepAudio.Stop();
    }

    // Play roar sound occasionally based on cooldown timers
    void HandleRoar()
    {
        if (Time.time >= nextRoarTime && Time.time - lastRoarTime >= roarCooldown)
        {
            if (roarClip != null)
                footstepAudio.PlayOneShot(roarClip);

            lastRoarTime = Time.time;
            ScheduleNextRoar();
        }
    }

    // Play chase start sound effect
    void PlayChaseStartSound()
    {
        if (chaseStartClip != null)
            footstepAudio.PlayOneShot(chaseStartClip);
    }

    // Schedule next roar randomly between min and max delay
    void ScheduleNextRoar()
    {
        nextRoarTime = Time.time + Random.Range(roarMinDelay, roarMaxDelay);
    }

    // Schedule next lurk attempt randomly between min and max cooldown
    void ScheduleNextLurk()
    {
        nextLurkTime = Time.time + Random.Range(lurkCooldownMin, lurkCooldownMax);
    }

    // Draw gizmos in editor to visualize ranges
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Red sphere shows flee/chase trigger distance around enemy
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);

            // Yellow sphere shows lurk distance radius around player
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.transform.position, lurkDistanceFromPlayer);
        }
    }
}
