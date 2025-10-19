using UnityEngine;

public class EcholocationReactive : MonoBehaviour
{
    public Light revealLight; // Optional light source
    public Renderer objectRenderer;
    public Color emissionColor = Color.cyan;
    public float glowDuration = 2f;

    private Material mat;
    private bool isGlowing = false;

    void Start()
    {
        if (objectRenderer == null) objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            mat = objectRenderer.material;
            mat.DisableKeyword("_EMISSION");
        }
        if (revealLight != null)
            revealLight.enabled = false;
    }

    public void OnPinged()
    {
        if (!isGlowing) StartCoroutine(Glow());
    }

    private System.Collections.IEnumerator Glow()
    {
        isGlowing = true;

        // Turn on glow/light
        if (mat != null)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor);
        }
        if (revealLight != null)
            revealLight.enabled = true;

        yield return new WaitForSeconds(glowDuration);

        // Turn off
        if (mat != null)
            mat.DisableKeyword("_EMISSION");
        if (revealLight != null)
            revealLight.enabled = false;

        isGlowing = false;
    }
}
