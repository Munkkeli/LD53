using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void OnDrawGizmos() {
        if (!Map.Current) return;
        Gizmos.color = Color.red;
        var local = Map.Current.WorldToMap(transform.position);
        Gizmos.DrawCube( Map.Current.MapToWorld((int)local.x, (int)local.y), new Vector3(2, 2));
    }
}
