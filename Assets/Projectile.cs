using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float lifeSeconds = 5f;
    Collider selfCol;

    void Awake()
    {
        selfCol = GetComponent<Collider>();
    }

    public void Init(Collider[] ignoreThese)
    {
        if (selfCol == null) selfCol = GetComponent<Collider>();
        for (int i = 0; i < ignoreThese.Length; i++)
        {
            if (ignoreThese[i] && selfCol) Physics.IgnoreCollision(selfCol, ignoreThese[i], true);
        }
        Destroy(gameObject, lifeSeconds);
    }

    void OnCollisionEnter(Collision c)
    {
        Destroy(gameObject);
    }
}
