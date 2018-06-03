using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Splits a shape into equally sized components facing unique directions. An example is cutting up a cube to make facades in each direction. 
 */

public class DecomposeRule : Rule {

    private const bool percentageBased = true;
    private const float decomposedWidth = 0.5f;
    private const float forwardPush = 0.01f;

    // Constraints on geometry of input 

    public DecomposeRule() {
        multiDimensional = true;
    }

    // Define a decompose action
    public override GameObject[] ruleAction(Transform parent) {
        List<GameObject> compShapes = new List<GameObject>();
        List<GameObject> toAdd = new List<GameObject>();
        compShapes.Add(parent.gameObject);
        Vector3 rot = parent.eulerAngles;

        int axesUsed = 0;

        // If we should decompose on the X axis
        if (containsX()) {
            Vector3 firstPos = parent.position + parent.right * parent.lossyScale.x / 4;
            Vector3 secondPos = parent.position - parent.right * parent.lossyScale.x / 4;
            Vector3 firstRot = new Vector3(0, 90, 0);
            Vector3 secondRot = new Vector3(0, 270, 0);
            Vector3 newScale = new Vector3(parent.localScale.z, parent.localScale.y, parent.localScale.x / 2);
            compShapes = decompose(compShapes, firstPos, secondPos, firstRot, secondRot, parent, axesUsed, newScale);
            axesUsed++;
        }

        foreach (GameObject s in toAdd) {
            compShapes.Add(s);
        }
        toAdd.Clear();

        // If we should decompose on the Y axis
        if (containsY()) {
            Vector3 firstPos = parent.position + parent.up * parent.lossyScale.y / 4;
            Vector3 secondPos = parent.position - parent.up * parent.lossyScale.y / 4;
            Vector3 firstRot = new Vector3(90, 0, 0);
            Vector3 secondRot = new Vector3(270, 0, 0);
            Vector3 newScale = new Vector3(parent.localScale.x, parent.localScale.y/2, parent.localScale.z);
            compShapes = decompose(compShapes, firstPos, secondPos, firstRot, secondRot, parent, axesUsed, newScale);
            axesUsed++;
        }

        foreach (GameObject s in toAdd) {
            compShapes.Add(s);
        }
        toAdd.Clear();

        // If we should decompose on the Z axis
        if (containsZ()) {
            Vector3 firstPos = parent.position + parent.forward * parent.lossyScale.z / 4;
            Vector3 secondPos = parent.position - parent.forward * parent.lossyScale.z / 4; ;
            Vector3 firstRot = Vector3.zero;
            Vector3 secondRot = new Vector3(0, 180, 0);
            Vector3 newScale = new Vector3(parent.localScale.x, parent.localScale.y, parent.localScale.z / 2);
            compShapes = decompose(compShapes, firstPos, secondPos, firstRot, secondRot, parent, axesUsed, newScale);
            axesUsed++;
        }

        foreach (GameObject s in toAdd) {
            compShapes.Add(s);
        }
        toAdd.Clear();

        compShapes.Remove(parent.gameObject);
        foreach (GameObject s in compShapes) {
            if (s.transform != parent) {
                scaleDownObject(s);
            }
        }
        return compShapes.ToArray();
    }

    // Instantiates the new decomposed shapes and modifies their transform accordingly to axis
    private List<GameObject> decompose(List<GameObject> compShapes, Vector3 firstPos, Vector3 secondPos, Vector3 firstRot, Vector3 secondRot, Transform parent, int axisIndex, Vector3 newScale) {
        GameObject firstShape = Instantiate(into[axisIndex], firstPos, parent.rotation);
        GameObject secondShape = Instantiate(into[axisIndex], secondPos, parent.rotation);
        firstShape.name = into[axisIndex].name;
        secondShape.name = into[axisIndex].name;
        firstShape.transform.Rotate(firstRot);
        secondShape.transform.Rotate(secondRot);
        firstShape.transform.localScale = newScale;
        secondShape.transform.localScale = newScale;
        compShapes.Add(firstShape);
        compShapes.Add(secondShape);
        return compShapes;
    }

    // Scales down the decomposed object and moves it to suit its former position
    private void scaleDownObject(GameObject s) {
        float shapeDepth = (percentageBased) ? (s.transform.lossyScale.z * decomposedWidth) : decomposedWidth;
        Vector3 tmp = s.transform.localScale;
        s.transform.Translate(s.transform.forward * (s.transform.lossyScale.z - shapeDepth) / 2 + s.transform.forward * forwardPush, Space.World);
        tmp.z = shapeDepth;
        s.transform.localScale = tmp;
    }
}