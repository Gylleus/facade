using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rule : ScriptableObject {

    public GameObject[] into;
    public float[] scale;
    public int[] flexibleElements;
    public bool multiDimensional = false;

    public enum Axis {
        X,
        Y,
        Z,
        XY,
        XZ,
        YZ,
        XYZ
    };

    // Geometric constraints for shape to invoke rule
    public Vector3 minSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    public Vector3 maxSize = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

    // On which axis the rule will be performed
    public Axis axis;

    public bool checkSizeContraints(GameObject shape) {
        Shape s = shape.GetComponent<Shape>();
        // Try to get the responsible transform to get lossy scale
        Transform trans = (s != null) ? s.getTransform() : shape.transform;
        Vector3 size = trans.lossyScale;
        return (size.x >= minSize.x && size.x <= maxSize.x && size.y >= minSize.y && size.y <= maxSize.y
               && size.z >= minSize.z && size.z <= maxSize.z);
    }

    public virtual bool matchesShape(GameObject shape) {
        return checkSizeContraints(shape);
    }

    public Vector3 getAxis(Transform parent) {
        if (axis == Axis.X) {
            //  return new Vector3(1, 0, 0);
            return parent.right;
        }
        else if (axis == Axis.Y) {
            //return new Vector3(0, 1, 0);
            return parent.up;
        } else {
            //            return new Vector3(0, 0, 1);
            return parent.forward;
        }
    }

    public Vector3 nonRotatedAxis() {
        if (axis == Axis.X) {
            return new Vector3(1, 0, 0);
        }
        else if (axis == Axis.Y) {
            return new Vector3(0, 1, 0);
        }
        else if (axis == Axis.Z) {
            return new Vector3(0, 0, 1);
        }
        else if (axis == Axis.XY) {
            return new Vector3(1, 1, 0);
        }
        else if (axis == Axis.XZ) {
            return new Vector3(1, 0, 1);
        }
        else if (axis == Axis.YZ) {
            return new Vector3(0, 1, 1);
        }
        else {
            return new Vector3(1, 1, 1);
        }
    }

    public float axisScale(Transform parent) {
        if (axis == Axis.X) {
            //  return new Vector3(1, 0, 0);
            return parent.lossyScale.x;
        }
        else if (axis == Axis.Y) {
            //return new Vector3(0, 1, 0);
            return parent.lossyScale.y;
        }
        else {
            //            return new Vector3(0, 0, 1);
            return parent.lossyScale.z;
        }
    }

    // Action to be performed on rule invocation. Abstract to enable different types of rules.
    public abstract GameObject[] ruleAction(Transform parent);

    public bool containsX() {
        return axis == Axis.X || axis == Axis.XY || axis == Axis.XZ || axis == Axis.XYZ;
    }

    public bool containsY() {
        return axis == Axis.Y || axis == Axis.XY || axis == Axis.YZ || axis == Axis.XYZ;
    }

    public bool containsZ() {
        return axis == Axis.Z || axis == Axis.XZ || axis == Axis.YZ || axis == Axis.XYZ;
    }

}
