using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofCreator : MonoBehaviour {

    public float roofHeight = 1.0f;

    public GameObject[] roofTypes;

    public void createRoofs(GameObject[] buildings) {

        foreach (GameObject building in buildings) {
            if (building != null && building.tag != "ToDestroy") {
                GameObject roofType = roofTypes[Random.Range(0, roofTypes.Length)];
                GameObject createdRoof = Instantiate(roofType);

                Transform roofTrans = createdRoof.transform;

                roofTrans.localEulerAngles = Vector3.zero;
                roofTrans.localPosition = building.transform.localPosition + new Vector3(0, building.transform.localScale.y/2, 0);
                
                Mesh m = createdRoof.GetComponent<MeshFilter>().mesh;
                Vector3 b = m.bounds.extents;
                Vector3 buildingScale = building.transform.localScale;
                roofTrans.localScale = new Vector3((buildingScale.x * 0.5f) / b.x, (roofHeight/2)/b.y, (buildingScale.z/2) / b.z);
                roofTrans.localEulerAngles = building.transform.localEulerAngles;
                roofTrans.parent = building.transform;
                
            }
        }
    }
}
