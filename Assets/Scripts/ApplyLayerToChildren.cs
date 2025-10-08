using UnityEngine;

public class ApplyLayerToChildren : MonoBehaviour
{
    [SerializeField] string layerName = "Objects";

    void Start()
    {
        int layer = LayerMask.NameToLayer(layerName);

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }
}
