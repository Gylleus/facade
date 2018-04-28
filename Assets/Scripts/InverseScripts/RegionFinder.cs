using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Rectangle = RuleGenerator.Rectangle;

public static class RegionFinder {

    public static Dictionary<Color, List<Rectangle>> findRectangles(string terminalRegionsName, int width, int height) {
        byte[] bytes = File.ReadAllBytes(Application.dataPath + terminalRegionsName);
        Texture2D image = new Texture2D(width, height);
        image.LoadImage(bytes);

        bool[,] visited = new bool[image.width, image.height];


        Dictionary<Color, List<Rectangle>> rectangles = new Dictionary<Color, List<Rectangle>>();

        for (int x = 0; x < image.width; x++) {
            for (int y = 0; y < image.height; y++) {
                if (!visited[x, y]) {
                    Color c = image.GetPixel(x, y);
                    //      print(c);
                    if (!rectangles.ContainsKey(c)) {
                        rectangles.Add(c, new List<Rectangle>());
                        Debug.Log("New color" + c + " at: (" + x + "," + y + ")");
                    }

                    Rectangle newRec = new Rectangle();
                    newRec.fromX = x;
                    newRec.fromY = y;
                    newRec.symbol = c;

                    int indexPointer = x;

                    // While the color of the current pixel is equal to the rectangles color
                    while (indexPointer < image.width && image.GetPixel(indexPointer, y) == c && !visited[indexPointer, y]) {
                        indexPointer++;
                    }

                    newRec.toX = indexPointer - 1;

                    int currentY = y + 1;
                    indexPointer = x;

                    while (currentY < image.height && image.GetPixel(indexPointer, currentY) == c && !visited[indexPointer, currentY]) {
                        indexPointer++;
                        if (indexPointer > newRec.toX) {
                            indexPointer = x;
                            currentY++;
                        }
                    }

                    // -1 since the current color was evaluated to not be equal
                    newRec.toY = currentY - 1;

                    // Set all pixels of the rectangle to visited
                    for (int vX = newRec.fromX; vX <= newRec.toX; vX++) {
                        for (int vY = newRec.fromY; vY <= newRec.toY; vY++) {
                            visited[vX, vY] = true;
                        }
                    }

                    // Add the rectangle to list
                    rectangles[c].Add(newRec);
                }
            }
        }
        for (int x = 0; x < image.width; x++) {
            for (int y = 0; y < image.height; y++) {
                //       s += visited[x, y];
            }
        }
        return rectangles;
    }

    public static List<Region> findAllRegionCombinations(List<Rectangle> terminals) {

        Dictionary<Region, int> occurences = new Dictionary<Region, int>();
        List<Region> combinations = new List<Region>();
        
        foreach (Rectangle rect in terminals) {
            Region newRegion = new Region(rect);
            combinations.Add(newRegion);
            updateOccurences(occurences, newRegion);
        }

        bool merged = true;
        while (merged) {
            merged = false;
            Region mergedRegion = null, first = null, second = null;

            // Total search all existing regions if some can be combined
            foreach (Region current in combinations) {
                foreach (Region other in combinations) {
                    if (current == other) continue;

                    mergedRegion = RegionManager.tryMergeRegions(current, other);
                    if (mergedRegion != current) {
                        merged = true;
                        first = current;
                        second = other;
                        break;
                    }
                }
                if (merged) break;
            }
            // If we found a merge
            if (merged) {
                combinations.Remove(first);
                combinations.Remove(second);
                combinations.Add(mergedRegion);
                updateOccurences(occurences, mergedRegion);
            }
        }
        return combinations;
    }

    // Finds and returns areas that can be defined by repeats as a merged region
    public static List<Region> findRealRepeats(List<Region> splits, string axis) {

        
        List<Region> repeatAreas = new List<Region>();
        List<Region> notAddedRepeats = new List<Region>();

        // Get a list of all split regions that appear at least twice
        List<Region> duplicates = RegionManager.getRegionDuplicates(splits);
        List<Region> repeatedSplits = new List<Region>(duplicates);

        // Fix when the original non-merged regions of splits can be repeated

        while (duplicates.Count > 0) {

            List<Region> mergedRegions = new List<Region>();

            // Try to merge all regions in all four directions (which only be two due to the list being splits)
            foreach (Region current in duplicates) {
                Region mergedRegion = RegionManager.mergeOnAxis(current, repeatedSplits, axis);
                // If the merge was successful
                if (mergedRegion.terminals.Count > current.terminals.Count) {
                    mergedRegions.Add(mergedRegion);
                }
            }

            duplicates = RegionManager.getRegionDuplicates(mergedRegions);
            List<Region> mergedDuplicates = RegionManager.mergeDuplicateRegions(duplicates);
            List<Region> newAreas = new List<Region>();

            // Now to check if we had any successful merges and then add them to the repeat list
            foreach (Region r in mergedDuplicates) {
                if (!duplicates.Contains(r)) {
                    repeatAreas.Add(r);
                }
            }

            // Check again for duplicates as we have merged now
            duplicates = RegionManager.getRegionDuplicates(mergedDuplicates);
            
        }

        // Now to clean up the repeat list of duplicates and subareas
        List<Region> uniqueRepeats = new List<Region>();
        foreach (Region current in repeatAreas) {
            bool isSubarea = false;
            bool exists = false;
            foreach (Region other in repeatAreas) {
                if (current.terminals.Count < other.terminals.Count && other.containsRegion(current)) {
                    isSubarea = true;
                }
            }
            // Check if we have already added such a region to our list
            foreach (Region rep in uniqueRepeats) {
                if (rep.equalTerminals(current)) {
                    exists = true;
                }
            }
            if (!isSubarea && !exists) {
                uniqueRepeats.Add(current);
            }
        }

        return uniqueRepeats;
    }
    

