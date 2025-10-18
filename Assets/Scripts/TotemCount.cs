using UnityEngine;

public class TotemCount : MonoBehaviour
{
    [SerializeField] private GameObject[] totems;
    [SerializeField] private float totemCount;

    private void Update()
    {
       foreach (GameObject totem in totems)
        {
            totem.SetActive(false);
            totemCount++;
        }
    }
}
