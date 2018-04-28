using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Presenter : MonoBehaviour {

    // The names for the buildings that will be presented
    public GameObject[] buildings;
    public int buildingTypes;

    public int cycles = 2;

    private int buildingsPerView = 1;

    public Vector3 buildingSpawn;

    public string resultFilePath;

    private int currentBuildingIndex = 0;
    private List<GameObject> buildingsLeft;
    private GameObject currentBuilding;
    private RoofCreator rc;

    public int stage = 0;
    GameObject[,] hypos = new GameObject[3, 2];
    private int hypoIndex = -1;
    public List<GameObject> currentHypos;

    GameObject[,] colorReal = new GameObject[3, 2];
    private int colorIndex = -1;
    public List<GameObject> currentCR;

    public Text buildingNameText;
    public Text instructionText;

	// Use this for initialization
	void Start () {
        resultFilePath = Application.dataPath + resultFilePath;
        writeToResultFile(System.Environment.NewLine + " --- NEW SESSION ---");
        setHypos();
        setCR();

        rc = GetComponent<RoofCreator>();
        buildingsLeft = new List<GameObject>();
		if (buildings.Length % buildingsPerView != 0) {
            Debug.LogError("Amount of buildings not divisible by amount of buildings per view.");
        } else {
            createNextBuildings();
        }
	}

    private void setCR() {
        currentCR = new List<GameObject>();
        colorReal[0, 0] = buildings[0];
        colorReal[0, 1] = buildings[3];
        colorReal[1, 0] = buildings[4];
        colorReal[1, 1] = buildings[7];
        colorReal[2, 0] = buildings[8];
        colorReal[2, 1] = buildings[11];
    }

    private void setHypos() {
        currentHypos = new List<GameObject>();
        hypos[0, 0] = buildings[2];
        hypos[0, 1] = buildings[3];
        hypos[1, 0] = buildings[6];
        hypos[1, 1] = buildings[7];
        hypos[2, 0] = buildings[10];
        hypos[2, 1] = buildings[11];
    }

    public void chooseRealBuilding() {
        submitBuilding("REAL");
    }

    public void chooseFakeBuilding() {
        submitBuilding("NOT REAL");
    }

    private void submitBuilding(string verdict) {
        if (currentBuilding != null) {
            string resultLine = "BUILDING: " + currentBuilding.name + "\t VERDICT: " + verdict;
            writeToResultFile(resultLine);
        }
        createNextBuildings();
    }

    private void writeToResultFile(string line) {
        File.AppendAllText(resultFilePath, line + System.Environment.NewLine);
    }

    public List<GameObject> getBuildingOrder(GameObject[] buildingList, int differentBuildings) {
        int versions = buildingList.Length / differentBuildings;
        List<GameObject>[] indices = new List<GameObject>[differentBuildings];
        List<GameObject> buildingsOrder = new List<GameObject>();

        for (int i = 0; i < differentBuildings; i++) {
            indices[i] = new List<GameObject>();
            for (int j = 0; j < versions; j++) {
                indices[i].Add(buildingList[i * versions + j]);
            }
        }

        for (int i = 0; i < versions; i++) {
            for (int j = 0; j < differentBuildings; j++) {
                List<GameObject> from = indices[j];
                int index = Random.Range(0, from.Count);
                buildingsOrder.Add(from[index]);
                from.RemoveAt(index);
            }
        }
        return buildingsOrder;
    }

    /*
     * Creates the next set of buildings
     */
    public void createNextBuildings() {
        if (stage == 0) {
            if (currentBuilding != null) {
                Destroy(currentBuilding);
            }

            if (buildingsLeft.Count == 0) {
                if (cycles > 0) {
                    buildingsLeft = getBuildingOrder(buildings, buildingTypes);
                    cycles--;
                }
                else {
                    stage++;
                    createNextBuildings();
                    return;
                }
            }
            
            int index = 0;
            GameObject newHouse = buildingsLeft[index];
            buildingsLeft.Remove(newHouse);
            instructionText.text = "Real building or not?";

            currentBuilding = Instantiate(newHouse, buildingSpawn + new Vector3(0, newHouse.transform.localScale.y / 2, 0), Quaternion.identity);
            currentBuilding.name = newHouse.name;
            buildingNameText.text = currentBuilding.GetComponent<NameHolder>().tagName;
            rc.createRoofs(new GameObject[] { currentBuilding });
        }
        else if (stage == 1) {

            foreach (GameObject h in currentHypos) {
                Destroy(h);
            }
            currentHypos.Clear();
            if (hypoIndex < hypos.Rank) {
                hypoIndex++;
            } else {
                stage++;
                currentBuilding = null;
                createNextBuildings();
                return;
            }

            List<float> indices = new List<float>();
            indices.Add(10);
            indices.Insert(Random.Range(0, 2), -10);
            instructionText.text = "Which building is most realistic?";

            GameObject hypReal = hypos[hypoIndex, 0];
            GameObject hypUnreal = hypos[hypoIndex, 1];
            GameObject HR = Instantiate(hypReal, buildingSpawn + new Vector3(indices[0], hypReal.transform.localScale.y / 2, 0), Quaternion.identity);
            HR.name = hypReal.name;
            GameObject HUR = Instantiate(hypUnreal, buildingSpawn + new Vector3(indices[1], hypUnreal.transform.localScale.y / 2, 0), Quaternion.identity);
            HUR.name = hypUnreal.name;
            rc.createRoofs(new GameObject[] { HUR, HR });
            currentHypos.Add(HR);
            currentHypos.Add(HUR);
        }
        else {
            Application.Quit();
        }
    }
    
	// Update is called once per frame
	void Update () {
	    	
	}
}
