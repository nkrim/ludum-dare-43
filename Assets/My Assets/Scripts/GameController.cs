using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    /* PUBLIC PROPERTIES */
    [Header("Game Settings")]
    public float similarityThreshold = 0.2f;
    public float startingLineupCount = 3.0f;
    public int maxLineupCount = 7;
    public float lineupIncrementation = 0.5f;
    [Header("Formatting/Positioning")]
    public float lineupWidth = 12f;

    /* REFERENCE VARS */
    RandomFaceGenerator gen;

    /* WRAPPER REFERENCE VARS */
    Transform originalWrapper;
    Transform lineupWrapper;
    /* WRAPPER REFERENCE NAMES */
    public static readonly string OriginalWrapperName = "original-wrapper";
    public static readonly string LineupWrapperName = "lineup-wrapper";

    /* INPUT STATE VARS */
    Face mouseDownTarget;

    /* GAME STATE VARS */
    bool game_active = false;
    int generation = 0;
    Face original_face;
    Face cur_face;

    /* PERSISTENT GAME STATE VARS */
    int highScore = 0;

	/* LIFECYCLE METHODS */
	void Awake () {
        // Link Generator
        gen = GetComponent<RandomFaceGenerator>();
        if (gen == null)
            Debug.LogError("GameController needs a RandomFaceGenerator component");
        // Link placement wrappers
        originalWrapper = transform.Find(OriginalWrapperName);
        lineupWrapper = transform.Find(LineupWrapperName);
        if (originalWrapper == null || lineupWrapper == null)
            Debug.LogError("GameController is missing one or more internal wrappers");
	}

    void Start () {
        StartGame();
    }

    private void Update () {
        if (!game_active)
            return;
        /* INPUT HANDLING */
        // Raycast Child Selection
        if (mouseDownTarget != null && Input.GetMouseButtonUp(0)) {
            Vector2 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(clickPoint, Vector2.zero, Mathf.Infinity, LayerMask.NameToLayer("lineup"));
            if (hit.transform != null) {
                if (mouseDownTarget == hit.transform.GetComponentInParent<Face>()) {
                    SelectChild(mouseDownTarget);
                }
            }
        }
        if (Input.GetMouseButtonDown(0)) {
            Vector2 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(clickPoint, Vector2.zero, Mathf.Infinity, LayerMask.NameToLayer("lineup"));
            if (hit.transform != null) {
                mouseDownTarget = hit.transform.GetComponentInParent<Face>();
            }
        }
    }


    /* LINEUP PROCEDURES */
    void BuildLineup (Face parent) {
        int numChildren = Mathf.Min(maxLineupCount, (int)(startingLineupCount + generation * lineupIncrementation));
        BuildLineup(parent, numChildren);
    }
    void BuildLineup (Face parent, int numChildren) {
        // Setup positioning
        float localLineupWidth = lineupWidth / lineupWrapper.transform.localScale.x;
        float splitWidth = localLineupWidth / numChildren;
        float firstPosition = (splitWidth - localLineupWidth) / 2;
        // Generate and place children
        for(int i=0; i<numChildren; i++) {
            Face face = gen.GenerateRandomFaceFromParent(parent);
            face.transform.SetParent(lineupWrapper, false);
            face.transform.localPosition += (firstPosition + (i * splitWidth)) * Vector3.right;
        }
    }
    void ClearLineup () {
        // TEMPORARY IMPLEMENTATION (NEEDS ANIMATION)
        Face[] children = lineupWrapper.GetComponentsInChildren<Face>();
        foreach (Face f in children)
            DestroyImmediate(f.gameObject);
    }

    void SelectChild (Face child) {
        // TEMPORARY IMPLEMENTATION
        if (cur_face == null)
            original_face.gameObject.SetActive(false);
        else
            DestroyImmediate(cur_face.gameObject);
        cur_face = child;
        child.transform.SetParent(originalWrapper, false);
        child.transform.localPosition = Vector3.zero;
        ClearLineup();
        generation++;
        BuildLineup(cur_face);

        // TEMPORARY TRIGGER
        float similarity = FaceSimilarity(original_face, cur_face);
        SetPercentageDisplay(similarity);

        if((similarity - similarityThreshold) <= 0.0001) {
            ResetGameState();
            StartGame();
        }
    }


    /* FACE COMPARISON */
    float FaceSimilarity (Face face1, Face face2) {
        List<int> face1_indices = face1.GetIndices();
        List<int> face2_indices = face2.GetIndices();
        int length = Mathf.Min(face1_indices.Count, face2_indices.Count);
        int num_equal = 0;
        for (int i = 0; i < length; i++) {
            if (face1_indices[i] == face2_indices[i])
                num_equal++;
        }
        return (float)num_equal / length;
    }


    /* UI CONTROL */
    void SetPercentageDisplay (float x) {
        var percentageDisplay = GameObject.FindWithTag("UI").transform.Find("percentage-display").GetComponent<TMPro.TextMeshProUGUI>();
        percentageDisplay.text = string.Format("{0:0.0%}", x);
    }


    /* CONTROLLING GAMESTATE */
    public void StartGame () {
        // Set game to active
        game_active = true;
        // Generate initial face
        original_face = gen.GenerateRandomFace();
        original_face.transform.SetParent(originalWrapper, false);
        // BuildLineup
        BuildLineup(original_face, 3);
    }

    public void ResetGameState () {
        // Clear game area
        ClearLineup();
        if (cur_face != null)
            DestroyImmediate(cur_face.gameObject);
        DestroyImmediate(original_face.gameObject);
        // Reset state vars
        game_active = false;
        generation = 0;
        original_face = null;
        cur_face = null;
        // Reset UI
        SetPercentageDisplay(1);
    }
}
