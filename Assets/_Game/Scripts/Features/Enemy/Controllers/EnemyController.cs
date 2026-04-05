using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;

    StateMachine stateMachine;

    void OnValidate() => this.ValidateRefs();

    void Start()
    {
        stateMachine = new StateMachine();
        var WalkState = new EnemyWanderState(this, animator, agent, 5f);
        Any(WalkState, new FuncPredicate(() => true));
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