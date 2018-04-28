using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceRule : Rule {

    public Shape.ShapeType shapeTypeInto;

    // Define a repeat action
    public override GameObject[] ruleAction(Transform parent) {
        GameObject[] newObjects;

        // If we change to a hexagon
        if (shapeTypeInto == Shape.ShapeType.hexagon) {
            newObjects = new GameObject[3];
            float height = Mathf.Sqrt(3) * parent.localScale.x / 2;
            float width = height / Mathf.Sqrt(2);

            Vector3 spawnPos = parent.position;

            // Middle Cube
            GameObject middleCube = Instantiate(into[0], spawnPos, parent.rotation);
            // Right Cube
            GameObject rightCube = Instantiate(into[0], spawnPos, parent.rotation);
            // Left Cube
            GameObject leftCube = Instantiate(into[0], spawnPos, parent.rotation);

            middleCube.transform.localScale = new Vector3(parent.localScale.x/2, parent.localScale.y,  height);
            leftCube.transform.localScale = new Vector3(width, parent.localScale.y, width);
            rightCube.transform.localScale = new Vector3(width, parent.localScale.y, width);
            rightCube.transform.Rotate(0, 45, 0);
            leftCube.transform.Rotate(0, -45, 0);
        
            newObjects[0] = middleCube;
            newObjects[1] = rightCube;
            newObjects[2] = leftCube;

            foreach (GameObject shape in newObjects) {
                shape.name = into[0].name;
                Shape shapeS = shape.GetComponent<Shape>();
            }
        }
        else {
            newObjects = new GameObject[1];
            newObjects[0] = Instantiate(into[0], parent.position, parent.rotation);
            newObjects[0].transform.localScale = parent.lossyScale;
        }
        return newObjects;
    }
}