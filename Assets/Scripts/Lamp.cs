using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour {

    public Color cordColor = Color.black;
    public float lineDipHeight = 1.0f;
    public float lineWidth = 0.2f;

    private Vector3 leftCordPoint, rightCordPoint;

	// Use this for initialization
	void Start () {
		if (getCordPoints()) {
    //        heightenCords(lineDipHeight);
            createCordLine();
        }
	}
	
    private void heightenCords(float amount) {
        leftCordPoint += new Vector3(0, amount, 0);
        rightCordPoint += new Vector3(0, amount, 0);
    }

    private void createCordLine() {

        LineRenderer lineR = GetComponent<LineRenderer>();
        lineR.positionCount = 3;

        lineR.SetPositions(new Vector3[] { leftCordPoint, transform.position, rightCordPoint });
        //lineR.widthMultiplier = lineWidth;
       // lineR.startColor = cordColor;

    }

    // Gets and sets the points that the cords of the lamp will be attached to.
    // Returns true of succesful, false if not.
    private bool getCordPoints() {

        leftCordPoint = transform.position;
        rightCordPoint = transform.position;

        Ray left = new Ray(transform.position, -transform.right);
        Ray right = new Ray(transform.position, transform.right);
        RaycastHit hit;
        
        // Raycast left
        if (Physics.Raycast(left, out hit)) {
            leftCordPoint = hit.point;
            if (hit.collider.tag != "HangingLamp") {
                leftCordPoint += new Vector3(0, lineDipHeight, 0);
            }
        }
        // Raycast right
        if (Physics.Raycast(right, out hit)) {
            rightCordPoint = hit.point;
            if (hit.collider.tag != "HangingLamp") {
                rightCordPoint += new Vector3(0, lineDipHeight, 0);
            }
        }

        return leftCordPoint != transform.position && rightCordPoint != transform.position;
    }
}
