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
            

       
        //    GameObject squeezeContainer = new GameObject();
         //   float width = parent.localScale.x / (2 * Mathf.Sqrt(2));
            float height = Mathf.Sqrt(3) * parent.localScale.x / 2;
            float width = height / Mathf.Sqrt(2);

            // Vector3 spawnPos = parent.position + ((parent.localScale.z - height)/2 + height/2) * parent.transform.forward;
            Vector3 spawnPos = parent.position;

            // Middle Cube
            GameObject middleCube = Instantiate(into[0], spawnPos, parent.rotation);
            // Right Cube
            //GameObject rightCube = Instantiate(into[0], spawnPos + parent.transform.right * (parent.localScale.x - width) / 2, parent.rotation);
            GameObject rightCube = Instantiate(into[0], spawnPos, parent.rotation);
            //newObjects[1] = Instantiate(into[0]);
            // Left Cube
            //GameObject leftCube = Instantiate(into[0], spawnPos - parent.transform.right * (parent.localScale.x - width) / 2, parent.rotation);
            GameObject leftCube = Instantiate(into[0], spawnPos, parent.rotation);
            //newObjects[2] = Instantiate(into[0]);

            // Rotate the left and right cube accordingly

            // Use empty parent objects to compress rotated cubes along parent z-axis

            middleCube.transform.localScale = new Vector3(parent.localScale.x/2, parent.localScale.y,  height);
            leftCube.transform.localScale = new Vector3(width, parent.localScale.y, width);
            rightCube.transform.localScale = new Vector3(width, parent.localScale.y, width);
            //         leftContainer.transform.localScale = new Vector3(parent.localScale.x/4 / Mathf.Sqrt(2), parent.localScale.y,  height);
            //         rightContainer.transform.localScale = new Vector3(parent.localScale.x / 4 / Mathf.Sqrt(2), parent.localScale.y,  height);
            rightCube.transform.Rotate(0, 45, 0);
            leftCube.transform.Rotate(0, -45, 0);

            Debug.Log(parent.transform.localScale.z + "  -  " + height);
        
            newObjects[0] = middleCube;
            newObjects[1] = rightCube;
            newObjects[2] = leftCube;

            foreach (GameObject shape in newObjects) {
                shape.name = into[0].name;
                Shape shapeS = shape.GetComponent<Shape>();
           //     shapeS.parent = parent.gameObject;
            }

        }
        else {
            newObjects = new GameObject[1];
            newObjects[0] = Instantiate(into[0], parent.position, parent.rotation);
            newObjects[0].transform.localScale = parent.lossyScale;
        }

        // Göra hexagon
        // Ändra i split så borde det vara lugnt

       

        return newObjects;
    }

}