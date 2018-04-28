using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class RuleGenerator : MonoBehaviour {

    // Public variables

    public string outputFileName = "rules.txt";

    public string buildingName;
    public float buildingHeight = 1;
    public float buildingWidth = 1;

    public Texture2D inputFacade, facadeLayout;
    public string terminalRegionsName;

    // Private variables

    // regionIndex is used to name regions
    private int regionIndex = 1;
    private int facadeHeight, facadeWidth;
    private Dictionary<Region, string> regionNames = new Dictionary<Region, string>();
    private List<string> hasRules = new List<string>();
    private StreamWriter fileDS;

    // Contains the names of regions that can stretch
    private List<string> relativeRegions = new List<string>();

    public class Rectangle {
        public int fromX, fromY, toX, toY;
        public string name;
        public float depth;
        public Color symbol;

        public void debugPrint() {
            Debug.Log("Rectangle from (" + fromX + "," + fromY + ")  -  to (" + toX + "," + toY + ")");
        }
    }

    public void beginGeneration(Dictionary<Color,List<Rectangle>> rectangles, InputFacade inputF, StreamWriter sw) {
        fileDS = sw;
        if (!inputF.formattedCorrectly()) {
            Debug.LogError("Skipping " + inputF.gameObject.name);
        }
        else {
            buildingName = inputF.gameObject.name;
            inputFacade = inputF.inputFacade;
            
            facadeHeight = inputFacade.height;
            facadeWidth = inputFacade.width;

            generateRules(rectangles);
        }
    }

    private void generateRules(Dictionary<Color, List<Rectangle>> rectangles) {

        List<Region> repeatedRegions = null;
        List<Rectangle> terminalRegions = new List<Rectangle>();
        foreach (Color c in rectangles.Keys) {
            int count = 0;
            foreach (Rectangle ret in rectangles[c]) {
                terminalRegions.Add(ret);
                count++;
            }
        }
        writeDebugPicture(terminalRegions);

        splitFacade(repeatedRegions, terminalRegions);
        setTerminalNames(rectangles);
        checkTerminalProtrusion(rectangles);
        MaterialExtractor.extractMaterials(rectangles, inputFacade);
    }

    private void writeInitialRules() {
        writeToRuleFile(buildingName + " -> decompose(XZ) {" + buildingName + "Start" + " | " + buildingName + "Start}");
    }

    private void splitFacade(List<Region> repeatedRegions, List<Rectangle> terminalRegions) {

        writeInitialRules();

        Queue<Region> regionsToSplit = new Queue<Region>();

        Region start = new Region(terminalRegions);

        regionNames.Add(start, buildingName+"Start");
        regionsToSplit.Enqueue(start);

        while (regionsToSplit.Count > 0) {
            Region toSplit = regionsToSplit.Dequeue();

            // If the current region is a terminal region then we should not split further
            if (toSplit.terminals.Count == 1 || hasRules.Contains(getRegionName(toSplit))) {
                continue;
            }

            // Gets the list of areas that the region can be split into on x and y axes respectively
            List<Region> xSplits = RegionFinder.createSplitHorizontal(toSplit);
            List<Region> ySplits = RegionFinder.createSplitVertical(toSplit);

            List<Region> toEnqueue = new List<Region>();

            // Choose the one with most contained elements
            if (xSplits == null && ySplits == null) {
                // No splits were able to be performed
                Debug.Assert(toSplit.terminals.Count == 1, "Unable to split non-terminal region. Terminating rule generation.");
                toSplit.debugPrintRegion();
                return;
            } else if (xSplits != null && (ySplits == null || xSplits.Count >= ySplits.Count)) {
                // If the X-split is preferred
                createSplitRule(xSplits, repeatedRegions, "X", getRegionName(toSplit));
                toEnqueue = xSplits;
            }
            else {
                // If the Y-split is preferred
                toEnqueue = createSplitRule(ySplits, repeatedRegions, "Y", getRegionName(toSplit));
                toEnqueue = ySplits;
            }

            foreach (Region newRegion in toEnqueue) {
                regionsToSplit.Enqueue(newRegion);
            }

            fileDS.Flush();
        }

    }

    // Deducts and write a split rule given a split on an axis
    private List<Region> createSplitRule(List<Region> splits, List<Region> repeats, string axis, string regionName) {
        List<Region> splitRepeats = new List<Region>();
        List<Region> nonRepeats = new List<Region>();

        // If this area already has a rule defined
        if (hasRules.Contains(regionName)) {
            return splits;
        }

        repeats = RegionFinder.findRealRepeats(splits, axis);
        
        // Bad datastructure and complexity, should use other
        foreach (Region current in splits) {
            // Find the largest repeat-region that contains our current split region
            Region containedIn = null;
            if (repeats != null) {
                foreach (Region other in repeats) {
                    // If the repeated region contains the split region and is greater than our current one
                    if (other.containsRegion(current) && (containedIn == null || (other.terminals.Count > containedIn.terminals.Count))) {
                        containedIn = other;
                        other.subregions.Add(current);
                    }
                }
            }
            if (containedIn == null) {
                nonRepeats.Add(current);
            }
            else if (!splitRepeats.Contains(containedIn)) {
                // If there was a repeated area that contains our split region and it was not already added
                splitRepeats.Add(containedIn);
            }
        }
        
        splits = new List<Region>(splitRepeats);
        splits.AddRange(nonRepeats);

        splits = RegionManager.sortRegions(splits, axis);

        foreach (Region region in splits) {
            setRegionName(region);
        }

        foreach (Region repeatR in splitRepeats) {
            relativeRegions.Add(getRegionName(repeatR));
        }

        // Add case for just repeat later
        if (splits.Count > splitRepeats.Count) {
            string rule = regionName + " -> split(" + axis + ")";
            rule += createShapeSplitString(splits, axis);
            writeToRuleFile(rule);
            hasRules.Add(regionName);     
        }
        // Now we need to write the repeat rules, which regions are contained in the large repeat areas 
        foreach (Region r in splitRepeats) {
            writeRepeatRule(r, axis);
        }
        return splits;
    }

    private void writeRepeatRule(Region r, string axis) {

        string from = getRegionName(r);
        string repeatRule =  from + " -> repeat(" + axis + ")";
        // If there already exists a rule
        if (hasRules.Contains(from)) {
            return;
        }
        hasRules.Add(from);
        List<Region> containedShapes = RegionManager.sortRegions(r.subregions, axis);

        // Try to find the repeat sequence to identify the repeating pattern in the repeat
        List<Region> repeatSequence = new List<Region>();
        for (int i = 0; i < containedShapes.Count; i++) {
            repeatSequence.Add(containedShapes[i]);
            int repeatLength = repeatSequence.Count;
            // If the length of our sequence is not a divisor of the repeating region it cannot be what we are looking for
            if (containedShapes.Count % repeatLength != 0) {
                continue;
            }

            bool matches = true;

            // Iterate over the contained shapes and see if our pattern repeats
            for (int k = 0; k < containedShapes.Count/repeatLength; k++) {
                for (int j = 0; j < repeatLength; j++) {
                    Region other = containedShapes[repeatLength * k + j];
                    if (!containedShapes[j].equalTerminals(other)) {
                        matches = false;
                        break;
                    }
                }
                if (!matches) {
                    break;
                }
            }
            // If we have found a matching pattern
            if (matches) {
                break;
            }
        }

        repeatRule += createShapeSplitString(repeatSequence, axis);
        writeToRuleFile(repeatRule);
    }

    // Creates and returns the string that defines the size and shapes of that the rule splits into
    public string createShapeSplitString(List<Region> shapes, string axis) {
        string splitString = " {";
        int facadeLength = (axis == "X") ? inputFacade.width : inputFacade.height;
        float buildingSize = (axis == "X") ? buildingWidth : buildingHeight;
        for (int i = 0; i < shapes.Count; i++) {
            Region subarea = shapes[i];
            float size = (axis == "X") ? (subarea.toX - subarea.fromX) * 1f / facadeLength : (subarea.toY - subarea.fromY) * 1f / facadeLength;
            string shapeName = getRegionName(subarea);
            string relative = (relativeRegions.Contains(shapeName)) ? "N" : "";
            splitString += size * buildingSize + relative + ": " + shapeName;

            if (i < shapes.Count - 1) {
                splitString += " | ";
            }
        }
        splitString += "}";
        return splitString;
    }

    private void setRegionName(Region r) {
        bool exists = false;
        // Check if the region type already exists so we dont give redundant names
        foreach (Region other in regionNames.Keys) {
            if (r.equalTerminals(other)) {
                exists = true;
            }
        }
        if (!exists) {
            // Check if it is a terminal region and it has a name
            string addedName = "";
            if (r.terminals.Count == 1 && r.terminals[0].name != "") {
                addedName = r.terminals[0].name;
            } else {
                addedName += regionIndex;
                regionIndex++;
            }
            string newName = buildingName + addedName;
            regionNames.Add(r, newName);
        }
    }

    private string getRegionName(Region r) {
        foreach (Region other in regionNames.Keys) {
            if (r.equalTerminals(other)) {
                return regionNames[other];
            }
        }

        // If no name for this kind of region was found
        setRegionName(r);
        return regionNames[r];
    }



    private List<Vector3> findColors(Texture2D lul) {
        byte[] bytes = File.ReadAllBytes(Application.dataPath + terminalRegionsName);
        Texture2D image = new Texture2D(lul.width, lul.height);
        image.LoadImage(bytes);
        image = lul;
        List<Vector3> colorsFound = new List<Vector3>();

        for (int x = 0; x < image.width; x++) {
            for (int y = 0; y < image.height; y++) {
                Color col = image.GetPixel(x, y);
                Vector3 colV = new Vector3(col.r, col.b, col.g);
                if (!colorsFound.Contains(colV)) {
                    colorsFound.Add(colV);
                }
            }
        }
        return colorsFound;
    }

    private void setTerminalNames(Dictionary<Color,List<Rectangle>> terminals) {
        foreach (Color c in terminals.Keys) {
            foreach (Rectangle rect in terminals[c]) {
                Region termReg = new Region(rect);
                rect.name = getRegionName(termReg);
            }
        }

    }

    private void writeToRuleFile(string newRule) {
        fileDS.WriteLine(newRule);
    }

    private void writeDebugPicture(List<Rectangle> rectangles) {

        Texture2D debugTex = new Texture2D(facadeWidth, facadeHeight);

        foreach (Rectangle rectangle in rectangles) {
            int width = rectangle.toX - rectangle.fromX;
            int height = rectangle.toY - rectangle.fromY;

            for (int x = 1; x < width; x++) {
                for (int y = 1; y < height; y++) {
                    debugTex.SetPixel(rectangle.fromX + x, rectangle.fromY + y, rectangle.symbol);
                }
            }
        }

        File.WriteAllBytes(Application.dataPath + "/DebugPicture.png", debugTex.EncodeToPNG());
    }

    // Create rules for the protrusion of terminal regions
    private void checkTerminalProtrusion(Dictionary<Color,List<Rectangle>> rectangles) {

        foreach (Color c in rectangles.Keys) {
            // Take the first rectangle as anyone would suffice since they share the same Z value
            Rectangle rect = rectangles[c][0];
            if (rect.depth != 0) {
                writeDepthRule(getRegionName(new Region(rect)), rect.depth);
            }
        }

    }

    // Writes a protrusion rule to file
    private void writeDepthRule(string from, float size) {
        string rule = from + " -> protrude(Z) { " + size + " }";
        fileDS.WriteLine(rule);
    }
}
