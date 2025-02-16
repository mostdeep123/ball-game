using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleDefender : DefenderAnimationState
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if(playerDefender.isRun)animator.SetTrigger("run");
    }
}
