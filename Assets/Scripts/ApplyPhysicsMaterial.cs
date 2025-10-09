using UnityEngine;

public class ApplyPhysicsMaterialToChildren : MonoBehaviour
{
    public PhysicsMaterial materialToApply;

    void Start()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.material = materialToApply;
        }
    }
}
