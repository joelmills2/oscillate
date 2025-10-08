using UnityEngine;

public class SetLayerForChildrenOnce : MonoBehaviour
{
    [SerializeField] string layerName = "Default";

    void Start()
    {
        int layer = LayerMask.NameToLayer(layerName);

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }

        Debug.Log($"Set all children of {name} to layer '{layerName}'");

        // Optional: remove this script after it runs once
        Destroy(this);
    }
}
