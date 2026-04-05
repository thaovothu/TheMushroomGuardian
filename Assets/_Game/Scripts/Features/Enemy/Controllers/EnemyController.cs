using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] float wanderRadius = 10f;
    [SerializeField] PlayerDetector playerDetector;
    StateMachine stateMachine;

    void OnValidate() => this.ValidateRefs();

    void Start()
    {
        stateMachine = new StateMachine();
        var WalkState = new EnemyWanderState(this, animator, agent, wanderRadius);
        var ChaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
        
        At(WalkState, ChaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
        At(ChaseState, WalkState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
        stateMachine.SetState(WalkState);
    }

    void At (IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any (IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    void Update()
    {
        stateMachine.Update();
    }
    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

}