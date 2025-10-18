using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damageAmount = 10f;

    private void OnTriggerEnter(Collider other)
    {
        //PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        //if (player != null)
        //{
        //    player.TakeDamage(damageAmount);
        //}
    }
}
