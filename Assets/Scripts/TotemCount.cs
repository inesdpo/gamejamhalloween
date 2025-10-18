using UnityEngine;

public class TotemCount : MonoBehaviour
{
    [SerializeField] private GameObject[] totems;
    [SerializeField] private int totemCount;

    private void Start()
    {
        CountTotems();
    }

    private void CountTotems()
    {
       foreach (GameObject totem in totems)
        {
            if (totem.activeSelf)
            {
                totemCount++;
            }
        }
    }
}
