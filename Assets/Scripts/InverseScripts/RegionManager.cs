using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RegionManager {

    private delegate bool CanMerge(Region first, Region second);


    // Sorts regions by axis by their from value on respective axes
    public static List<Region> sortRegions(List<Region> regions, string axis) {
        List<Region> sorted = new List<Region>();

        // Add all regions to the sorted list
        foreach (Region reg in regions) {
            // Base case
            if (sorted.Count == 0) {
                sorted.Add(reg);
            }
            else if ((axis.ToLower() == "x" && reg.fromX < sorted[0].fromX) || (axis.ToLower() == "y" && reg.fromY < sorted[0].fromY)) {
                sorted.Insert(0, reg);
            }
            else if ((axis.ToLower() == "x" && reg.fromX > sorted[sorted.Count - 1].fromX) || (axis.ToLower() == "y" && reg.fromY > sorted[sorted.Count - 1].fromY)) {
                sorted.Add(reg);
            }
            else {
                for (int i = 0; i < sorted.Count; i++) {
                    Region other = sorted[i];
                    if (axis.ToLower() == "x" && reg.fromX < other.fromX) {
                        sorted.Insert(i, reg);
                        break;
                    }
                    if (axis.ToLower() == "y" && reg.fromY < other.fromY) {
                        sorted.Insert(i, reg);
                        break;
                    }
                }
            }
        }
        return sorted;
    }


    public static Region tryMergeRegions(Region first, Region second) {
        if (canMerge(first, second)) {
            List<RuleGenerator.Rectangle> mergedRegionTerminals = new List<RuleGenerator.Rectangle>(first.terminals);
            mergedRegionTerminals.AddRange(second.terminals);
            return new Region(mergedRegionTerminals);
        }
        else {
            return first;
        }
    }

    // Merge up on Y-axis, Right on X-axis for deterministic repeat finding
    public static Region mergeOnAxis(Region from, List<Region> others, string axis) {

        CanMerge mergeAllowed;

        Region first = new Region(from);

        if (axis.ToLower() == "x") {
            mergeAllowed = canMergeRight;
        } else if (axis.ToLower() == "y") {
            mergeAllowed = canMergeUp;
        } else {
            Debug.LogError("Invalid axis for merge.");
            return first;
        }

        foreach (Region other in others) {
            if (first != other && mergeAllowed(first, other)) {
                first.absorb(other);
                return first;
            }
        }
        return first;
    }

    public static bool canMergeUp(Region first, Region second) {
        return (first.fromX == second.fromX && first.toX == second.toX && first.toY == second.fromY - 1);
    }

    public static bool canMergeDown(Region first, Region second) {
        return (first.fromX == second.fromX && first.toX == second.toX && second.toY == first.fromY - 1);
    }

    public static bool canMergeRight(Region first, Region second) {
        return (first.fromY == second.fromY && first.toY == second.toY && first.toX == second.fromX - 1);
    }

    public static bool canMergeLeft(Region first, Region second) {
        return (first.fromY == second.fromY && first.toY == second.toY && second.toX == first.fromX - 1);
    }
    public static bool canMerge(Region first, Region second) {
        return (canMergeUp(first,second) || canMergeDown(first, second) || canMergeRight(first, second) || canMergeLeft(first, second));
    }

    /// <summary>
    /// Tries to merge with regions to the left, bottom, right and up at the same time
    /// </summary>
    /// <param name="toMerge">Region that will be attempted to expand.</param>
    /// <param name="otherRegions">List of regions to possibly merge with.</param>
    /// <returns>Merged region.</returns>
    public static Region mergeAllDirections(Region toMerge, List<Region> otherRegions) {

        List<Region> toMergeWith = new List<Region>();

        foreach (Region other in otherRegions) {
            // We can check all directions at the same time as there would only be one direction for merging at a time with one region per direction maximum
            if (canMerge(toMerge, other)) {
                toMergeWith.Add(other);
            }
        }
        List<RuleGenerator.Rectangle> mergedTerminals = new List<RuleGenerator.Rectangle>();
        mergedTerminals.AddRange(toMerge.terminals);
        foreach (Region other in toMergeWith) {
            mergedTerminals.AddRange(other.terminals);
        }
        return new Region(mergedTerminals);
    }

    /// <summary>
    /// Returns a list of region that have an identical counterpart in terms of terminal regions contained.
    /// </summary>
    /// <param name="regionList">List of regions to check.</param>
    /// <returns></returns>
    public static List<Region> getRegionDuplicates(List<Region> regionList) {
        List<Region> duplicates = new List<Region>();

        foreach (Region current in regionList) {
            foreach (Region other in regionList) {
                if (current != other && current.equalTerminals(other) && !current.equals(other)) {
                    duplicates.Add(new Region(current));
                    break;
                }
            }
        }

        return duplicates;
    }

    /// <summary>
    /// Merges all regions that are identical to one another.
    /// </summary>
    /// <param name="regionList">List of regions to merge.</param>
    /// <returns>List of regions where all duplicate regions have been merged.</returns>
    public static List<Region> mergeDuplicateRegions(List<Region> regionList) {

        List<Region> mergedList = new List<Region>(regionList);

        bool merged = true;

        // Try to merge as many of the identical regions as possible
        while (merged) {
            merged = false;

            Region from = null;
            List<Region> toRemove = new List<Region>();
            Region tmpMerge = null;

            foreach (Region current in mergedList) {
                tmpMerge = new Region(current);
                from = current;

                bool absorbed = true;

                while (absorbed) {
                    absorbed = false;
                    foreach (Region other in mergedList) {
                        if (current != other && canMerge(tmpMerge, other) && current.equalTerminals(other)) {
                            tmpMerge.absorb(other);
                            toRemove.Add(other);
                            merged = true;
                            absorbed = true;
                            // Redo iteration as some element we previously saw might be mergable now
                            break;
                        }
                    }
                }
                
                if (merged) break;
            }
            // If we found a merge we remove the two regions and add the combined one to our identical regions list
            if (merged) {
                mergedList.Add(tmpMerge);
                mergedList.Remove(from);
                // Remove all regions that share a terminal with the merge
                foreach (Region region in mergedList) {
                    if (region != tmpMerge && tmpMerge.overlaps(region)) {
                        toRemove.Add(region);
                    }
                }
                foreach (Region inMerge in toRemove) {
                    mergedList.Remove(inMerge);
                }

            }
        }
        return mergedList;
    }

}




