using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionHandler : MonoBehaviour {

    private const float rayOriginOffset = 5.0f;

	public void handleOcclusion() {
        GameObject[] allTerminals = GameObject.FindGameObjectsWithTag("TerminalShape");
        foreach (GameObject shape in allTerminals) {
            if (shape.GetComponent<Shape>() != null && shape.GetComponent<Shape>().occludes != null) {
                continue;
            }
            GameObject[] occluders = checkOcclusionFor(shape);
            // If an object is occluding our current object
            if (occluders != null) {
                foreach (GameObject occluder in occluders) {
                    invertOccludingNormals(occluder, shape);
                }
            }
        }
    }

    // Gets the object that an input object is occluded by. If there is not occluding object null will be returned.
    public GameObject[] checkOcclusionFor(GameObject shape) {

        float factorFromCenter = 0.4f;
        Vector3[] rayCheckPoints = { shape.transform.position - shape.transform.right * factorFromCenter * shape.transform.localScale.x,
            shape.transform.position + shape.transform.right * factorFromCenter * shape.transform.localScale.x,
            shape.transform.position - shape.transform.up * factorFromCenter * shape.transform.localScale.y,
            shape.transform.position + shape.transform.up * factorFromCenter * shape.transform.localScale.y,
            shape.transform.position - shape.transform.right * factorFromCenter/2 * shape.transform.localScale.x,
            shape.transform.position + shape.transform.right * factorFromCenter/2 * shape.transform.localScale.x,
            shape.transform.position - shape.transform.up * factorFromCenter/2 * shape.transform.localScale.y,
            shape.transform.position + shape.transform.up * factorFromCenter/2 * shape.transform.localScale.y
        };

        List<GameObject> occluders = new List<GameObject>();
        foreach (Vector3 checkPoint in rayCheckPoints) {
            Ray checkRay = new Ray(checkPoint + shape.transform.forward * (rayOriginOffset + shape.transform.localScale.z / 2), -shape.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(checkRay, out hit)) {
                Shape cur = shape.GetComponent<Shape>();
                Shape other = hit.collider.gameObject.GetComponent<Shape>();

                if (hit.collider.gameObject != shape && cur != null && other != null && cur.topParent == other.topParent) {
           //         Debug.DrawLine(checkRay.origin, checkRay.origin + checkRay.direction * 5.0f, Color.red, 45.0f);
                    hit.collider.gameObject.GetComponent<Shape>().occludes = shape;
                    //  return null;
                    occluders.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                } else {
                 //   Debug.DrawLine(checkRay.origin, checkRay.origin + checkRay.direction * 5.0f, Color.green, 45.0f);
                }
            }
        }
        if (occluders.Count > 0) {
            foreach (GameObject g in occluders) {
                g.SetActive(true);
            }
            return occluders.ToArray();
        }
        return null;
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

        Debug.DrawLine(targetPointFront, coolPoint, Color.blue, 20.0f);

        float newZ = (targetPointFront - targetPointBack).magnitude;

        occluder.transform.localScale += new Vector3(0, 0, newZ - occluder.transform.localScale.z);

        occluder.transform.position = targetPointBack + occluder.transform.forward * newZ / 2;
        
    }
}
