using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Rectangle = RuleGenerator.Rectangle;

public static class MaterialExtractor {

    public static void extractMaterials(Dictionary<Color,List<Rectangle>> terminals, Texture2D facade) {
        // Select one from each kind of terminal to extract the texture
        List<Rectangle> chosenTerminals = new List<Rectangle>();

        foreach (Color c in terminals.Keys) {
            chosenTerminals.Add(chooseFromTerminals(terminals[c]));
        }

        System.IO.Directory.CreateDirectory(Application.dataPath + "/Textures/GeneratedTextures");
        foreach (Rectangle terminalRect in chosenTerminals) {
            Texture2D terminalTexture = cutFromFacade(terminalRect, facade);
            byte[] bytes = terminalTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Textures/GeneratedTextures/" + terminalRect.name + ".png", bytes);
        }
    }

    private static Texture2D cutFromFacade(Rectangle cutArea, Texture2D facade) {
        int width = cutArea.toX - cutArea.fromX;
        int height = cutArea.toY - cutArea.fromY;
        Texture2D cutTexture = new Texture2D(width, height);

        if (width == 0 || height == 0) {
            Debug.LogWarning("Nonvalid width/height of area. From (" + cutArea.fromX + "," + cutArea.fromY + ") - To: (" + cutArea.toX + "," + cutArea.toY + ")");
        }

        cutTexture.SetPixels(facade.GetPixels(cutArea.fromX, cutArea.fromY, width, height));

        return cutTexture;
    }

    private static Rectangle chooseFromTerminals(List<Rectangle> terminals) {
        return terminals[Random.Range(0, terminals.Count-1)];        
    }
}
