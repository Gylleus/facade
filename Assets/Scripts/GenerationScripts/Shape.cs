using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Shape : MonoBehaviour {

    public Grammar.Symbol symbol;

    // To prevent infinite loops of protrusion
    public bool hasProtruded = false;
    public bool debugging = false;
    public GameObject occludes;

    // Used for remembering how much to squeeze hexagons
    private Vector3 finalSize;
    public GameObject parent;
    public GameObject topParent;

    public enum ShapeType {
        cube,
        cylinder,
        hexagon
    }

    public ShapeType shapeType = ShapeType.cube;

    void Start() {
        if (gameObject.tag == "TerminalShape") {
            assignMaterial();
        }
    }

    public float absoluteX, absoluteY, absoluteZ;

    public void resizeHexagonCube(GameObject hexCube, Rule.Axis axis, float newLength) {
        switch (axis) {
            case Rule.Axis.X:
                break;
            case Rule.Axis.Y:
                break;
            case Rule.Axis.Z:
                hexCube.transform.localScale = new Vector3(newLength/Mathf.Sqrt(3), hexCube.transform.localScale.y, newLength);
                break;
        }
    }

	public void setShapeType(ShapeType shapeT) {
        MeshFilter mf = GetComponent<MeshFilter>();
        shapeType = shapeT;
        switch (shapeT) {
            case ShapeType.cube:
                if (mf != null) {
                    mf.mesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;
                }
                break;
            case ShapeType.cylinder:
                if (mf != null) {
                    mf.mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<MeshFilter>().mesh;
                }
                break;
        }
    }

    // Returns the transform responsible for scaling. Will be a different element in the case of hexagons.
    public Transform getTransform() {
        Transform ret = transform;
        while (ret.parent != null) {
            ret = ret.parent;
        }
        return ret;
    }

    public void setParent(GameObject p) {
        Shape tmp = p.GetComponent<Shape>();
        while (tmp != null && tmp.parent != null) {
            tmp = tmp.parent.GetComponent<Shape>();
        }
        topParent = tmp.gameObject;
        parent = p.gameObject;
    }

    public void assignParent(Transform newParent) {
        transform.parent = newParent;    
    }

    public void subscribeForSqueeze(Vector3 squeezedSize) {
        finalSize = squeezedSize;
    }

    public void applyHexagonSqueeze() {
        transform.localScale = finalSize;
        transform.parent = parent.transform;
        GrammarDerivator.Finished -= applyHexagonSqueeze;
    }

    public void assignMaterial() {

        string filename = Application.dataPath + "/Textures/GeneratedTextures/" + gameObject.name + ".png";

        Texture2D terminalTexture = new Texture2D(1, 1);

        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material shapeMaterial = mr.material;

       try {
            byte[] imageBytes = File.ReadAllBytes(filename);
            terminalTexture.LoadImage(imageBytes);
            shapeMaterial.mainTexture = terminalTexture;
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarning(gameObject.name + " does not have a material defined. No material was assigned to terminal shape.");
        }
    }

    

}

