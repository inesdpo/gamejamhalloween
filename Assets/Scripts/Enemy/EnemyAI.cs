using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    // Reference to the NavMeshAgent that controls enemy movement
    public NavMeshAgent ai;

    // List of random patrol destinations the enemy can walk to
    public List<Transform> destinations;

    // Animator used to control enemy animations
    public Animator aiAnim;

    // Movement speeds and time settings
    public float walkSpeed, chaseSpeed;
    public float minIdleTime, maxIdleTime; // Random idle duration range
    public float idleTime;                 // Chosen idle duration
    public float sightDistance;            // How far the enemy can "see" the player
    public float catchDistance;            // Distance needed to catch the player
    public float chaseTime;                // How long the chase lasts
    public float minChaseTime, maxChaseTime;
    public float jumscareTime;             // Delay before loading death scene after jumpscare

    // State booleans
    public bool walking, chasing;

    // Reference to the player transform
    public Transform player;

    // Internal tracking of destinations
    private Transform currentDest;
    private Vector3 dest;
    private int randNum, randNum2;
    public int destinationAmount;

    // Raycast offset to make the enemy's vision start from a specific point (e.g., eyes)
    public Vector3 raycastOffet;

    // Name of the scene to load when the player dies
    public string deathScene;

    void Start()
    {
        // Start in walking mode and pick a random destination
        walking = true;
        randNum = Random.Range(0, destinationAmount);
        currentDest = destinations[randNum];
    }

    void Update()
    {
        // Calculate direction from enemy to player
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;

        // Cast a ray toward the player to check if the enemy sees them
        if (Physics.Raycast(transform.position + raycastOffet, direction, out hit, sightDistance))
        {
            // If the ray hits the player, start chasing
            if (hit.collider.gameObject.tag == "Player")
            {
                walking = false;

                // Stop other routines to avoid conflicts
                StopCoroutine("stayIdle");
                StopCoroutine("chaseRoutine");

                // Start chasing behavior
                StartCoroutine("chaseRoutine");

                // Change animation to sprint
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.SetTrigger("sprint");

                chasing = true;
            }
        }

        // --- CHASING BEHAVIOR ---
        if (chasing == true)
        {
            // Move toward the player's current position
            dest = player.position;
            ai.destination = dest;
            ai.speed = chaseSpeed;

            // If close enough, catch the player
            if (ai.remainingDistance <= catchDistance)
            {
                // Disable player and play jumpscare animation
                player.gameObject.SetActive(false);
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");

                // Start death sequence
                StartCoroutine(deathRoutine());
                chasing = false;
            }
        }

        // --- WALKING / PATROLLING BEHAVIOR ---
        if (walking)
        {
            // Move toward current patrol destination
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;

            // If reached destination...
            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                // Randomly choose between walking again or idling
                randNum2 = Random.Range(0, 2);

                if (randNum2 == 0)
                {
                    // Pick a new random destination and continue walking
                    randNum = Random.Range(0, destinationAmount);
                    currentDest = destinations[randNum];
                }

                if (randNum2 == 1)
                {
                    // Stop and idle for a while
                    aiAnim.ResetTrigger("walk");
                    aiAnim.SetTrigger("idle");
                    ai.speed = 0;

                    StartCoroutine("stayIdle");
                    walking = false;
                }
            }
        }

        // --- COROUTINES ---
        IEnumerator StayIdle()
        {
            // Wait a random amount of time before walking again
            idleTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(idleTime);

            walking = true;
            randNum = Random.Range(0, destinationAmount);
            currentDest = destinations[randNum];

            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("walk");
        }

        IEnumerator chaseRoutine()
        {
            // Chase for a random duration
            chaseTime = Random.Range(minChaseTime, maxChaseTime);
            yield return new WaitForSeconds(chaseTime);

            // Return to patrol after chase ends
            walking = true;
            chasing = false;
            randNum = Random.Range(0, destinationAmount);
            currentDest = destinations[randNum];

            aiAnim.ResetTrigger("sprint");
            aiAnim.SetTrigger("walk");
        }

        IEnumerator deathRoutine()
        {
            // Wait for jumpscare animation, then load death scene
            yield return new WaitForSeconds(jumscareTime);
            SceneManager.LoadScene(deathScene);
        }
    }
}
