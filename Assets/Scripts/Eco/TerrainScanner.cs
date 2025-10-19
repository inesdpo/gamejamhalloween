using UnityEngine;

public class TerrainScanner : MonoBehaviour
{
    [Header("Scanner Settings")]
    public GameObject TerrainScannerPrefab;
    public float duration = 10f;
    public float size = 500f;
    public float pulseInterval = 1f; // seconds between pulses

    private float nextPulseTime = 0f;

    void Update()
    {
        // Detect movement input (WASD)
        bool isMoving =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D);

        // Trigger pulse when moving and cooldown allows
        if (isMoving && Time.time >= nextPulseTime)
        {
            SpawnTerrainScanner();
            nextPulseTime = Time.time + pulseInterval;
        }
    }

    void SpawnTerrainScanner()
    {
        if (TerrainScannerPrefab == null)
        {
            Debug.LogError("TerrainScannerPrefab not assigned!");
            return;
        }

        // Spawn visual scanner
        GameObject terrainScanner = Instantiate(
            TerrainScannerPrefab,
            transform.position,
            Quaternion.identity
        );

        // Spawn invisible detection sphere
        GameObject wave = new GameObject("EcholocationWave");
        wave.transform.position = transform.position;

        var waveScript = wave.AddComponent<EcholocationWave>();
        waveScript.maxRadius = size;      // match your visual pulse size
        waveScript.lifetime = duration;   // match your pulse duration


        // Particle tuning
        ParticleSystem ps = terrainScanner.transform.GetChild(0).GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startLifetime = duration;
            main.startSize = size;
        }

        Destroy(terrainScanner, duration + 1f);
    }

}
