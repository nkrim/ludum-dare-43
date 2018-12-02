using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour {

    // Feature list
    public enum Feature {
        Head=0, Skin, Ears, Eyes, Nose
    }

    /* VARIABLES AND PROPERTIES */
    // References to sprite renderers
    SpriteRenderer head_sr;
    SpriteRenderer ear_l_sr;
    SpriteRenderer ear_r_sr;
    SpriteRenderer eye_l_sr;
    SpriteRenderer eye_r_sr;
    SpriteRenderer nose_sr;

    // Feature name paths
    public static readonly string EarLeftName = "ear-left";
    public static readonly string EarRightName = "ear-right";
    public static readonly string EyeLeftName = "eye-left";
    public static readonly string EyeRightName = "eye-right";
    public static readonly string NoseName = "nose";

    // Indices of features
    protected List<int> indices;


    /* LIFECYCLE METHODS */
    // Use this for initialization
    void Awake () {
        // Init indices
        indices = new List<int>();
        int length = NumFeatures();
        for(int i=0; i<length; i++) {
            indices.Add(-1);
        }
    }
	
	
    /* MAIN MODIFYING FUNCTIONS */
    public void SetHead (GameObject head_prefab, int index) {
        if (index >= 0)
           indices[(int)Feature.Head] = index;
        GameObject head = Instantiate(head_prefab);
        head.transform.parent = transform;
        // If a head already exists, prepare for swap
        if (head_sr != null) {
            // Get current properties
            Color skin = head_sr.color;
            Sprite ear = ear_l_sr.sprite;
            Sprite eye = eye_l_sr.sprite;
            Sprite nose = nose_sr.sprite;
            // Destroy old head and reset references
            DestroyImmediate(head_sr.gameObject);
            FindSpriteRendererReferences();
            // Reset values
            SetSkin(skin, -1);
            SetEars(ear, -1);
            SetEyes(eye, -1);
            SetNose(nose, -1);
        }
        else
            FindSpriteRendererReferences();
    }
    public void SetSkin (Color skin, int index) {
        if(index >= 0)
            indices[(int)Feature.Skin] = index;
        head_sr.color = skin;
        ear_l_sr.color = skin;
        ear_r_sr.color = skin;
    }
    public void SetEars (Sprite ear, int index) {
        if (index >= 0)
            indices[(int)Feature.Ears] = index;
        ear_l_sr.sprite = ear;
        ear_r_sr.sprite = ear;
    }
    public void SetEyes (Sprite eye, int index) {
        if (index >= 0)
            indices[(int)Feature.Eyes] = index;
        eye_l_sr.sprite = eye;
        eye_r_sr.sprite = eye;
    }
    public void SetNose (Sprite nose, int index) {
        if (index >= 0)
            indices[(int)Feature.Nose] = index;
        nose_sr.sprite = nose;
    }


    /* MAIN INDEX ACCESSOR FUNCTIONS */
    public List<int> GetIndices() {
        return indices;
    }
    public int GetIndexOf(Feature f) {
        return indices[(int)f];
    }
    public void DebugIndices() {
        string res = "{";
        foreach (int i in indices)
            res += i + ", ";
        res += "}";
        Debug.Log(res);
    }


    /* CLONE METHODS */
    public Face Clone() {
        Face clone = Instantiate(this);
        clone.indices = new List<int>(this.indices);
        clone.FindSpriteRendererReferences();
        return clone;
    }


    /* MEMBER HELPER METHODS */
    protected void FindSpriteRendererReferences() {
        head_sr = GetComponentInChildren<SpriteRenderer>();
        ear_l_sr = head_sr.transform.Find(EarLeftName).GetComponent<SpriteRenderer>();
        ear_r_sr = head_sr.transform.Find(EarRightName).GetComponent<SpriteRenderer>();
        eye_l_sr = head_sr.transform.Find(EyeLeftName).GetComponent<SpriteRenderer>();
        eye_r_sr = head_sr.transform.Find(EyeRightName).GetComponent<SpriteRenderer>();
        nose_sr = head_sr.transform.Find(NoseName).GetComponent<SpriteRenderer>();
    }


    /* STATIC HELPER METHODS */
    public static int NumFeatures () {
        return System.Enum.GetNames(typeof(Feature)).Length;
    }
    public static List<Feature> BuildFeatureList () {
        System.Array arr = System.Enum.GetValues(typeof(Feature));
        List<Feature> list = new List<Feature>();
        foreach (Feature f in arr) {
            list.Add(f);
        }
        return list;
    }
}
