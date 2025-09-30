using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Search, Chase, Attack }

    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private float detectRange = 15f;
    [SerializeField] private float attackRange = 2.5f;

    [SerializeField] private float idleSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 10f;
    [SerializeField] private float searchSpeed = 4.5f;

    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private float attackDuration = 0.6f;

    [SerializeField] private float stopDistanceFromPlayer = 1.2f;

    private State currentState = State.Idle;

    private Vector3 lastKnownPosition;
    private float searchTimer;
    private float attackTimer;

    private float sqrDetectRange;
    private float sqrAttackRange;

    public Renderer rend;
    public Color idleColor = Color.green;
    public Color searchColor = Color.yellow;
    public Color chaseColor = Color.orange;
    public Color attackColor = Color.red;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!rend) rend = GetComponentInChildren<Renderer>();
        sqrDetectRange = detectRange * detectRange;
        sqrAttackRange = attackRange * attackRange;
        agent.stoppingDistance = stopDistanceFromPlayer;
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
                else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) 
                    SetState(State.Idle);
                else if (searchTimer <= 0f) 
                    SetState(State.Idle);
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
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
                agent.isStopped = true;
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    if (canDetect) SetState(State.Chase);
                    else
                    {
                        searchTimer = searchDuration;
                        SetState(State.Search);
                    }
                }
                break;
        }
    }

    void SetState(State next)
    {
        if (currentState == next) return;
        currentState = next;
        if (next == State.Search) searchTimer = searchDuration;
        if (next == State.Attack) attackTimer = attackDuration;
        if (next == State.Chase) agent.speed = chaseSpeed;
        if (next == State.Idle) agent.ResetPath();
        ApplyColor(next);
        Debug.Log($"Enemy state -> {next}");
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
        if (agent) agent.stoppingDistance = stopDistanceFromPlayer;
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
