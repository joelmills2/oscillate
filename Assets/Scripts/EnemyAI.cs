using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack }
    enum SequencePhase { None, Forward, Backward }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent agent;

    [Header("Ranges")]
    [SerializeField] float detectRange = 30f;
    [SerializeField] float attackRange = 12f;
    [SerializeField] float attackHoldDistance = 8f;

    [Header("Speeds")]
    [SerializeField] float patrolSpeed = 4.5f;
    [SerializeField] float chaseSpeed = 10f;

    [Header("Timers")]
    [SerializeField] float fireCooldown = 0.75f;
    [SerializeField] float stayInChaseTime = 1f;
    [SerializeField] float unseenIdleMin = 7f;
    [SerializeField] float unseenIdleMax = 10f;

    [Header("Patrol Points")]
    [SerializeField] Transform[] patrolPoints;

    [Header("Line of Sight")]
    [SerializeField] float viewAngle = 60f;
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

    [Header("Sequence Trigger")]
    [SerializeField] string sequenceTriggerName = "PWcave1";
    [SerializeField] Transform[] sequenceOrder;

    [Header("Visual Debugging")]
    public Renderer rend;
    public Color idleColor = Color.green;
    public Color patrolColor = Color.cyan;
    public Color chaseColor = new Color(1f, 0.5f, 0f);
    public Color attackColor = Color.red;

    State currentState = State.Idle;
    SequencePhase seqPhase = SequencePhase.None;

    float nextFireTime;
    float sqrAttackRange;
    float timeSinceLastSeen;
    float idleTimer;
    float currentUnseenThreshold;
    int currentPatrolIndex = -1;
    float lookPhase;
    Vector3 lastMoveDir = Vector3.forward;

    NavMeshPath pathCache;
    Vector3 lastSampledTarget;
    float lastRepathTime;
    float repathInterval = 0.2f;
    float repathMoveThreshold = 0.5f;

    float lastStuckCheck;
    float stuckCheckInterval = 0.4f;
    float lastRemainingDist;
    Vector3 currentGoal;

    readonly Queue<Transform> sequenceQueue = new Queue<Transform>();
    Transform lastAssignedGoalTransform;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!rend) rend = GetComponentInChildren<Renderer>();
        sqrAttackRange = attackRange * attackRange;
        agent.stoppingDistance = attackHoldDistance;
        agent.autoRepath = true;
        pathCache = new NavMeshPath();
        currentUnseenThreshold = Random.Range(unseenIdleMin, unseenIdleMax);
        SetState(State.Idle);
    }

    void Update()
    {
        if (!player) return;

        bool visible = CanSeePlayer();
        if (visible) timeSinceLastSeen = 0f; else timeSinceLastSeen += Time.deltaTime;
        if (!visible && timeSinceLastSeen >= currentUnseenThreshold && currentState != State.Idle) SetState(State.Idle);

        bool canDetect = visible || (timeSinceLastSeen < 2f);
        float sqrDist = (player.position - transform.position).sqrMagnitude;
        bool inAttack = sqrDist <= sqrAttackRange;

        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                idleTimer -= Time.deltaTime;
                if (canDetect) SetState(State.Chase);
                else if (idleTimer <= 0f) SetState(State.Patrol);
                UpdateLook(true);
                break;

            case State.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
                {
                    OnReached(lastAssignedGoalTransform);
                    currentGoal = GetNextPatrolDestination(out lastAssignedGoalTransform);
                    TrySetPathTo(currentGoal);
                }
                UpdateLook(true);
                if (visible) SetState(State.Chase);
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                Vector3 tgt = player.position;
                if (NeedRepath(tgt)) TrySetPathTo(tgt);
                if (!visible && timeSinceLastSeen >= stayInChaseTime) SetState(State.Patrol);
                if (inAttack) SetState(State.Attack);
                break;

            case State.Attack:
                agent.isStopped = false;
                agent.speed = 0f;
                agent.SetDestination(player.position);
                Vector3 look = player.position; look.y = transform.position.y;
                transform.LookAt(look);
                if (Time.time >= nextFireTime) Fire();
                if (!visible) SetState(State.Patrol);
                else if (!inAttack) SetState(State.Chase);
                break;
        }

        Vector3 vel = agent.desiredVelocity; vel.y = 0f;
        if (vel.sqrMagnitude > 0.01f) lastMoveDir = vel.normalized;

        if (currentState == State.Patrol || currentState == State.Chase) DetectAndRecoverStuck();
    }

    void SetState(State next)
    {
        if (currentState == next) return;
        currentState = next;

        if (next == State.Idle)
        {
            agent.updateRotation = false;
            agent.ResetPath();
            idleTimer = Random.Range(1f, 3f);
        }
        else if (next == State.Patrol)
        {
            agent.updateRotation = false;
            timeSinceLastSeen = 0f;
            currentUnseenThreshold = Random.Range(unseenIdleMin, unseenIdleMax);
            lastRemainingDist = 0f;
        }
        else if (next == State.Chase)
        {
            agent.updateRotation = true;
            lastRemainingDist = 0f;
        }
        else if (next == State.Attack)
        {
            agent.updateRotation = false;
            nextFireTime = Time.time;
        }

        ApplyColor(next);
    }

    void OnReached(Transform reached)
    {
        if (!reached) return;

        if (seqPhase == SequencePhase.None && reached.name == sequenceTriggerName)
        {
            sequenceQueue.Clear();
            if (sequenceOrder != null)
            {
                for (int i = 0; i < sequenceOrder.Length; i++)
                    if (sequenceOrder[i] && sequenceOrder[i] != reached) sequenceQueue.Enqueue(sequenceOrder[i]);
                if (sequenceQueue.Count > 0) seqPhase = SequencePhase.Forward;
            }
            return;
        }

        if (seqPhase == SequencePhase.Forward && sequenceQueue.Count == 0)
        {
            sequenceQueue.Clear();
            if (sequenceOrder != null && sequenceOrder.Length > 0)
            {
                for (int i = sequenceOrder.Length - 1; i >= 0; i--)
                    if (sequenceOrder[i]) sequenceQueue.Enqueue(sequenceOrder[i]);
                Transform trigger = FindTriggerTransform();
                if (trigger) sequenceQueue.Enqueue(trigger);
                seqPhase = SequencePhase.Backward;
            }
            return;
        }

        if (seqPhase == SequencePhase.Backward && sequenceQueue.Count == 0 && reached.name == sequenceTriggerName)
        {
            seqPhase = SequencePhase.None;
        }
    }

    Transform FindTriggerTransform()
    {
        if (patrolPoints == null) return null;
        for (int i = 0; i < patrolPoints.Length; i++)
            if (patrolPoints[i] && patrolPoints[i].name == sequenceTriggerName) return patrolPoints[i];
        return null;
    }

    bool NeedRepath(Vector3 worldTarget)
    {
        if (Time.time - lastRepathTime < repathInterval) return false;
        if (!NavMesh.SamplePosition(worldTarget, out var hit, 2f, NavMesh.AllAreas)) return false;
        if ((hit.position - lastSampledTarget).sqrMagnitude < repathMoveThreshold * repathMoveThreshold) return false;
        return true;
    }

    bool TrySetPathTo(Vector3 worldTarget)
    {
        if (!NavMesh.SamplePosition(worldTarget, out var hit, 3f, NavMesh.AllAreas)) return false;
        bool ok = NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, pathCache);
        if (!ok || pathCache.status != NavMeshPathStatus.PathComplete) return false;
        agent.SetPath(pathCache);
        lastSampledTarget = hit.position;
        lastRepathTime = Time.time;
        return true;
    }

    void DetectAndRecoverStuck()
    {
        if (Time.time - lastStuckCheck < stuckCheckInterval) return;
        lastStuckCheck = Time.time;
        if (agent.pathPending) return;
        if (!agent.hasPath) return;
        if (agent.remainingDistance <= agent.stoppingDistance) return;
        float rem = agent.remainingDistance;
        float progress = lastRemainingDist <= 0f ? 1f : lastRemainingDist - rem;
        lastRemainingDist = rem;
        bool barelyMoving = agent.velocity.sqrMagnitude < 0.01f && agent.desiredVelocity.sqrMagnitude < 0.01f;
        bool noProgress = progress < 0.01f;
        if (barelyMoving || noProgress)
        {
            Vector3 nudge = transform.position + transform.right * 1.5f;
            if (!TrySetPathTo(nudge))
            {
                nudge = transform.position - transform.right * 1.5f;
                TrySetPathTo(nudge);
            }
        }
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

    Vector3 GetNextPatrolDestination(out Transform goalTransform)
    {
        if (sequenceQueue.Count > 0)
        {
            goalTransform = sequenceQueue.Dequeue();
            return SafeSample(goalTransform.position);
        }

        goalTransform = null;
        if (patrolPoints == null || patrolPoints.Length == 0) return transform.position;

        if (patrolPoints.Length == 1)
        {
            currentPatrolIndex = 0;
            goalTransform = patrolPoints[0];
            return SafeSample(goalTransform.position);
        }

        int nextIndex;
        do { nextIndex = Random.Range(0, patrolPoints.Length); }
        while (nextIndex == currentPatrolIndex);

        currentPatrolIndex = nextIndex;
        goalTransform = patrolPoints[currentPatrolIndex];
        return SafeSample(goalTransform.position);
    }

    Vector3 SafeSample(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas)) return hit.position;
        return target;
    }

    void Fire()
    {
        if (!projectilePrefab || !firePoint || !player) return;
        Vector3 dir = (player.position - firePoint.position).normalized;
        Vector3 spawnPos = firePoint.position + dir * spawnOffset;
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
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

    bool CanSeePlayer()
    {
        if (!player) return false;
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);
        if (dist > detectRange) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;
        if (Physics.Raycast(origin, dir, dist, obstacleMask)) return false;
        return true;
    }

    void ApplyColor(State s)
    {
        if (!rend) return;
        Color c = idleColor;
        if (s == State.Patrol) c = patrolColor;
        else if (s == State.Chase) c = chaseColor;
        else if (s == State.Attack) c = attackColor;
        rend.material.color = c;
    }

    void OnValidate()
    {
        sqrAttackRange = attackRange * attackRange;
        if (agent) agent.stoppingDistance = attackHoldDistance;
    }
}
