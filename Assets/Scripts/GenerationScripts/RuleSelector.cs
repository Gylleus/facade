using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleSelector : MonoBehaviour {

    private Grammar grammar;

    void Start() {
        grammar = GetComponent<Grammar>();
    }

	public Rule selectRule(GameObject currentShape) {

        // If grammar does not contain a rule for selected shape (TERMINAL SYMBOL)
        if (!grammar.ruleList.ContainsKey(currentShape.name)) {
            return null;
        }

        List<Rule> shapeRules = grammar.ruleList[currentShape.name];
        List<Rule> matchingRules = new List<Rule>();


        // Extract matching rules
        foreach (Rule rule in shapeRules) {
            if (rule.matchesShape(currentShape)) {
                matchingRules.Add(rule);
            }
        }
        if (matchingRules.Count > 0) {
            Rule chosenRule = selectFromMatchingRules(matchingRules, currentShape);
            return chosenRule;
        } else {
            return null;
        }
    }

    private Rule selectFromMatchingRules(List<Rule> matchingRules, GameObject currentShape) {

        // Give protrusion rules priority if we have not yet protruded
        Shape shapeS = currentShape.GetComponent<Shape>();
        if (shapeS != null) {
            List<Rule> protrusionRules = new List<Rule>();

            foreach (Rule r in matchingRules) {
                if (r is ProtrudeRule) {
                    protrusionRules.Add(r);
                }
            }

            // If any protrusion rules exists
            if (protrusionRules.Count > 0) {
                if (!shapeS.hasProtruded) {
                    return protrusionRules[Random.Range(0, protrusionRules.Count)];
                } else {
                    // If we have already protruded, remove all protrusion rules from matching rules
                    foreach (Rule r in protrusionRules) {
                        matchingRules.Remove(r);
                    }
                }
            }
        }


        return matchingRules[Random.Range(0, matchingRules.Count)];
    }


}
