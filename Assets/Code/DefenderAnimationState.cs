using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderAnimationState : StateMachineBehaviour
{
    protected Animator animator;
    protected Defender playerDefender;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        this.animator = animator;
        playerDefender = animator.GetComponent<Defender>();
    }

    public virtual void OnUpdate () {}

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        OnUpdate();
    }
}
