using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : AttackerAnimationState
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if(playerAttacker.backToIdle)animator.SetTrigger("idle");
    }
}
