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
    public float lineupHeight = 6f;
    public int lineupRowMax = 3;

    /* REFERENCE VARS */
    RandomFaceGenerator gen;
    PhotoDecay photoDecay;
    GameObject startUI;
    GameObject gameUI;
    GameObject endUI;
    ProgressBar similarityBar;

    /* WRAPPER REFERENCE VARS */
    Transform photoWrapper;
    Transform parentWrapper;
    Transform lineupWrapper;
    /* WRAPPER REFERENCE NAMES */
    public static readonly string PhotoWrapperName = "photo-wrapper";
    public static readonly string ParentWrapperName = "parent-wrapper";
    public static readonly string LineupWrapperName = "lineup-wrapper";

    /* INPUT STATE VARS */
    Body mouseDownTarget;

    /* GAME STATE VARS */
    bool game_active = false;
    int generation = 0;
    Body original_body;
    Body cur_body;
    Body hover_body;
    /* INTRO/OUTRO VARS */
    bool intro_active = false;
    int intro_phase = 0;
    SpriteRenderer startBackground;
    float startFade = 1f;
    float startFadeOutPerSec = 0.5f;
    SpriteRenderer whiteFlash;
    float whiteFade = 0f;
    float whiteFadeOutPerSec = 2.0f;
    Vector3 parentWrapperPos1 = new Vector3(0, -2f, 0);
    Vector3 parentWrapperScale1 = new Vector3(1, 1, 1);
    Vector3 parentWrapperPos2;
    Vector3 parentWrapperScale2;

    /* PERSISTENT GAME STATE VARS */
    int highScore = 0;

	/* LIFECYCLE METHODS */
	void Awake () {
        // Link Generator
        gen = GetComponent<RandomFaceGenerator>();
        if (gen == null)
            Debug.LogError("GameController needs a RandomFaceGenerator component");
        // Link placement wrappers
        photoWrapper = transform.Find(PhotoWrapperName);
        parentWrapper = transform.Find(ParentWrapperName);
        lineupWrapper = transform.Find(LineupWrapperName);
        if (parentWrapper == null || lineupWrapper == null)
            Debug.LogError("GameController is missing one or more internal wrappers");
        // Link photo decay
        photoDecay = photoWrapper.GetComponent<PhotoDecay>();
        // Link UIs
        startUI = GameObject.FindWithTag("start-UI");
        similarityBar = GameObject.FindWithTag("similarity-bar").GetComponent<ProgressBar>();
        gameUI = GameObject.FindWithTag("game-UI");
        gameUI.SetActive(false);
        endUI = GameObject.FindWithTag("end-UI");
        endUI.SetActive(false);
        // Link intro/outro pieces
        startBackground = GameObject.FindWithTag("start-background").GetComponent<SpriteRenderer>();
        whiteFlash = GameObject.FindWithTag("white-flash").GetComponent<SpriteRenderer>();
        parentWrapperPos2 = parentWrapper.localPosition;
        parentWrapperScale2 = parentWrapper.localScale;
	}

    void Start () {
        
    }

    private void Update () {
        /* PREQUEL PHASE */
        if (!game_active && !intro_active) {
            if(Input.GetMouseButtonUp(0)) {
                startUI.SetActive(false);
                endUI.SetActive(false);
                StartIntro();
            }
            return;
        }

        /* INTRO PHASE */
        if (intro_active) {
            switch (intro_phase) {
                case 0: // FADE START BG OUT
                    if (startFade > 0) {
                        startFade -= Time.deltaTime * startFadeOutPerSec;
                        startBackground.color = new Color(1, 1, 1, Mathf.Max(0, startFade));
                        Debug.Log("FADING OUT START: " + startFade);
                    }
                    else {
                        startFade = 0;
                        intro_phase++;
                        Debug.Log("START FADED OUT");
                    }
                    break;
                case 1: // FLASH (AND INIT GAME STATE)
                    whiteFade = 1;
                    whiteFlash.color = new Color(1, 1, 1, 1);
                    intro_phase++;
                    parentWrapper.localPosition = parentWrapperPos2;
                    parentWrapper.localScale = parentWrapperScale2;
                    photoDecay.gameObject.SetActive(true);
                    photoDecay.SetBody(original_body);
                    gameUI.SetActive(true);
                    break;
                case 2: // FADE OUT FLASH
                    if (whiteFade > 0) {
                        whiteFade -= Time.deltaTime * whiteFadeOutPerSec;
                        whiteFlash.color = new Color(1, 1, 1, Mathf.Max(0, whiteFade));
                    }
                    else {
                        whiteFade = 0;
                        intro_phase++;
                    }
                    break;
                case 3:
                    StartGame();
                    intro_active = false;
                    break;
                default:
                    break;
            }
            return;
        }

        /* GAME PHASE */
        if (game_active) {
            /* INPUT HANDLING */
            Vector2 clickPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitLineupBody = Physics2D.Raycast(clickPoint, Vector2.zero, Mathf.Infinity, LayerMask.NameToLayer("lineup"));
            // Raycast hover focus
            bool hovering = false;
            if (hitLineupBody.transform) {
                Body hitBody = GetBody(hitLineupBody.transform);
                if (hitBody && hitBody != cur_body && hitBody != original_body && hitBody != photoDecay.GetBody()) {
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
                    Body hitBody = GetBody(hitLineupBody.transform);
                    if (mouseDownTarget == GetBody(hitLineupBody.transform) && hitBody != cur_body && hitBody != original_body && hitBody != photoDecay.GetBody()) {
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
        child.transform.SetParent(parentWrapper, false);
        child.transform.localPosition = Vector3.zero;
        ClearLineup();
        generation++;
        photoDecay.AdvanceDecay();
        BuildLineup(cur_body);

        // TEMPORARY TRIGGER
        float similarity = FaceSimilarity(original_body.GetFace(), cur_body.GetFace());
        SetPercentageDisplay(similarity);

        if((similarity - similarityThreshold) <= 0.0001) {
            ResetGameState();
        }
    }


    /* FACE COMPARISON */
    float FaceSimilarity (Face face1, Face face2) {
        int num_equal = 0;
        // Indices comparison
        List<int> face1_indices = face1.GetIndices();
        List<int> face2_indices = face2.GetIndices();
        int length_indices = Mathf.Min(face1_indices.Count, face2_indices.Count);
        for (int i = 0; i < length_indices; i++) {
            if (face1_indices[i] == face2_indices[i])
                num_equal++;
        }
        // Shrinkage comparison
        List<bool> face1_shrinkage = face1.GetShrunkFeatures();
        List<bool> face2_shrinkage = face2.GetShrunkFeatures();
        int length_shrinkage = Mathf.Min(face1_shrinkage.Count, face2_shrinkage.Count);
        for (int i = 0; i < length_shrinkage; i++) {
            if (face1_shrinkage[i] == face2_shrinkage[i])
                num_equal++;
        }
        return (float)num_equal / (length_indices + length_shrinkage);
    }


    /* UI CONTROL */
    void SetPercentageDisplay (float x) {
        //var percentageDisplay = gameUI.transform.Find("percentage-display").GetComponent<TMPro.TextMeshProUGUI>();
        //percentageDisplay.text = string.Format("{0:0.0%}", x);
        similarityBar.UpdateProgress(x);
    }


    /* CONTROLLING GAMESTATE */
    public void StartGame () {
        // Set game to active
        game_active = true;
        // Generate initial face
        //original_body = gen.GenerateRandomBody();
        //original_body.transform.SetParent(parentWrapper, false);
        //photoDecay.SetBody(original_body);
        // BuildLineup
        BuildLineup(original_body);
    }

    public void ResetGameState () {
        game_active = false;
        // Set high score
        highScore = Mathf.Max(highScore, generation);
        // Clear game area
        ClearLineup();
        if (cur_body != null)
            DestroyImmediate(cur_body.gameObject);
        DestroyImmediate(original_body.gameObject);
        // Setup end game screen
        endUI.SetActive(true);
        endUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Your descendents have no more resemblance to you, it's over.\n\nYou were able to keep your face alive for:\n"
            + generation + " generations\n\nThe longest you've gone is:\n"
            + highScore + " generations\n\n"
            + "Click to try again.";
        startBackground.color = Color.white;
        startFade = 1;
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
        photoDecay.Reset();
        photoDecay.gameObject.SetActive(false);
        SetPercentageDisplay(1);
        gameUI.SetActive(false);
    }

    public void StartIntro () {
        intro_active = true;
        intro_phase = 0;
        // Generate initial face
        original_body = gen.GenerateRandomBody();
        original_body.transform.SetParent(parentWrapper, false);
        parentWrapper.localPosition = parentWrapperPos1;
        parentWrapper.localScale = parentWrapperScale1;
    }

    /* HELPER METHODS */
    public Body GetBody (Transform t) {
        Body b = t.GetComponent<Body>();
        if (b)
            return b;
        return t.GetComponentInParent<Body>();
    }
}
