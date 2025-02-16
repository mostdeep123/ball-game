using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Run : AttackerAnimationState
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if(playerAttacker.isDie)animator.SetTrigger("die");
        if(playerAttacker.isIdle)animator.SetTrigger("idle");
        if(playerAttacker.isCatch)animator.SetTrigger("hit");
    }
}
