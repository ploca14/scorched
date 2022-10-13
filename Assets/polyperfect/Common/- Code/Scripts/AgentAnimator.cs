using UnityEngine;
using UnityEngine.AI;

namespace Polyperfect.Common
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentAnimator : PolyMono
    {
        public override string __Usage => $"Animates a NavMeshAgent-driven character. The Animator may be on a child. The {nameof(SpeedParameter)} is set to the velocity divided by {SpeedDivider}";
        
        public string SpeedParameter = "ForwardSpeed";
        public float SpeedDivider = 1f;

        NavMeshAgent _agent;
        Animator _animator;
        
        void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _agent = GetComponent<NavMeshAgent>();

            if (_animator)
                return;

            Debug.LogError($"No animator on a child of {gameObject.name}. One is required for the AgentAnimator");
            enabled = false;
        }

        void Update()
        {
            _animator.SetFloat(SpeedParameter, _agent.velocity.magnitude / SpeedDivider);
        }
    }
}