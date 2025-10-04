using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Search, Chase, Attack }

    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] float detectRange = 30f;
    [SerializeField] float attackRange = 12f;

    [SerializeField] float idleSpeed = 3.5f;
    [SerializeField] float chaseSpeed = 10f;
    [SerializeField] float searchSpeed = 4.5f;

    [SerializeField] float searchDuration = 5f;
    [SerializeField] float attackDuration = 0.6f;

    [SerializeField] float attackHoldDistance = 8f;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 25f;
    [SerializeField] float fireCooldown = 0.75f;
    [SerializeField] float spawnOffset = 0.4f;

    State currentState = State.Idle;

    Vector3 lastKnownPosition;
    float searchTimer;
    float attackTimer;

    float sqrDetectRange;
    float sqrAttackRange;

    float nextFireTime;

    public Renderer rend;
    public Color idleColor = Color.green;
    public Color searchColor = Color.yellow;
    public Color chaseColor = new Color(1f, 0.5f, 0f);
    public Color attackColor = Color.red;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!rend) rend = GetComponentInChildren<Renderer>();
        sqrDetectRange = detectRange * detectRange;
        sqrAttackRange = attackRange * attackRange;
        agent.stoppingDistance = attackHoldDistance;
        SetState(State.Idle);
    }

    void Update()
    {
        if (!player) return;

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        bool canDetect = sqrDist <= sqrDetectRange;
        bool inAttack = sqrDist <= sqrAttackRange;

        if (canDetect) lastKnownPosition = player.position;

        switch (currentState)
        {
            case State.Idle:
                agent.speed = idleSpeed;
                agent.isStopped = true;
                if (canDetect) SetState(State.Chase);
                break;

            case State.Search:
                agent.speed = searchSpeed;
                agent.isStopped = false;
                agent.SetDestination(lastKnownPosition);
                searchTimer -= Time.deltaTime;
                if (canDetect) SetState(State.Chase);
                else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) SetState(State.Idle);
                else if (searchTimer <= 0f) SetState(State.Idle);
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.stoppingDistance = attackHoldDistance;
                if (canDetect) agent.SetDestination(player.position);
                else
                {
                    searchTimer = searchDuration;
                    SetState(State.Search);
                    break;
                }
                if (inAttack) SetState(State.Attack);
                break;

            case State.Attack:
                agent.isStopped = false;
                agent.speed = 0f;
                agent.stoppingDistance = attackHoldDistance;
                agent.SetDestination(player.position);
                Vector3 look = player.position;
                look.y = transform.position.y;
                transform.LookAt(look);
                if (Time.time >= nextFireTime) Fire();
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    if (canDetect) SetState(State.Chase);
                    else { searchTimer = searchDuration; SetState(State.Search); }
                }
                break;
        }
    }

    void SetState(State next)
    {
        if (currentState == next) return;
        currentState = next;
        if (next == State.Search) searchTimer = searchDuration;
        if (next == State.Attack)
        {
            attackTimer = attackDuration;
            nextFireTime = Time.time;
        }
        if (next == State.Chase) agent.speed = chaseSpeed;
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

        if (rb != null) rb.linearVelocity = dir * projectileSpeed;

        nextFireTime = Time.time + fireCooldown;
    }

    void ApplyColor(State s)
    {
        if (!rend) return;
        var c = idleColor;
        if (s == State.Search) c = searchColor;
        else if (s == State.Chase) c = chaseColor;
        else if (s == State.Attack) c = attackColor;
        rend.material.color = c;
    }

    void OnValidate()
    {
        sqrDetectRange = detectRange * detectRange;
        sqrAttackRange = attackRange * attackRange;
        if (agent) agent.stoppingDistance = attackHoldDistance;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(lastKnownPosition, 0.3f);
    }
}
