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
    /*
    public GameObject[] groundFloorTop(Transform trans) {

        GameObject ground = Instantiate(grammar.ground, trans.position, trans.rotation);
        GameObject floor = Instantiate(grammar.floor, trans.position, trans.rotation);
        GameObject top = Instantiate(grammar.top, trans.position, trans.rotation);

        // Position the ground component
        Vector3 tmp = ground.transform.position;
        tmp.y = 0;
        ground.transform.position = tmp;

        // Position the floor component
        tmp = floor.transform.position;
        tmp.y = ground.transform.localScale.z;
        floor.transform.position = tmp;

        // Position the top component
        tmp = top.transform.position;
        tmp.y = trans.localScale.y - top.transform.localScale.y;
        top.transform.position = tmp;

        // Scale the floor component
        tmp = floor.transform.localScale;
        tmp.y = trans.localScale.y - ground.transform.localScale.y - top.transform.localScale.y;
        floor.transform.localScale = tmp;

        return new GameObject[3] { ground, floor, top };
    }
    */
}
