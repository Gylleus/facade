using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules : MonoBehaviour {

    private List<SplitRule> splitRules;
    private Shape thisShape;

    public GameObject grammarObject;
    private Grammar grammar;

    void Start() {
        thisShape = GetComponent<Shape>();
        grammar = grammarObject.GetComponent<Grammar>();
    }

    private void instantiateRules() {
        
        switch (thisShape.symbol) {
            case Grammar.Symbol.Start:

                break;
            default:
                break;

        }
    }
}
