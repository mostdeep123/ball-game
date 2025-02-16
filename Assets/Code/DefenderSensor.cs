using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderSensor : Sensor
{
    void OnMouseDown() 
    {
        if(GameCore.gameCore.energyBarProperties.defenderPoint >= 3)
        {
            Vector3 pos = GetPointerWorldPosition();
            GameCore.gameCore.energyBarProperties.defenderPoint -= 3;
            GameCore.gameCore.barPointDefender.SetContentValue(GameCore.gameCore.energyBarProperties.defenderPoint);
            SpawnPlayer(GameCore.gameCore.defenders[0], pos);
            GameCore.gameCore.defenders.RemoveAt(0);
        }
    }

    protected override void SpawnPlayer(GameObject playerObject, Vector3 pos)
    {
        base.SpawnPlayer(playerObject, pos);
    }
}
