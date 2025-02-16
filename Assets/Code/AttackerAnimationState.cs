using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackerAnimationState : StateMachineBehaviour
{
    protected Animator animator;
    protected Attacker playerAttacker;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        this.animator = animator;
        playerAttacker = animator.GetComponent<Attacker>();
    }

    public virtual void OnUpdate () {}

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        OnUpdate();
    }
}
