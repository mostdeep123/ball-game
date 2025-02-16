using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    protected virtual void SpawnPlayer (GameObject playerObject, Vector3 pos)
    {
        float x = pos.x;
        float y = playerObject.transform.position.y;
        float z = pos.z;
        playerObject.transform.position = new Vector3(x, y, z);
        playerObject.SetActive(true);
    }

    protected Vector3 GetPointerWorldPosition ()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane + 1.0f;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