    private static void updateOccurences(Dictionary<Region,int> occurences, Region reg) {

        bool exists = false;

        foreach (Region other in occurences.Keys) {
            if (other.equalTerminals(other)) {
                exists = true;
                occurences[other]++;
            }
        }

        if (!exists) {
            occurences.Add(reg, 1);
        }

    }

    public static List<Region> findRepeats(List<Region> regions) {


        // Need to find the largest area of repeats for each terminal
        Dictionary<Color, List<Rectangle>> terminalList = new Dictionary<Color, List<Rectangle>>();

        foreach (Region r in regions) {
            foreach (Rectangle rect in r.terminals) {
                if (!terminalList.ContainsKey(rect.symbol)) {
                    terminalList.Add(rect.symbol, new List<Rectangle>());
                }
                terminalList[rect.symbol].Add(rect);
            }
        }
        return findRepeats(terminalList);
    }

    public static List<Region> findRepeats(Dictionary<Color,List<Rectangle>> terminals) {

        List<Rectangle> repeatedTerminals = new List<Rectangle>();
        List<Region> allMergedRegions = new List<Region>();

        // Find all terminals that are repeated at least twice
        foreach (Color c in terminals.Keys) {
            if (terminals[c].Count > 1) {
                repeatedTerminals.AddRange(terminals[c]);
            }
        }
        return findAllRegionCombinations(repeatedTerminals);
        // Total search all possible merges of repeating terminals
        foreach (Rectangle fromTerminal in repeatedTerminals) {
            // Initialize a new region starting from the current terminal
            Region mergingRegion = new Region(fromTerminal);

            // Create a copy of all terminal regions to check which we can merge with
            List<Rectangle> notMerged = new List<Rectangle>(repeatedTerminals);
            notMerged.Remove(fromTerminal);

            bool merged = true;
            // While we can still merge with another repeated terminal region
            while(merged) {
                merged = false;
                int terminalsBefore = mergingRegion.terminals.Count;
               
                mergingRegion = tryMerge(mergingRegion, notMerged);
               // If successfully merged 
                if (mergingRegion.terminals.Count > terminalsBefore) {
                    merged = true;
                    // Remove the last added terminal from the list of unmerged terminals
                    notMerged.Remove(mergingRegion.terminals[mergingRegion.terminals.Count - 1]);
                    // Can optimize by placing correctly when adding the new terminal region
                    mergingRegion.sortTerminals();
                    allMergedRegions.Add(new Region(mergingRegion));
                }
            }
        }

        // allMergedRegions now should contain all possible merged regions
        allMergedRegions = allMergedRegions.OrderBy(o => o.terminals.Count).ToList();
        List<Region> distinctMergedRegions = new List<Region>();

        // Remove all duplicate entries of the same region (same terminals with same position)
        foreach(Region current in allMergedRegions) {
            bool exists = false;
            foreach (Region other in distinctMergedRegions) {
                // If the same region already exists
                if (current.equals(other)) {
                    exists = true;
                    break;
                }
            }
            if (!exists) {
                distinctMergedRegions.Add(current);
            }
        }
        // Finally check which distinct merged regions are connected
        return distinctMergedRegions;
    }


    private static Region tryMerge(Region r, List<Rectangle> otherRegions) {
        Region newRegion = new Region(r);
        
        foreach (Rectangle other in otherRegions) {
            // Try merge up
            if (newRegion.fromX == other.fromX && newRegion.toX == other.toX && newRegion.toY == other.fromY - 1) {
                newRegion.toY = other.toY;
                newRegion.terminals.Add(other);
                return newRegion;
            }
            // Try merge down
            if (newRegion.fromX == other.fromX && newRegion.toX == other.toX && other.toY == newRegion.fromY - 1) {
                newRegion.fromY = other.fromY;
                newRegion.terminals.Add(other);
                return newRegion;
            }
            // Try merge right
            if (newRegion.toX == other.fromX - 1 && newRegion.fromY == other.fromY && newRegion.toY == other.toY) {
                newRegion.toX = other.toX;
                newRegion.terminals.Add(other);
                return newRegion;
            }
            // Try merge left
            if (other.toX == newRegion.fromX - 1 && newRegion.fromY == other.fromY && newRegion.toY == other.toY) {
                newRegion.fromX = other.fromX;
                return newRegion;
            }
        }
        return newRegion;
    }

