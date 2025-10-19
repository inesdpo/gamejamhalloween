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

        // Spawn scanner at player position
        GameObject terrainScanner = Instantiate(
            TerrainScannerPrefab,
            transform.position,
            Quaternion.identity
        );

        // Access the particle system on the prefab
        ParticleSystem terrainScannerPS = terrainScanner.transform.GetChild(0).GetComponent<ParticleSystem>();
        if (terrainScannerPS != null)
        {
            var main = terrainScannerPS.main;
            main.startLifetime = duration;
            main.startSize = size;
        }
        else
        {
            Debug.LogError("Particle System not found on Terrain Scanner Prefab.");
        }

        // Cleanup
        Destroy(terrainScanner, duration + 1f);
    }
}
