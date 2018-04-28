using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarDerivator : MonoBehaviour {

    private GameObject[] startingShapes;

    private RuleSelector ruleSelector;

    private List<GameObject> shapes;
    private Grammar grammar;

    public GameObject grammarObject;
    public string textureFolder = "Textures/GeneratedTextures/";

    public delegate void FinishedGeneration();
    public static event FinishedGeneration Finished;

	// Use this for initialization
	void Start () {
        shapes = new List<GameObject>();

        if (grammarObject == null) {
            grammarObject = GameObject.Find("Split Grammar");
        }

        ruleSelector = grammarObject.GetComponent<RuleSelector>();
        grammar = grammarObject.GetComponent<Grammar>();
        startingShapes = new GameObject[] { gameObject };

        foreach (GameObject s in startingShapes) {
            shapes.Add(s);
        }
        StartCoroutine(generate());
	}
	
    private GameObject[] getStartingShapes() {
        return GameObject.FindGameObjectsWithTag("StartShape");
    }

    IEnumerator generate() {
        yield return new WaitUntil(() => grammar.finishedRuleReading);

        List<GameObject> processedShapes = new List<GameObject>();
        List<GameObject> buildings = new List<GameObject>();

        while (shapes.Count > 0) {
            GameObject currentShape = shapes[0];
            // Should make into queue
            shapes.RemoveAt(0);

            Rule chosenRule = ruleSelector.selectRule(currentShape);

            // Check if we are trying to protrude a protruded shape to wrap up our terminal shape
            if (chosenRule is ProtrudeRule && currentShape.GetComponent<Shape>().hasProtruded) {
                chosenRule = null;
            }

            if (currentShape.tag == "StartShape") {
                buildings.Add(currentShape);
                currentShape.tag = "Building";
            }

            if (chosenRule != null) {
                Rule.Axis ruleAxis = chosenRule.axis;
                // Choose the axis for the rule to be performed on if multiple axes are available.
                if (!chosenRule.multiDimensional) chosenRule.axis = chooseAxis(chosenRule, currentShape.transform);

                GameObject[] newShapes = chosenRule.ruleAction(currentShape.transform);

                // If the rule split the previous shape into new components
                if (newShapes != null) {
                    // If we return with the current shape continue with it
                    if (newShapes[0] != currentShape) {
                        MeshRenderer rend = currentShape.GetComponent<MeshRenderer>();
                        if (rend != null) {
                            rend.enabled = false;

                        }
                        Collider col = currentShape.GetComponent<Collider>();
                        if (col != null) {
                            col.enabled = false;
                        }
                    }
                    foreach (GameObject shape in newShapes) {
                        if (!shapes.Contains(shape)) {
                            shapes.Insert(0, shape);
                            Shape shapeS = shape.GetComponent<Shape>();
                            if (shapeS.parent == null && shape.tag != "StartShape") {
                                shapeS.setParent(currentShape);
                            }
                        }
                    }
                    
                }
                // Revert to allow for multiple axes again
                chosenRule.axis = ruleAxis;
            }
            else {
                // We are at a terminal shape
                currentShape.GetComponent<Shape>().assignMaterial();
                currentShape.tag = "TerminalShape";
            }
            // Add the handled shape to our list of processed shapes for later use
            processedShapes.Add(currentShape);
        }

        // Fix potential occlusions of shapes
        grammarObject.GetComponent<OcclusionHandler>().handleOcclusion();
        assignParents(processedShapes);
        cleanParentlessShapes();
        grammarObject.GetComponent<RoofCreator>().createRoofs(buildings.ToArray());
        if (Finished != null) {
            Finished();
        }
    }

    // Assign the parents of all processed objects
    // Reason for retroactive assignment is due to easier calculations in rules using world space instead of relative positions to transform parent
    private void assignParents(List<GameObject> objects) {
        foreach (GameObject obj in objects) {
            Shape shape = obj.GetComponent<Shape>();
            if (shape != null && shape.parent != null) {
                obj.transform.SetParent(shape.parent.transform, true);
            }
        }
    }

    private void cleanParentlessShapes() {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject o in gameObjects) {
            if (o.GetComponent<Shape>() != null) {
                if (o.tag == "Untagged" && o.transform.parent == null && o.transform.childCount < 1) {
                    Destroy(o);
                }
            }
        }
    }

    private Rule.Axis chooseAxis(Rule rule, Transform curTrans) {
        switch (rule.axis) {
            case Rule.Axis.X:
                return rule.axis;
            case Rule.Axis.Y:
                return rule.axis;
            case Rule.Axis.Z:
                return rule.axis;
            case Rule.Axis.XZ:
                return (curTrans.lossyScale.x >= curTrans.lossyScale.z) ? Rule.Axis.X : Rule.Axis.Z;
            case Rule.Axis.XY:
                return (curTrans.lossyScale.x >= curTrans.lossyScale.y) ? Rule.Axis.X : Rule.Axis.Y;
            case Rule.Axis.YZ:
                return (curTrans.lossyScale.y >= curTrans.lossyScale.z) ? Rule.Axis.Y : Rule.Axis.Z;
            case Rule.Axis.XYZ:
                Rule.Axis ax = (curTrans.lossyScale.x >= curTrans.lossyScale.y) ? Rule.Axis.X : Rule.Axis.Y;
                if (ax == Rule.Axis.X) {
                    return (curTrans.lossyScale.x >= curTrans.lossyScale.z) ? Rule.Axis.X : Rule.Axis.Z;
                } else {
                    return (curTrans.lossyScale.y >= curTrans.lossyScale.z) ? Rule.Axis.Y : Rule.Axis.Z;
                }
            default:
                break;
        }
        Debug.LogWarning("Did not find suitable axis for rule for shape " + curTrans.name + ".");
        return Rule.Axis.X;
    }
}
