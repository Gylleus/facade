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
    private bool toSqueeze = false;

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

    void Update() {
        if (debugging) checkOcclusionFor(gameObject);
    }

    public void setParent(GameObject p) {
        Shape tmp = p.GetComponent<Shape>();
        while (tmp != null && tmp.parent != null) {
            tmp = tmp.parent.GetComponent<Shape>();
        }
        topParent = tmp.gameObject;
        parent = p.gameObject;
    }

    // Gets the object that an input object is occluded by. If there is not occluding object null will be returned.
    public void checkOcclusionFor(GameObject shape) {
        Vector3[] norms = GetComponent<MeshFilter>().mesh.normals;
        Vector3[] verts = GetComponent<MeshFilter>().mesh.vertices;
        int[] triangles = GetComponent<MeshFilter>().mesh.triangles;
        for (int i = 0; i < triangles.Length; i+=3) {
            Vector3 p1 = verts[triangles[i]];
            Vector3 p2 = verts[triangles[i + 1]];
            Vector3 p3 = verts[triangles[i + 2]];
            Vector3 n1 = norms[triangles[i]];
            Vector3 n2 = norms[triangles[i+1]];
            Vector3 n3 = norms[triangles[i+2]];
        }
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

