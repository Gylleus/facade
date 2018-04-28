using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rectangle = RuleGenerator.Rectangle;

// Definition of a Region class
// A region contains its position and size, as well as information of its subregions and possibly terminal value
public class Region {
    public int fromX, fromY, toX, toY;
    public string name;

    public List<Rectangle> terminals;
    public List<Region> subregions;

    public Region(int fx, int fy, int tx, int ty) {
        fromX = fx;
        fromY = fy;
        toX = tx;
        toY = ty;
        terminals = null;
        subregions = new List<Region>();
    }

    public Region(Rectangle rectangle) {
        fromX = rectangle.fromX;
        fromY = rectangle.fromY;
        toX = rectangle.toX;
        toY = rectangle.toY;
        terminals = new List<Rectangle>();
        terminals.Add(rectangle);
        subregions = new List<Region>();
    }

    // Creates a new region from a list of terminals, sorts them and sets bounds accordingly
    public Region(List<Rectangle> newTerminals) {
        terminals = new List<Rectangle>(newTerminals);
        sortTerminals();
        setBounds();
       subregions = new List<Region>();
    }

    public Region(Region other) {
        fromX = other.fromX;
        fromY = other.fromY;
        toX = other.toX;
        toY = other.toY;
        terminals = new List<Rectangle>(other.terminals);
        subregions = new List<Region>(other.subregions);
    }

    // Sorts the terminal regions according to their position
    // From lowest X, if X1=X2 then look at Y
    public void sortTerminals() {

        List<Rectangle> sortedList = new List<Rectangle>();

        foreach (Rectangle toInsert in terminals) {

            if (sortedList.Count == 0) {
                sortedList.Add(toInsert);
            } else {
                // If it's the smallest element
                if (toInsert.fromX < sortedList[0].fromX || (toInsert.fromX == sortedList[0].fromX && toInsert.fromY < sortedList[0].fromY)) {
                    sortedList.Insert(0, toInsert);
                }
                else if (toInsert.fromX > sortedList[sortedList.Count-1].fromX || (toInsert.fromX == sortedList[sortedList.Count - 1].fromX && toInsert.fromY > sortedList[sortedList.Count - 1].fromY)) {
                    sortedList.Add(toInsert);
                }   else {
                    // Go through all elements until it is the biggest
                    for (int i = 0; i < sortedList.Count; i++) {

                        Rectangle other = sortedList[i];

                        if (toInsert.fromX < other.fromX || (toInsert.fromX == other.fromX && toInsert.fromY < other.fromY)) {
                            sortedList.Insert(i, toInsert);
                            break;
                        }
                    }
                }
            }
        }
        terminals = sortedList;
    }

    // Checks if two regions contains the identical regions in regards to position and type
    public bool equals(Region other) {

        if (terminals.Count != other.terminals.Count) {
            return false;
        }

        for (int i = 0; i < terminals.Count; i++) {
            // If the two terminals start at different coordinates then the regions can not be equal
            if (terminals[i].fromX != other.terminals[i].fromX || terminals[i].fromY != other.terminals[i].fromY) {
                return false;
            }
        }

        return true;
    }

    // Iterates over the contained terminal regions and sets the bounds of the region accordingly
    public void setBounds() {

        fromX = int.MaxValue; fromY = int.MaxValue;
        toX = int.MinValue; toY = int.MinValue;

        foreach (Rectangle r in terminals) {
            fromX = Mathf.Min(r.fromX, fromX);
            fromY = Mathf.Min(r.fromY, fromY);
            toX = Mathf.Max(r.toX, toX);
            toY = Mathf.Max(r.toY, toY);
        }

    }

    public void cutY(int newFrom, int newTo) {
        List<Rectangle> toRemove = new List<Rectangle>();
        foreach (Rectangle rect in terminals) {
            if (rect.fromY < newFrom || rect.toY > newTo) {
                toRemove.Add(rect);
            }
        }
        foreach (Rectangle rect in toRemove) {
            terminals.Remove(rect);
        }
        sortTerminals();
        setBounds();
    }

    public void cutX(int newFrom, int newTo) {
        List<Rectangle> toRemove = new List<Rectangle>();
        foreach (Rectangle rect in terminals) {
            if (rect.fromX < newFrom || rect.toX > newTo) {
                toRemove.Add(rect);
            }
        }
        foreach (Rectangle rect in toRemove) {
            terminals.Remove(rect);
        }
        sortTerminals();
        setBounds();
    }

    // Checks if two regions contain the same terminal regions in the same structure
    public bool equalTerminals(Region other) {

        if (terminals.Count != other.terminals.Count) {
            return false;
        }

        for (int i = 0; i < terminals.Count; i++) {
            if (terminals[i].symbol != other.terminals[i].symbol) {
                return false;
            }
        }
        return true;
    }

    public void debugPrintRegion() {
        Debug.Log("Region contains: ");
        sortTerminals();
        Debug.Log(terminals.Count + " terminal regions.");
        Debug.Log("From: (" + fromX + "," + fromY + ")" + " - To: (" + toX + "," + toY + ")");
        /*  for (int i = 0; i < terminals.Count; i++) {
            Rectangle r = terminals[i];
            //    Debug.Log("Rectangle: Color - " + r.symbol);
            r.debugPrint();
        }*/
    }

    // Checks if this region contains all the terminal regions of another region
    public bool containsRegion(Region other) {

        if (terminals.Count < other.terminals.Count) {
            return false;
        }

        foreach (Rectangle rect in other.terminals) {
            if (!terminals.Contains(rect)) {
                return false;
            }
        }
        return true;
    }

    public void absorb(Region other) {
        terminals.AddRange(other.terminals);
        fromX = Mathf.Min(fromX, other.fromX);
        fromY = Mathf.Min(fromY, other.fromY);
        toX = Mathf.Max(toX, other.toX);
        toY = Mathf.Max(toY, other.toY);
    }

    public bool overlaps(Region other) {
        foreach (Rectangle currentR in terminals) {
            foreach (Rectangle otherR in other.terminals) {
                if (currentR.fromX == otherR.fromX && currentR.fromY == otherR.fromY && currentR.toX == otherR.toX && currentR.toY == otherR.toY) {
                    return true;
                }
            }
        }
        return false;
    }

}