    // Creates a list of the splits of a region on the Y-axis
    // If successful return the list, if not return null
    public static List<Region> createSplitVertical(Region r) {
        List<Region> splits = new List<Region>();
        Region cuttingRegion = new Region(r);
        // While we still have terminals remaining and the last cut was successful
        while (cuttingRegion.terminals.Count > 0) {
            Region newRegion = createRegionVertical(cuttingRegion);
            if (newRegion == null) {
                // The split was unsuccesful meaning the region can not be split on this axis
                return null;
            } else {
                splits.Add(newRegion);
                cuttingRegion.cutY(newRegion.toY+1, r.toY);
            }
        }

        // If the split only resulted in one area
        if (splits.Count == 1) {
            return null;
        }

        // If the shape was split correctly there should be 0 terminals not assigned to any area and we can return
        return splits;
    }

    // Creates a list of the splits of a region on the X-axis
    // If successful return the list, if not return null
    public static List<Region> createSplitHorizontal(Region r) {
        List<Region> splits = new List<Region>();
        Region cuttingRegion = new Region(r);

        // While we still have terminals remaining and the last cut was successful
        while (cuttingRegion.terminals.Count > 0) {
            Region newRegion = createRegionHorizontal(cuttingRegion);
            if (newRegion == null) {
                // The split was unsuccesful meaning the region can not be split on this axis
                return null;
            }
            else {
                splits.Add(newRegion);
                cuttingRegion.cutX(newRegion.toX+1, r.toX);
            }
        }

        // If the split only resulted in one area
        if (splits.Count == 1) {
            return null;
        }

        // If the shape was split correctly there should be 0 terminals not assigned to any area and we can return
        return splits;
    }

    // Tries to create a vertical region starting from the bottom-left of the area
    public static Region createRegionHorizontal(Region region) {
        region.sortTerminals();

        int toX = -1;

        // Try to find a line starting from Y = 'from'
        for (int i = 0; i < region.terminals.Count; i++) {
            Rectangle rect = region.terminals[i];
            if (rect.fromY == region.fromY) {
                // We can start to draw a line from here
                Rectangle current = rect;

                bool nextRectangleExists = true;
                // While there exists a rectangle that can continue the line or we have reached the end
                while (nextRectangleExists && current.toY != region.toY) {
                    nextRectangleExists = false;

                    foreach (Rectangle next in region.terminals) {
                        // If the rectangles are Y-aligned and are adjacent
                        if (current != next && next.fromY-1 == current.toY && next.toX == current.toX) {
                            current = next;
                            nextRectangleExists = true;
                            break;
                        }
                    }
                }

                // If the generated line succesfully stretches from 'from' to 'to' then we have created a valid line and can end the loop
                if (current.toY == region.toY) {
                    toX = current.toX;
                    break;
                }
            }
        }

        if (toX == -1) {
            // Algorithm was unable to draw a vertical line on the shape
            return null;
        }

        List<Rectangle> areaTerminals = new List<Rectangle>();

        // We now have a valid line of rectangles but would like to fill in the potential gaps of the area
        foreach (Rectangle rect in region.terminals) {
            // Add all regions below the line to our new area list
            if (rect.toX <= toX) {
                areaTerminals.Add(rect);
            }
        }

        return new Region(areaTerminals);

    }

    // Tries to create a horizontal region starting from the bottom-left of the area
    public static Region createRegionVertical(Region region) {
        region.sortTerminals();
        int toY = -1;
       
        // Try to find a line starting from X = 'from'
        for (int i = 0; i < region.terminals.Count; i++) {
            Rectangle rect = region.terminals[i];
            if (rect.fromX == region.fromX) {
                // We can start to draw a line from here
                Rectangle current = rect;

                bool nextRectangleExists = true;
                 // While there exists a rectangle that can continue the line or we have reached the end
                while (nextRectangleExists && current.toX != region.toX) {
                    nextRectangleExists = false;
                    foreach (Rectangle next in region.terminals) {
                        // If the rectangles are Y-aligned and are adjacent
                        if (current != next && next.fromX-1 == current.toX && next.toY == current.toY) {
                 //           stop(current,next);
                            current = next;
                            nextRectangleExists = true;
                            break;
                        }
                    }
                }

                // If the generated line succesfully stretches from 'from' to 'to' then we have created a valid line and can end the loop
                if (current.toX == region.toX) {
                    toY = current.toY;
                    break;
                }
            }
        }

        if (toY == -1) {
            // Algorithm was unable to draw a horizontal line on the shape
            return null;
        }

        List<Rectangle> areaTerminals = new List<Rectangle>();

        // We now have a valid line of rectangles but would like to fill in the potential gaps of the area
        foreach (Rectangle rect in region.terminals) {
            // Add all regions below the line to our new area list
            if (rect.toY <= toY) {
                areaTerminals.Add(rect);
            }
        }

        return new Region(areaTerminals);
    }
}
