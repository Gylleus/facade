using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symbol : MonoBehaviour {

    public float xSize, ySize, zSize;

    public Symbol (float x, float y, float z) {
        xSize = x;
        ySize = y;
        zSize = z;
    }

    public Vector3 size() {
        return new Vector3(xSize, ySize, zSize);
    }

}
