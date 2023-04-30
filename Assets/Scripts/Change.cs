using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Change : MonoBehaviour {
    public Intersection Intersection;

    private void OnMouseDown() {
        Intersection.Switch();
    }
}
