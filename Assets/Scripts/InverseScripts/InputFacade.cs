using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputFacade : MonoBehaviour {

    public Texture2D inputFacade;
    public string facadeLayoutName;

    /// <summary>
    /// Checks if the components of the facade are set and of correct form.
    /// </summary>
    /// <returns></returns>
    public bool formattedCorrectly() {

        bool isSet = inputFacade != null && facadeLayoutName != "";
        
        if (!isSet) {
            Debug.LogError("Facade with name " + name + " has undefined input images.");
            return false;
        }
        return true;
    }
}
