using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatRule : Rule {

    // Define a repeat action
    public override GameObject[] ruleAction(Transform parent) {
        float parentAxisScale = axisScale(parent);

        // Calculate total scale of the repeated pattern
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

        // round the amount of repetitions to the closest multiple of sequences
        int repetitions = Mathf.RoundToInt(parentAxisScale / totalScale);
        GameObject[] repeatShapes = new GameObject[into.Length * repetitions];

        float[] localScales = new float[scale.Length];
        Array.Copy(scale, localScales, scale.Length);
        if (repetitions == 0) {
            Debug.Log(into[0].name);
        }

        // Calculate difference between parents size and the split components size.
        float scaleDifference = parentAxisScale - totalScale * repetitions;
        float multiplier = scaleDifference / (totalRelativeScale * repetitions);
        // Scale all flexible elements equally
        foreach (int el in flexibleElements) {
            localScales[el] += localScales[el] * multiplier;
        }

        Vector3 repeatStart = parent.position - getAxis(parent) * parentAxisScale / 2;
        Vector3 spawnPos = repeatStart;


        for (int r = 0; r < repetitions; r++) {
            for (int i = 0; i < into.Length; i++) {
                spawnPos += localScales[i] / 2 * getAxis(parent);
                // Instantiate the new object
                GameObject newObject = Instantiate(into[i], spawnPos, parent.rotation);
                newObject.name = into[i].name;
                // Scale the object accordingly
                newObject.transform.localScale = parent.lossyScale - nonRotatedAxis() * parentAxisScale + nonRotatedAxis() * localScales[i];
                Shape objectShape = newObject.GetComponent<Shape>();
                // Check if some size of the area has an absolute value
                if (objectShape != null) {
                    Vector3 tmp = newObject.transform.localScale;
                    tmp.x = (objectShape.absoluteX > 0 && axis != Axis.X) ? objectShape.absoluteX : tmp.x;
                    tmp.y = (objectShape.absoluteY > 0 && axis != Axis.Y) ? objectShape.absoluteY : tmp.y;
                    tmp.z = (objectShape.absoluteZ > 0 && axis != Axis.Z) ? objectShape.absoluteZ : tmp.z;
                    newObject.transform.localScale = tmp;
                }
                repeatShapes[i+(r*into.Length)] = newObject;
                spawnPos += localScales[i] * getAxis(parent) / 2;
     //           newObject.transform.SetParent(parent);
          //      Debug.Log(newObject.name + " - " + newObject.transform.lossyScale + "  |  " + parent.name + "  - " + parent.lossyScale);

                //      Debug.Log(i + (r * into.Length));
            }
        }
        return repeatShapes;
    }

}