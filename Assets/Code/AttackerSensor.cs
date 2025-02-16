using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerSensor : Sensor
{
    void OnMouseDown()
    {
        if(GameCore.gameCore.energyBarProperties.attackerPoint >= 2)
        {
            Vector3 pos = GetPointerWorldPosition();
            GameCore.gameCore.energyBarProperties.attackerPoint -= 2;
            GameCore.gameCore.barPointAttacker.SetContentValue(GameCore.gameCore.energyBarProperties.attackerPoint);
            Attacker attacker = GameCore.gameCore.attackers[0].GetComponent<Attacker>();
            attacker.InactiveTime(attacker.modelMaterialsAttacker[0], attacker.modelMaterialsAttacker[1]);
            attacker.activationState = Player.ActivationState.Disable;
            attacker.hitFench = false;
            attacker.catched = false;
            SpawnPlayer(GameCore.gameCore.attackers[0], pos);
            if(attacker.speed <= 0)attacker.speed = attacker.startSpeed; 
            GameCore.gameCore.attackers.RemoveAt(0);
        }   
      
    }

    protected override void SpawnPlayer(GameObject playerObject, Vector3 pos)
    {
        base.SpawnPlayer(playerObject, pos);
    }
}
