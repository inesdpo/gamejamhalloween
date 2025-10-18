using UnityEngine;

public class Totem : MonoBehaviour
{
    [SerializeField] private GameObject totem;


    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyUp(KeyCode.T))
        {
            totem.SetActive(false);
        }
    }
}
