using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionHandler : MonoBehaviour {

    private const float rayOriginOffset = 0.5f;

	public void handleOcclusion() {
        GameObject[] allTerminals = GameObject.FindGameObjectsWithTag("TerminalShape"); 
        foreach (GameObject shape in allTerminals) {
            if (shape.GetComponent<Shape>() == null) {
                continue;
            }
            fixOcclusionFor(shape);
        }
    }

    // Gets the object that an input object is occluded by. If there is not occluding object null will be returned.
    public void fixOcclusionFor(GameObject shape) {

        float factorFromCenter = 0.2f;
        Vector3[] rayCheckPoints = { shape.transform.position - shape.transform.right * factorFromCenter * shape.transform.localScale.x,
            shape.transform.position + shape.transform.right * factorFromCenter * shape.transform.localScale.x,
            shape.transform.position - shape.transform.up * factorFromCenter * shape.transform.localScale.y,
            shape.transform.position + shape.transform.up * factorFromCenter * shape.transform.localScale.y,
            shape.transform.position - shape.transform.right * factorFromCenter/2 * shape.transform.localScale.x,
            shape.transform.position + shape.transform.right * factorFromCenter/2 * shape.transform.localScale.x,
            shape.transform.position - shape.transform.up * factorFromCenter/2 * shape.transform.localScale.y,
            shape.transform.position + shape.transform.up * factorFromCenter/2 * shape.transform.localScale.y
        };

        foreach (Vector3 checkPoint in rayCheckPoints) {
            Ray checkRay = new Ray(checkPoint + shape.transform.forward * (rayOriginOffset + shape.transform.localScale.z / 2), -shape.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(checkRay, out hit)) {
                Shape cur = shape.GetComponent<Shape>();
                Shape other = hit.collider.gameObject.GetComponent<Shape>();
                if (hit.collider.gameObject != shape && cur != null && other != null && cur.topParent == other.topParent && cur.transform.forward != other.transform.forward) {
                    invertOccludingNormals(hit.collider.gameObject, shape);
                }
            }
        }
    }

    // Changes all normals that are creating the occlusion such that they will be transparent to the occluding side
    private void invertOccludingNormals(GameObject occluder, GameObject victim) {
        // Small constant to avoid textures being at the exact same position, causing graphical errors
        const float additionalPushingConstant = 0.02f;
        float forScale = victim.transform.localScale.x / 2 + additionalPushingConstant;

        Vector3 coolPoint = victim.transform.position + occluder.transform.forward * forScale;
        Vector3 targetPointFront = occluder.transform.position + occluder.transform.forward * occluder.transform.localScale.z / 2;
        Vector3 currentBackPoint = occluder.transform.position - occluder.transform.forward * occluder.transform.localScale.z / 2;
        Vector3 dif = Vector3.Scale(occluder.transform.forward, (coolPoint - currentBackPoint));
        Vector3 targetPointBack = currentBackPoint + occluder.transform.forward * dif.magnitude;
        float newZ = (targetPointFront - targetPointBack).magnitude;

        occluder.transform.localScale += new Vector3(0, 0, newZ - occluder.transform.localScale.z);
        occluder.transform.position = targetPointBack + occluder.transform.forward * newZ / 2;
    }
}
