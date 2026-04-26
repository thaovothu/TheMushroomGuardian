using UnityEngine;

public class AIController
{
    [SerializeField, HideInInspector]
    private UnityEngine.AI.NavMeshAgent Agent;

    public override void SetNPC(NPC npc)
    {
        base.SetNpc(npc);
        Agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
}
