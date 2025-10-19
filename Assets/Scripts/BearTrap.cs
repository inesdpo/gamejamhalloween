using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BearTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private float slowMultiplier = 0.4f;   // Player speed reduction factor
    [SerializeField] private float damagePerSecond = 10f;   // Damage applied per second
    [SerializeField] private float trapDuration = 2f;       // How long the player stays slowed
    [SerializeField] private bool destroyAfterUse = false;  // Optional: should the trap disappear?

    private bool playerTrapped = false;
    private FirstPersonController playerController;
    private PlayerHealth playerHealth;
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    private float trapTimer;

    private void Start()
    {
        // Make sure the trap collider works as a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerTrapped) return;

        playerController = other.GetComponent<FirstPersonController>();
        playerHealth = other.GetComponent<PlayerHealth>();

        if (playerController != null && playerHealth != null)
        {
            // Store original speeds
            originalWalkSpeed = playerController.walkSpeed;
            originalSprintSpeed = playerController.sprintSpeed;

            // Apply slowdown
            playerController.walkSpeed *= slowMultiplier;
            playerController.sprintSpeed *= slowMultiplier;

            playerTrapped = true;
            trapTimer = trapDuration;

            // Optional: play sound or animation here
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!playerTrapped || playerHealth == null) return;

        // Apply continuous damage while in trap
        playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    private void Update()
    {
        if (playerTrapped && playerController != null)
        {
            trapTimer -= Time.deltaTime;

            // When trap duration ends, restore movement
            if (trapTimer <= 0f)
            {
                playerController.walkSpeed = originalWalkSpeed;
                playerController.sprintSpeed = originalSprintSpeed;
                playerTrapped = false;

                if (destroyAfterUse)
                    Destroy(gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Restore movement immediately if player exits early
        if (playerTrapped && other.GetComponent<FirstPersonController>() == playerController)
        {
            playerController.walkSpeed = originalWalkSpeed;
            playerController.sprintSpeed = originalSprintSpeed;
            playerTrapped = false;
        }
    }
}
