using UnityEngine;
using UnityEngine.SceneManagement;

public class TotemCount : MonoBehaviour
{
    [SerializeField] private GameObject[] totems;
    [SerializeField] private int totalTotems = 5; // total totems in the level
    private int collectedTotems = 0;

    [SerializeField] private string sceneName;

    private void Start()
    {
        // Optionally check initial active state
        CountActiveTotems();
    }

    private void Update()
    {
        // Continuously check for collection (simple but reliable)
        CountActiveTotems();
    }

    private void CountActiveTotems()
    {
        int activeCount = 0;

        foreach (GameObject totem in totems)
        {
            if (totem.activeSelf)
                activeCount++;
        }

        collectedTotems = totalTotems - activeCount;

        // When all 5 are collected (none active), change scene
        if (collectedTotems >= totalTotems)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
