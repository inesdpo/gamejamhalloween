using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EcholocationWave : MonoBehaviour
{
    public float maxRadius = 500f;
    public float lifetime = 10f;

    private SphereCollider triggerSphere;
    private float elapsed = 0f;

    void Start()
    {
        triggerSphere = GetComponent<SphereCollider>();
        triggerSphere.isTrigger = true;
        triggerSphere.radius = 0f;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Linearly expand collider radius over lifetime
        float t = Mathf.Clamp01(elapsed / lifetime);
        triggerSphere.radius = Mathf.Lerp(0f, maxRadius, t);
    }

    void OnTriggerEnter(Collider other)
    {
        var reactive = other.GetComponent<EcholocationReactive>();
        if (reactive != null)
            reactive.OnPinged();
    }
    void OnDrawGizmos()
    {
        if (triggerSphere != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.25f);
            Gizmos.DrawWireSphere(transform.position, triggerSphere.radius);
        }
    }

}
