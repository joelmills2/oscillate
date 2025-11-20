using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance { get; private set; }

    [SerializeField] Transform[] spawnPoints;

    public Transform GetSpawnPoint(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        int index = (int)(clientId % (ulong)spawnPoints.Length);
        return spawnPoints[index];
    }

    void Awake()
    {
        Instance = this;
    }
}