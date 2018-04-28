using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grammar : MonoBehaviour {

    //public GameObject start, floor, terminalfloor, ground, top;

    public GameObject nonTerminalShape;
    public GameObject[] terminalShapes;
    public Dictionary<string, List<Rule>> ruleList;
    public bool finishedRuleReading;


    public static Mesh cylinder, cube;

    public enum Symbol {
        Start,
        Ground,
        TerminalFloor,
        Floor,
        Top
    };

    public enum TerminalRegions {
        Ground,
        Floor,
        Top
    };

}
