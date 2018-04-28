using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class RuleReader : MonoBehaviour {

    public string ruleFileName = "Assets/rules.txt";
    private List<GameObject> shapes;

    private Grammar grammar;

    private Dictionary<string,List<Rule>> ruleList;

    public GameObject instantiateFrom;

	// Use this for initialization
	void Start () {
        ruleList = new Dictionary<string, List<Rule>>();
        grammar = GetComponent<Grammar>();
        shapes = new List<GameObject>();
        readRules();
        grammar.ruleList = ruleList;
        grammar.finishedRuleReading = true;
	}

    private void getSymbols() {
        foreach(GameObject shape in grammar.terminalShapes) {
            shapes.Add(shape);
        }

    }

    private void readRules() {
            string line;
            StreamReader reader = new StreamReader(Application.dataPath + "/" + ruleFileName, Encoding.Default);
            using (reader) {
                do {
                    line = reader.ReadLine();
                    if (line != null) {
                        handleRuleLine(line);
                    }
                } while (line != null);

                reader.Close();

            }
    }

    private void handleRuleLine(string line) {
        string[] splitLine = line.Split(' ');
        string from = splitLine[0];
        // KANSKE KAPAR SCOPET
        if (splitLine.Length > 2 && splitLine[1] == "->") {
            string[] typeSplit = splitLine[2].Split('(');
            string axis = typeSplit[1].Substring(0, typeSplit[1].IndexOf(')'));
            addRule(from, createSplitRule(axis, line, typeSplit[0].ToLower()));
        }
    }
    
    // Adds a created rule to the rule 
    private void addRule(string shape, Rule newRule) {

        if (!ruleList.ContainsKey(shape)) {
            ruleList.Add(shape, new List<Rule>());
        }
        ruleList[shape].Add(newRule);

    }

    private Rule createSplitRule(string axis, string line, string ruleType) {

        Rule newRule = null;

        switch (ruleType) {
            case "split":
                newRule = ScriptableObject.CreateInstance<SplitRule>();
                break;
            case "repeat":
                newRule = ScriptableObject.CreateInstance<RepeatRule>();
                break;
            case "decompose":
                newRule = ScriptableObject.CreateInstance<DecomposeRule>();
                break;
            case "protrude":
                newRule = ScriptableObject.CreateInstance<ProtrudeRule>();
                break;
            case "replace":
                return createReplaceRule(axis, line, ruleType);
            default:
                Debug.Log("Invalid type of rule.");
                return newRule;
        }

        string[] newRuleShapes = line.Substring(line.IndexOf('{')+1, line.IndexOf('}')-1- line.IndexOf('{')).Split('|');
        newRule.into = new GameObject[newRuleShapes.Length];
        newRule.scale = new float[newRuleShapes.Length];
        List<int> relative = new List<int>();

        switch (axis.Trim().ToUpper()) {
            case "X":
                newRule.axis = Rule.Axis.X;
                break;
            case "Y":
                newRule.axis = Rule.Axis.Y;
                break;
            case "Z":
                newRule.axis = Rule.Axis.Z;
                break;
            case "XY":
                newRule.axis = Rule.Axis.XY;
                break;
            case "XZ":
                newRule.axis = Rule.Axis.XZ;
                break;
            case "YZ":
                newRule.axis = Rule.Axis.YZ;
                break;
            case "XYZ":
                newRule.axis = Rule.Axis.XYZ;
                break;
            default:
                Debug.LogWarning("Axis of rule misformed. Written as: " + axis.Trim() + ".");
                break;
        }

        // Read through each shape to be created
        for (int i = 0; i < newRuleShapes.Length; i++) {
            string[] shapeSplit = newRuleShapes[i].Split(':');

            //Extract scale
            string scale = shapeSplit[0].Trim();
            float number;
            if (scale[scale.Length-1] == 'N') {
                relative.Add(i);
                if (scale.Length == 1) {
                    newRule.scale[i] = 1;
                } else {
                    newRule.scale[i] = float.Parse(scale.Substring(0, scale.Length - 1));
                }
            } else if (float.TryParse(scale, out number)) {
                newRule.scale[i] = number;
            } else {
                newRule.scale[i] = 0;
            }

            string shapeName = (shapeSplit.Length > 1) ? shapeSplit[1].Trim() : shapeSplit[0].Trim();
            if (!shapeExists(shapeName)) {
                shapes.Add(createNewShapeObject(shapeName));
            }
            newRule.into[i] = getShape(shapeName);

            if (newRule.into[i] == null) {
                Debug.LogError("Could not find shape of name " + shapeName);
            }
        }
        readAttributes(line, newRule);
        newRule.flexibleElements = relative.ToArray();
        return newRule;
    }

    private void readAttributes(string line, Rule newRule) {
        int attStart = line.IndexOf('[');
        int attEnd = line.IndexOf(']');
        // If the rule has attributes attached
        if (attStart > 0 && attEnd > 0) {
            string[] attributes = line.Substring(attStart + 1, attEnd - attStart - 1).Split('|');
            foreach (string a in attributes) {
                string[] attSplit = a.Split(':');
                string attType = attSplit[0];
                string attValue = attSplit[1];

                switch (attType.Trim()) {
                    case "minX":
                        newRule.minSize.x = float.Parse(attValue);
                        break;
                    case "minY":
                        newRule.minSize.y = float.Parse(attValue);
                        break;
                    case "minZ":
                        newRule.minSize.z = float.Parse(attValue);
                        break;
                    case "maxX":
                        newRule.maxSize.x = float.Parse(attValue);
                        break;
                    case "maxY":
                        newRule.maxSize.y = float.Parse(attValue);
                        break;
                    case "maxZ":
                        newRule.maxSize.z = float.Parse(attValue);
                        break;
                    default:
                        Debug.LogWarning("Unknown attribute: " + attType);
                        break;
                }
            }
        }
    }

    private Rule createReplaceRule(string shapeType, string line, string ruleType) {
        ReplaceRule newRule = ScriptableObject.CreateInstance<ReplaceRule>();
        string newRuleShapes = line.Substring(line.IndexOf('{') + 1, line.IndexOf('}') - 1 - line.IndexOf('{')).Trim();

        switch (shapeType.Trim().ToLower()) {
            case "cylinder":
                newRule.shapeTypeInto = Shape.ShapeType.cylinder;
                break;
            case "hexagon":
                newRule.shapeTypeInto = Shape.ShapeType.hexagon;
                break;
            default:
                Debug.LogError("Error in new shape type name in replace rule.");
                break;
        }

        newRule.into = new GameObject[1];
        
        if (!shapeExists(newRuleShapes)) {
            shapes.Add(createNewShapeObject(newRuleShapes));
        }
        newRule.into[0] = getShape(newRuleShapes);
        readAttributes(line, newRule);

        return newRule;
    }

    private GameObject getShape(string shapeName) {
        foreach (GameObject shape in shapes) {
            if (shape.gameObject.name.Trim() == shapeName) {
                return shape.gameObject;
            }
        }
        return null;
    }

    private bool shapeExists(string shapeName) {
        foreach (GameObject shape in shapes) {
            if (shape.name.Trim() == shapeName.Trim()) {
                return true;
            }
        }
        return false;
    }


    private GameObject createNewShapeObject(string name) {
        GameObject newObject = Instantiate(instantiateFrom);
        newObject.name = name;
        return newObject;
    }

}
