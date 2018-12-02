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
    Body mouseDownTarget;

    /* GAME STATE VARS */
    bool game_active = false;
    int generation = 0;
    Body original_body;
    Body cur_body;
    Body hover_body;

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
        Vector2 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hitLineupBody = Physics2D.Raycast(clickPoint, Vector2.zero, Mathf.Infinity, LayerMask.NameToLayer("lineup"));
        // Raycast hover focus
        bool hovering = false;
        if (hitLineupBody.transform) {
            Body hitBody = GetBody(hitLineupBody.transform);
            if (hitBody && hitBody != cur_body && hitBody != original_body) {
                hovering = true;
                if (hitBody != hover_body) {
                    if (hover_body)
                        hover_body.SetFocusState(false);
                    hover_body = hitBody;
                    hover_body.SetFocusState(true);
                }
            }
        }
        if (!hovering && hover_body) {
            hover_body.SetFocusState(false);
            hover_body = null;
        }
        // Raycast Child Selection
        if (mouseDownTarget != null && Input.GetMouseButtonUp(0)) {
            if (hitLineupBody.transform != null) {
                if (mouseDownTarget == GetBody(hitLineupBody.transform)) {
                    SelectChild(mouseDownTarget);
                }
            }
            mouseDownTarget = null;
        }
        if (Input.GetMouseButtonDown(0)) {
            if (hitLineupBody.transform != null) {
                mouseDownTarget = GetBody(hitLineupBody.transform);
                if (mouseDownTarget == cur_body || mouseDownTarget == original_body)
                    mouseDownTarget = null;
            }
        }
    }


    /* LINEUP PROCEDURES */
    void BuildLineup (Body parent) {
        int numChildren = Mathf.Min(maxLineupCount, (int)(startingLineupCount + generation * lineupIncrementation));
        BuildLineup(parent, numChildren);
    }
    void BuildLineup (Body parent, int numChildren) {
        // Setup positioning
        float localLineupWidth = lineupWidth / lineupWrapper.transform.localScale.x;
        float splitWidth = localLineupWidth / numChildren;
        float firstPosition = (splitWidth - localLineupWidth) / 2;
        // Generate and place children
        for(int i=0; i<numChildren; i++) {
            Body body = gen.GenerateRandomBodyFromParent(parent);
            body.transform.SetParent(lineupWrapper, false);
            body.transform.localPosition += (firstPosition + (i * splitWidth)) * Vector3.right;
        }
    }
    void ClearLineup () {
        // TEMPORARY IMPLEMENTATION (NEEDS ANIMATION)
        Body[] children = lineupWrapper.GetComponentsInChildren<Body>();
        foreach (Body b in children)
            DestroyImmediate(b.gameObject);
    }

    void SelectChild (Body child) {
        // REMOVE HOVERING BEFORE MODIFYING ANYTHING
        if (hover_body) {
            hover_body.SetFocusState(false);
            hover_body = null;
        }

        // TEMPORARY IMPLEMENTATION
        if (cur_body == null)
            original_body.gameObject.SetActive(false);
        else
            DestroyImmediate(cur_body.gameObject);
        cur_body = child;
        child.transform.SetParent(originalWrapper, false);
        child.transform.localPosition = Vector3.zero;
        ClearLineup();
        generation++;
        BuildLineup(cur_body);

        // TEMPORARY TRIGGER
        float similarity = FaceSimilarity(original_body.GetFace(), cur_body.GetFace());
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
        original_body = gen.GenerateRandomBody();
        original_body.transform.SetParent(originalWrapper, false);
        // BuildLineup
        BuildLineup(original_body);
    }

    public void ResetGameState () {
        // Clear game area
        ClearLineup();
        if (cur_body != null)
            DestroyImmediate(cur_body.gameObject);
        DestroyImmediate(original_body.gameObject);
        // Reset state vars
        game_active = false;
        generation = 0;
        original_body = null;
        cur_body = null;
        if (hover_body) {
            hover_body.SetFocusState(false);
            hover_body = null;
        }
        // Reset UI
        SetPercentageDisplay(1);
    }

    
    /* HELPER METHODS */
    public Body GetBody (Transform t) {
        Body b = t.GetComponent<Body>();
        if (b)
            return b;
        return t.GetComponentInParent<Body>();
    }
}
