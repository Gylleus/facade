using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitRule : Rule {

    // Define a split action
    public override GameObject[] ruleAction(Transform parent) {
        GameObject[] splitShapes = new GameObject[into.Length];
        float parentAxisScale = axisScale(parent);
        // Calculate total scale of split componenets.
        float totalScale = 0;
        float totalRelativeScale = 0;
        foreach (float s in scale) {
            totalScale += s;
        }

        // If we have no flexible elements make them all flexible
        if (flexibleElements.Length == 0) {
            flexibleElements = new int[into.Length];
            for (int i = 0; i < into.Length; i++) {
                flexibleElements[i] = i;
            }
        }

        foreach (int el in flexibleElements) {
            totalRelativeScale += scale[el];
        }

        // Calculate difference between parents size and the split components size.
        float scaleDifference = parentAxisScale - totalScale;
        // Scale all flexible elements equally
        float multiplier = scaleDifference / totalRelativeScale;
        // Scale all flexible elements equally
        foreach (int el in flexibleElements) {
            scale[el] += scale[el] * multiplier;
        }

        // Calculate where the split starts
        Vector3 splitStart = parent.position - getAxis(parent) * parentAxisScale / 2;
        Vector3 spawnPos = splitStart;

        for (int i = 0; i < into.Length; i++) {
            spawnPos += scale[i]/2 * getAxis(parent);
            // Instantiate the new object
            GameObject newObject = Instantiate(into[i], spawnPos, parent.rotation);
            newObject.name = into[i].name;
            // Scale the object accordingly
            newObject.transform.localScale = parent.lossyScale - nonRotatedAxis() * parentAxisScale + nonRotatedAxis() * scale[i];
            Shape objectShape = newObject.GetComponent<Shape>();
            // Check if some size of the area has an absolute value
            if (objectShape != null) {
                Vector3 tmp = newObject.transform.localScale;
                tmp.x = (objectShape.absoluteX > 0 && axis != Axis.X) ? objectShape.absoluteX : tmp.x;
                tmp.y = (objectShape.absoluteY > 0 && axis != Axis.Y) ? objectShape.absoluteY : tmp.y;
                tmp.z = (objectShape.absoluteZ > 0 && axis != Axis.Z) ? objectShape.absoluteZ : tmp.z;
                newObject.transform.localScale = tmp;
            }
            splitShapes[i] = newObject;
            spawnPos += scale[i] * getAxis(parent) /2;
        }
        return splitShapes;
    }
}