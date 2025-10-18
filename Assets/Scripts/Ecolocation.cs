using UnityEngine;

public class TerrainScanner : MonoBehaviour
{
    public GameObject TerrainScannerPrefab;
    public float duration = 10f;
    public float size = 500f;
    public float pulseInterval = 1f; // 1 second between pulses

    private float nextPulseTime = 0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.E) && Time.time >= nextPulseTime)
        {
            SpawnTerrainScanner();
            nextPulseTime = Time.time + pulseInterval;
        }
    }

    void SpawnTerrainScanner()
    {
        GameObject terrainScanner = Instantiate(TerrainScannerPrefab, transform.position, Quaternion.identity);
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

        Destroy(terrainScanner, duration + 1f);
    }
}
