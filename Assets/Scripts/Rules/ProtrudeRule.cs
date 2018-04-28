using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtrudeRule : Rule {

    // Extra contraints on if shape matches rukle
    public override bool matchesShape(GameObject shape) {
        bool hasProtruded = shape.GetComponent<Shape>().hasProtruded;
        bool sizeMatches = checkSizeContraints(shape);
        return !hasProtruded && sizeMatches;
    }

    // Define a split action
    public override GameObject[] ruleAction(Transform parent) {
        Vector3 scaleVector = Vector3.zero;
        int scaleIndex = 0;
        Transform returnT = parent;

        Shape transformShape = parent.GetComponent<Shape>();

        bool negativeProtusion = false;

        // Check if shape has already protruded to prevent infinite loops
        if (transformShape.hasProtruded) {
            return null;
        }

        transformShape.hasProtruded = true;

        if (axis == Axis.X || axis == Axis.XY || axis == Axis.XZ || axis == Axis.XYZ) {
            if (scale[scaleIndex] < 0) {
                negativeProtusion = true;
                parent.Translate(scale[scaleIndex] * parent.right, Space.World);
            } else {
                scaleVector.x = scale[scaleIndex];
            }
            scaleIndex++;
        }
        if (axis == Axis.Y || axis == Axis.XY || axis == Axis.YZ || axis == Axis.XYZ) {
            if (scale[scaleIndex] < 0) {
                negativeProtusion = true;
                parent.Translate(scale[scaleIndex] * parent.up, Space.World);
            }
            else {
                scaleVector.y = scale[scaleIndex];
            }
            scaleIndex++;
        }
        if (axis == Axis.Z || axis == Axis.XZ || axis == Axis.YZ || axis == Axis.XYZ) {
            if (scale[scaleIndex] < 0) {
                negativeProtusion = true;
                parent.Translate(scale[scaleIndex] * parent.forward, Space.World);
            }
            else {
                scaleVector.z = scale[scaleIndex];
            }
            scaleIndex++;
        }

        // Fix for other dimensions

        if (!negativeProtusion) {
            //parent.Translate((parent.localScale.x / 2 * parent.right + parent.localScale.y / 2 * parent.up + parent.localScale.z / 2 * parent.forward) / 2, Space.World);
         //   parent.Translate(parent.localScale.z / 2 * parent.forward / 2, Space.World);
            parent.localScale = parent.lossyScale + scaleVector;
            parent.position = parent.transform.position + parent.transform.forward * scaleVector.z /2;
        } else {
            parent.localScale = parent.lossyScale + scaleVector * 2;
        }



        return new GameObject[] { parent.gameObject };
    }



    /*
        public bool matchesShape(Shape shape) {
            return (shape.size.x >= minSize.x && shape.size.x <= maxSize.x && shape.size.y >= minSize.y && shape.size.y <= maxSize.y
                && shape.size.z >= minSize.z && shape.size.z <= maxSize.z);
        }
        */
    /*
    public Symbol from;
    
    public SplitRule (Symbol f, GameObject[] t, float xmi, float xma, float ymi, float yma, float zmi, float zma) {
        xMin = xmi; xMax = xma;
        yMin = ymi; yMax = yma;
        zMin = zmi; zMax = zma;

        from = f;
        into = t;
    }
    */

    /*
    public GameObject[] into;
    public Vector3[] position;
    public bool[] absoluteX;
    public bool[] absoluteY;
    public bool[] absoluteZ;
    public bool[] inheritXSize;
    public bool[] inheritYSize;
    public bool[] inheritZSize;
    */

}