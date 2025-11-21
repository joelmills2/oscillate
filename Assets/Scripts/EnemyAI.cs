// Sources:
// https://learn.unity.com/tutorial/introduction-to-navmesh-agents
// https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
// https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AI.NavMeshAgent.Raycast.html
// https://discussions.unity.com/t/beginner-question-regarding-ai-view-radius-and-line-of-sight/765569
// https://www.youtube.com/watch?v=znZXmmyBF-o
// https://www.youtube.com/watch?v=UjkSFoLxesw
// https://www.gamedeveloper.com/
// https://learn.unity.com/project/navigation-and-pathfinding
// https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html
// https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AI.NavMesh.html

using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack }

    [Header("References")]
    [SerializeField] Transform[] playerCandidates;
    [SerializeField] NavMeshAgent agent;

    [Header("Ranges")]
    [SerializeField] float detectRange = 40f;
    [SerializeField] float attackRange = 12f;
    [SerializeField] float attackHoldDistance = 8f;

    [Header("Speeds")]
    [SerializeField] float patrolSpeed = 10f;
    [SerializeField] float chaseSpeed = 8.5f;

    [Header("Timers")]
    [SerializeField] float fireCooldown = 0.75f;
    [SerializeField] float stayInChaseTime = 1f;
    [SerializeField] Vector2 idleAtWaypointSeconds = new Vector2(1f, 4f);

    [Header("Patrol Points")]
    [SerializeField] Transform[] patrolPoints;

    [Header("Line of Sight")]
    [SerializeField] float viewAngle = 120f;
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] LayerMask obstacleMask;

    [Header("Look Behaviour")]
    [SerializeField] float lookYawAmplitude = 45f;
    [SerializeField] float lookYawSpeed = 0.6f;
    [SerializeField] float rotateSpeedDeg = 360f;

    [Header("Attack Settings")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 25f;
    [SerializeField] float spawnOffset = 0.4f;

    [Header("Pathfinding")]
    [SerializeField] float sampleMaxDistance = 3f;
    [SerializeField] float repathInterval = 0.25f;
    [SerializeField] float repathMoveThreshold = 0.75f;

    [Header("Visuals")]
    [SerializeField] Material idlePatrolMat;
    [SerializeField] Material chaseMat;
    [SerializeField] Material attackMat;

    State currentState = State.Patrol;
    float nextFireTime;
    float sqrAttackRange;
    float timeSinceLastSeen;
    float idleTimer;
    int currentPatrolIndex = -1;
    float lookPhase;
    Vector3 lastMoveDir = Vector3.forward;

    NavMeshPath pathCache;
    Vector3 lastSampledTarget;
    float lastRepathTime;
    Vector3 currentGoal;

    SkinnedMeshRenderer[] ghostMeshes;

    Transform currentTarget;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        sqrAttackRange = attackRange * attackRange;
        agent.stoppingDistance = attackHoldDistance;
        agent.autoRepath = true;
        pathCache = new NavMeshPath();
        ghostMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        SetState(State.Patrol);
    }

    void Update()
    {

        if (!IsServer) return;

        currentTarget = SelectBestTarget();

        if (!currentTarget)
        {
            if (currentState != State.Patrol && currentState != State.Idle)
                SetState(State.Patrol);

            agent.isStopped = false;
            agent.speed = patrolSpeed;
            if (!agent.hasPath)
            {
                currentGoal = GetNextPatrolDestination();
                SmartPathToNextPatrolPoint(currentGoal);
            }
            UpdateLook(true);

            Vector3 pvel = agent.desiredVelocity; pvel.y = 0f;
            if (pvel.sqrMagnitude > 0.01f) lastMoveDir = pvel.normalized;
            return;
        }

        bool visible = CanSeeTarget(currentTarget);
        if (visible) timeSinceLastSeen = 0f;
        else timeSinceLastSeen += Time.deltaTime;

        float sqrDist = (currentTarget.position - transform.position).sqrMagnitude;
        bool inAttack = sqrDist <= sqrAttackRange;

        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                idleTimer -= Time.deltaTime;
                if (visible) SetState(State.Chase);
                else if (idleTimer <= 0f)
                {
                    currentGoal = GetNextPatrolDestination();
                    SmartPathToNextPatrolPoint(currentGoal);
                    SetState(State.Patrol);
                }
                UpdateLook(true);
                break;

            case State.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                if (!agent.hasPath)
                {
                    currentGoal = GetNextPatrolDestination();
                    SmartPathToNextPatrolPoint(currentGoal);
                }
                if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance)
                {
                    SetState(State.Idle);
                }
                UpdateLook(true);
                if (visible) SetState(State.Chase);
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                Vector3 tgt = currentTarget.position;
                if (RequireNewPath(tgt)) SmartPathToNextPatrolPoint(tgt);
                if (!visible && timeSinceLastSeen >= stayInChaseTime) SetState(State.Patrol);
                if (inAttack) SetState(State.Attack);
                break;
            case State.Attack:
            {
                float dist = Mathf.Sqrt(sqrDist);

                if (dist < attackHoldDistance)
                {
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;

                    Vector3 toTarget = currentTarget.position - transform.position;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude < 0.001f)
                        toTarget = transform.forward;

                    Vector3 awayDir = -toTarget.normalized;
                    agent.Move(awayDir * chaseSpeed * Time.deltaTime);
                }
                else
                {
                    agent.isStopped = true;
                    agent.speed = 0f;
                    agent.ResetPath();
                }

                Vector3 look = currentTarget.position;
                look.y = transform.position.y;
                transform.LookAt(look);

                if (Time.time >= nextFireTime)
                    Fire();

                if (!inAttack)
                    SetState(State.Chase);

                break;
            }
        }

        Vector3 vel = agent.desiredVelocity; vel.y = 0f;
        if (vel.sqrMagnitude > 0.01f) lastMoveDir = vel.normalized;
    }

    void SetState(State next)
    {
        if (currentState == next) return;
        currentState = next;

        if (next == State.Idle)
        {
            agent.updateRotation = false;
            agent.ResetPath();
            idleTimer = Random.Range(idleAtWaypointSeconds.x, idleAtWaypointSeconds.y);
        }
        else if (next == State.Patrol)
        {
            agent.updateRotation = false;
            timeSinceLastSeen = 0f;
        }
        else if (next == State.Chase)
        {
            agent.updateRotation = true;
        }
        else if (next == State.Attack)
        {
            agent.updateRotation = false;
            nextFireTime = Time.time;
        }

        ApplyMaterials(next);

        if (IsServer && IsSpawned)
        {
            ApplyMaterialsClientRpc(next);
        }
    }

    void ApplyMaterials(State s)
    {
        Material m = idlePatrolMat;
        if (s == State.Chase) m = chaseMat;
        else if (s == State.Attack) m = attackMat;
        if (ghostMeshes == null) return;
        for (int i = 0; i < ghostMeshes.Length; i++) ghostMeshes[i].material = m;
    }

    bool RequireNewPath(Vector3 worldTarget)
    {
        if (Time.time - lastRepathTime < repathInterval) return false;
        if (!NavMesh.SamplePosition(worldTarget, out var hit, sampleMaxDistance, NavMesh.AllAreas)) return false;
        if ((hit.position - lastSampledTarget).sqrMagnitude < repathMoveThreshold * repathMoveThreshold) return false;
        return true;
    }

    bool SmartPathToNextPatrolPoint(Vector3 worldTarget)
    {
        if (!NavMesh.SamplePosition(worldTarget, out var hit, sampleMaxDistance, NavMesh.AllAreas)) return false;
        if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, pathCache)) return false;
        if (pathCache.status != NavMeshPathStatus.PathComplete) return false;
        agent.SetPath(pathCache);
        lastSampledTarget = hit.position;
        lastRepathTime = Time.time;
        return true;
    }

    void UpdateLook(bool oscillate)
    {
        Vector3 baseDir = lastMoveDir.sqrMagnitude > 0.01f ? lastMoveDir : transform.forward;
        if (oscillate)
        {
            lookPhase += Time.deltaTime * lookYawSpeed;
            float offset = Mathf.Sin(lookPhase) * lookYawAmplitude;
            baseDir = Quaternion.AngleAxis(offset, Vector3.up) * baseDir;
        }
        if (baseDir.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(baseDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeedDeg * Time.deltaTime);
    }

    Vector3 GetNextPatrolDestination()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return transform.position;
        if (patrolPoints.Length == 1) return SafeSample(patrolPoints[0].position);
        int nextIndex;
        do { nextIndex = Random.Range(0, patrolPoints.Length); }
        while (nextIndex == currentPatrolIndex);
        currentPatrolIndex = nextIndex;
        return SafeSample(patrolPoints[currentPatrolIndex].position);
    }

    Vector3 SafeSample(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out var hit, sampleMaxDistance, NavMesh.AllAreas)) return hit.position;
        return target;
    }

    void Fire()
    {
        if (!IsServer) return;
        if (!projectilePrefab || !firePoint || !currentTarget) return;

        Vector3 dir = (currentTarget.position - firePoint.position).normalized;
        Vector3 spawnPos = firePoint.position + dir * spawnOffset;

        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        var netObj = proj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

        var rb = proj.GetComponent<Rigidbody>();
        var p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            var myCols = GetComponentsInChildren<Collider>();
            p.Init(myCols);
        }
        if (rb != null) rb.linearVelocity = dir * projectileSpeed;
        nextFireTime = Time.time + fireCooldown;
    }

    bool CanSeeTarget(Transform targetTransform)
    {
        if (!targetTransform) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = targetTransform.position;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

        if (dist > detectRange) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;
        if (Physics.Raycast(origin, dir, dist, obstacleMask)) return false;

        return true;
    }

    void OnValidate()
    {
        sqrAttackRange = attackRange * attackRange;
        if (agent) agent.stoppingDistance = attackHoldDistance;
    }

    void OnDrawGizmos()
    {
        if (!agent || agent.path == null) return;
        if (agent.path.corners.Length < 2) return;
        Color pathColor = Color.white;
        switch (currentState)
        {
            case State.Patrol: pathColor = Color.cyan; break;
            case State.Chase: pathColor = new Color(1f, 0.5f, 0f); break;
            case State.Attack: pathColor = Color.red; break;
            case State.Idle: pathColor = Color.green; break;
        }
        Gizmos.color = pathColor;
        Vector3[] corners = agent.path.corners;
        for (int i = 0; i < corners.Length - 1; i++) Gizmos.DrawLine(corners[i], corners[i + 1]);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(agent.destination, 0.25f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, agent.destination);
    }

    Transform SelectBestTarget()
    {
        Transform best = null;
        float bestSqrDist = float.PositiveInfinity;

        foreach (var np in NetworkPlayer.ServerPlayers)
        {
            if (np == null || !np.IsSpawned)
                continue;

            Transform t = np.transform;
            float sqrDist = (t.position - transform.position).sqrMagnitude;

            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                best = t;
            }
        }

        if (best == null && playerCandidates != null && playerCandidates.Length > 0)
        {
            for (int i = 0; i < playerCandidates.Length; i++)
            {
                Transform t = playerCandidates[i];
                if (!t) continue;

                float sqrDist = (t.position - transform.position).sqrMagnitude;
                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    best = t;
                }
            }
        }
        return best;
    }

    [ClientRpc]
    void ApplyMaterialsClientRpc(State s)
    {
        if (IsServer) return;
        ApplyMaterials(s);
    }
}