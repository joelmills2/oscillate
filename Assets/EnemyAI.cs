using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Search, Chase, Attack }

    public Transform player;
    public NavMeshAgent agent;

    public float detectRange = 15f;
    public float attackRange = 2.5f;

    public float idleSpeed = 3.5f;
    public float chaseSpeed = 10f;
    public float searchSpeed = 4.5f;

    public float searchDuration = 5f;
    public float attackDuration = 0.6f;

    public float stopDistanceFromPlayer = 1.2f;

    State currentState = State.Idle;

    Vector3 lastKnownPosition;
    float searchTimer;
    float attackTimer;

    float sqrDetectRange;
    float sqrAttackRange;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
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
        Debug.Log($"Enemy state -> {next}");
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
