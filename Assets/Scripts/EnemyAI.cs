// Sources:
// https://learn.unity.com/tutorial/introduction-to-navmesh-agents
// https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
// https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AI.NavMeshAgent.Raycast.html
// https://discussions.unity.com/t/beginner-question-regarding-ai-view-radius-and-line-of-sight/765569
// https://www.youtube.com/watch?v=znZXmmyBF-o
// https://www.youtube.com/watch?v=UjkSFoLxesw
// https://www.gamedeveloper.com/
// https://learn.unity.com/project/navigation-and-pathfinding
// https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html=

using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Search, Chase, Attack }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent agent;

    [Header("Ranges")]
    [SerializeField] float detectRange = 30f;
    [SerializeField] float attackRange = 12f;
    [SerializeField] float attackHoldDistance = 8f;

    [Header("Speeds")]
    [SerializeField] float searchSpeed = 4.5f;
    [SerializeField] float chaseSpeed = 10f;

    [Header("Timers")]
    [SerializeField] float searchDuration = 6f;
    [SerializeField] float fireCooldown = 0.75f;

    [Header("Search Behavior")]
    [SerializeField] float searchRadius = 15f;
    float searchTimer;

    [Header("Line of Sight")]
    [SerializeField] float viewAngle = 60f;
    [SerializeField] float eyeHeight = 1.6f;
    [SerializeField] LayerMask obstacleMask;

    [Header("Attack Settings")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 25f;
    [SerializeField] float spawnOffset = 0.4f;

    [Header("Visual Debugging")]
    public Renderer rend;
    public Color idleColor = Color.green;
    public Color searchColor = Color.cyan;
    public Color chaseColor = new Color(1f, 0.5f, 0f);
    public Color attackColor = Color.red;

    State currentState = State.Idle;
    float nextFireTime;
    float sqrAttackRange;
    float timeSinceLastSeen;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!rend) rend = GetComponentInChildren<Renderer>();

        sqrAttackRange = attackRange * attackRange;
        agent.stoppingDistance = attackHoldDistance;

        SetState(State.Idle);
    }

    void Update()
    {
        if (!player) return;

        bool visible = CanSeePlayer();

        if (visible)
        {
            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }

        bool canDetect = visible || (timeSinceLastSeen < 2f);
        float sqrDist = (player.position - transform.position).sqrMagnitude;
        bool inAttack = sqrDist <= sqrAttackRange;

        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                if (canDetect) SetState(State.Chase);
                else if (Time.time % 3f < 0.02f) SetState(State.Search);
                break;

            case State.Search:
                agent.speed = searchSpeed;
                agent.isStopped = false;

                if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
                {
                    Vector3 roamPoint = GetRandomSearchPoint();
                    agent.SetDestination(roamPoint);
                    searchTimer = searchDuration;
                }

                searchTimer -= Time.deltaTime;

                if (visible)
                {
                    SetState(State.Chase);
                }
                else if (searchTimer <= 0f)
                {
                    SetState(State.Idle);
                }
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;

                if (visible)
                {
                    agent.SetDestination(player.position);
                }
                else
                {
                    SetState(State.Search);
                    break;
                }

                if (inAttack) SetState(State.Attack);
                break;

            case State.Attack:
                agent.isStopped = false;
                agent.speed = 0f;
                agent.SetDestination(player.position);

                Vector3 look = player.position;
                look.y = transform.position.y;
                transform.LookAt(look);

                if (Time.time >= nextFireTime) Fire();

                if (!visible)
                {
                    searchTimer = searchDuration;
                    SetState(State.Search);
                }
                else if (!inAttack)
                {
                    SetState(State.Chase);
                }
                break;
        }
    }

    void SetState(State next)
    {
        if (currentState == next) return;
        currentState = next;

        if (next == State.Search) searchTimer = searchDuration;
        if (next == State.Attack) nextFireTime = Time.time;
        if (next == State.Idle) agent.ResetPath();

        ApplyColor(next);
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

        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }

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
        if (Vector3.Angle(transform.forward, dir) > viewAngle / 2f) return false;
        if (Physics.Raycast(origin, dir, dist, obstacleMask)) return false;

        return true;
    }

    Vector3 GetRandomSearchPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * searchRadius;
        randomDir += transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    void ApplyColor(State s)
    {
        if (!rend) return;
        Color c = idleColor;
        if (s == State.Search) c = searchColor;
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
