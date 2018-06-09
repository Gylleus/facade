using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using Rectangle = RuleGenerator.Rectangle;

public class InputManager : MonoBehaviour {

    public Text buildingNameText, widthText, heightText;
    public InputField nameField, zField;

    
    public GameObject facadePreviewHolder;
    private Image facadePreview;
    public GameObject layoutPreviewHolder;
    private Image layoutPreview;
    public GameObject colorBox;
    public GameObject promptBox;
    public Button nextButton;
    public RuleGenerator ruleGen;


    // Private variables
    private StreamWriter fileDS;

    private GameObject[] facades;
    private int nextFacadeIndex = 0;
    private Queue<Rectangle> currentFacadeRectangles;
    private Dictionary<Color, List<Rectangle>> allCurrentFacadeRectangles;
    private List<Rectangle> informationRects;
    private Rectangle currentRectangle;

    void Start() {
        promptBox.SetActive(true);
        facadePreviewHolder.SetActive(true);
        facadePreview = facadePreviewHolder.GetComponent<Image>();
        layoutPreviewHolder.SetActive(true);
        layoutPreview = layoutPreviewHolder.GetComponent<Image>();

        if (fileDS == null) {
            fileDS = File.AppendText(Application.dataPath + "/" + ruleGen.outputFileName);
        }

        facades = GameObject.FindGameObjectsWithTag("Facade");
        nextFacade();
        nextRectangle();
        updateInformationBox(currentRectangle);
    }

    // Reads in all information of a facade
    public List<Rectangle> initializeNewFacade(InputFacade facade) {
        allCurrentFacadeRectangles = RegionFinder.findRectangles(facade.facadeLayoutName, facade.inputFacade.width, facade.inputFacade.height);
        float maxY = 0; float maxX = 0;

        foreach (Color c in allCurrentFacadeRectangles.Keys) {
            foreach (Rectangle r in allCurrentFacadeRectangles[c]) {
                maxX = Mathf.Max(maxX, r.toX);
                maxY = Mathf.Max(maxY, r.toY);
            }
        }

        buildingNameText.text = facade.gameObject.name;
        widthText.text = facade.getBuildingWidth().ToString();
        heightText.text = facade.getBuildingHeight().ToString();
        Vector3 previewScale = facadePreview.rectTransform.localScale;
        facadePreview.rectTransform.localScale = new Vector3(previewScale.y * facade.getBuildingWidth() / facade.getBuildingHeight(), previewScale.y, 1);
        layoutPreview.rectTransform.localScale = facadePreview.rectTransform.localScale;
        foreach (Color c in allCurrentFacadeRectangles.Keys) {
            Rectangle newRect = new Rectangle();
            newRect.symbol = c;
            currentFacadeRectangles.Enqueue(newRect);
        }
        return null;
    }

    void Update() {
        // If we have processed all facades, terminate
        if (nextFacadeIndex > facades.Length) {
            if (fileDS != null) {
                fileDS.Close();
            }
    #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
    #endif
        }
    }

    // Clears all information of the current facade and initializes variables for the next
    // Returns true if there was a next facade, false if not
    private bool nextFacade() {
        nextFacadeIndex++;
        if (nextFacadeIndex <= facades.Length) {
            // Clear all information
            informationRects = new List<Rectangle>();
            currentFacadeRectangles = new Queue<Rectangle>();
            currentRectangle = null;
            // Initialize the new facade
            InputFacade iF = facades[nextFacadeIndex - 1].GetComponent<InputFacade>();
            initializeNewFacade(iF); 
            Texture2D layoutTex = readFacadeImage(Application.dataPath + iF.facadeLayoutName);
            facadePreview.sprite = Sprite.Create(iF.inputFacade, new Rect(0, 0, iF.inputFacade.width, iF.inputFacade.height), facadePreview.sprite.pivot);
            layoutPreview.sprite = Sprite.Create(layoutTex, new Rect(0, 0, layoutTex.width, layoutTex.height), facadePreview.sprite.pivot);
            // Go to the next facade index
            return true;
        }
        return false;
    }

    // Chooses the next rectangle and invokes rule generation if it is the last one
    private void nextRectangle() {
        // If we should proceed to the next rectangle
        if (currentFacadeRectangles.Count <= 0) {
            // Update all rectangles with our given information
            foreach (Rectangle infoR in informationRects) {
                updateRectangles(infoR);
            }

            // Start generation from the given rectangles
            ruleGen.beginGeneration(allCurrentFacadeRectangles, facades[nextFacadeIndex-1].GetComponent<InputFacade>(), fileDS);
            fileDS.WriteLine();
            fileDS.Flush();
            if (!nextFacade()) {
                return;
            }
        } 
        currentRectangle = currentFacadeRectangles.Dequeue();
    }

    private void updateInformationBox(Rectangle rect) {
        nameField.text = "";
        zField.text = "";
        colorBox.GetComponent<Image>().color = rect.symbol;
    }

    // Goes a step in the generation interface, choosing the next rectangle or going to the next facade
    public void updateNext() {
        Rectangle infoRect = new Rectangle();
        infoRect.name = nameField.text;
        float.TryParse(zField.text, out infoRect.depth);
        infoRect.symbol = currentRectangle.symbol;
        informationRects.Add(infoRect);

        // Go to next rectangle
        nextRectangle();
        updateInformationBox(currentRectangle);

    }

    // Updates all rectangles of one color
    private void updateRectangles(Rectangle infoR) {
        foreach (Rectangle toUpdate in allCurrentFacadeRectangles[infoR.symbol]) {
            toUpdate.depth = infoR.depth;
            toUpdate.name = infoR.name;
        }
    }

    private Texture2D readFacadeImage(string path) {
        byte[] f = File.ReadAllBytes(path);
        Texture2D imageTex = new Texture2D(1, 1);
        imageTex.LoadImage(f);
        return imageTex;
    }


}
