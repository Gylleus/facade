using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class RuleReader : MonoBehaviour {

    public string ruleFileName = "rules.txt";
    private List<GameObject> shapes;

    private Grammar grammar;

    private Dictionary<string,List<Rule>> ruleList;

    public GameObject instantiateFrom;

    int lineNr = 0;

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
        string filePath = Application.dataPath + "/" + ruleFileName;
        if (!File.Exists(filePath)) {
            throw new Exception("Rules file " + ruleFileName + " was not found. Has rules not been generated or otherwise defined?");
        }
        string line;
        StreamReader reader = new StreamReader(filePath, Encoding.Default);
        using (reader) {
            do {
                lineNr++;
                line = reader.ReadLine();
                if (line != null) {
                    handleRuleLine(line, lineNr);
                }
            } while (line != null);

            reader.Close();

        }
    }

    private void handleRuleLine(string line, int lineNr) {
        // If the line is empty, skip it
        if (line.Trim(' ') == "") {
            return;
        }
        // First element should be the From shape, the second an arrow, the third the rule type
        string[] splitLine = line.Split(' ');
        
        if (splitLine.Length <= 3) {
            throwSyntaxError("Rule contains too few components.");
        }
        else if (splitLine[1] != "->") {
            throwSyntaxError("Second element is not an arrow ('->').");
        } else {
            string[] typeSplit = splitLine[2].Split('(');
            string axis = typeSplit[1].Substring(0, typeSplit[1].IndexOf(')'));
            addRule(splitLine[0], createNewRule(axis, line, typeSplit[0].ToLower()));
        }
    }

    // Adds a created rule to the rule 
    private void addRule(string shape, Rule newRule) {

        if (!ruleList.ContainsKey(shape)) {
            ruleList.Add(shape, new List<Rule>());
        }
        ruleList[shape].Add(newRule);

    }

    private void throwSyntaxError(string error) {
        string errorText = "Invalid rule syntax: " + error + "  (line: " + lineNr + ")";
        Debug.LogError(errorText);
        throw new Exception(errorText);
    }

    private Rule createNewRule(string axis, string line, string ruleType) {

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
            default:
                throwSyntaxError("Invalid type of rule: " + ruleType + "\nValid rules are split, repeat, decompose, protrude");
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
                throwSyntaxError("Axis of rule non-valid. Written as: " + axis.Trim() + "\nValid values are X, Y, Z, XY, XZ, YZ, XYZ");
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
                        throwSyntaxError("Unknown attribute: " + attType + "\nValid attributes are minX, minY, minZ, maxX, maxY, maxZ");
                        break;
                }
            }
        }
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
