using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour {

    // Feature list
    public enum Feature {
        Head=0, Skin, Ears, Eyes, Nose, Mouth
    }
    // Shrinkable Feature List and settings
    public enum ShrinkableFeature {
        NonShrinkable=-1, Ears=0, Eyes, Nose, Mouth
    }
    public static readonly float[] ShrunkFeatureScales = new float[] {
        0.5f, // Ears
        0.5f, // Eyes
        0.5f, // Nose
        0.5f, // Mouth
    };

    /* VARIABLES AND PROPERTIES */
    // References to sprite renderers
    SpriteRenderer head_sr;
    SpriteRenderer ear_l_sr;
    SpriteRenderer ear_r_sr;
    SpriteRenderer eye_l_sr;
    SpriteRenderer eye_r_sr;
    SpriteRenderer nose_sr;
    SpriteRenderer mouth_sr;

    // Feature name paths
    public static readonly string EarLeftName = "ear-left";
    public static readonly string EarRightName = "ear-right";
    public static readonly string EyeLeftName = "eye-left";
    public static readonly string EyeRightName = "eye-right";
    public static readonly string NoseName = "nose";
    public static readonly string MouthName = "mouth";

    // Indices of features
    protected List<int> indices;
    // Shrink states of shrinkable features
    protected List<bool> shrunkFeatures;


    /* LIFECYCLE METHODS */
    // Use this for initialization
    void Awake () {
        // Init indices
        indices = new List<int>();
        int length = NumFeatures();
        for(int i=0; i<length; i++) {
            indices.Add(-1);
        }
        // Init shrunkFeatures
        shrunkFeatures = new List<bool>();
        length = EnumCount(typeof(ShrinkableFeature));
        for(int i=0; i<length; i++) {
            shrunkFeatures.Add(false);
        }
    }


    /* MAIN MODIFYING FUNCTIONS */
    public void SetHead (GameObject head_prefab, int index) {
        if (index >= 0)
           indices[(int)Feature.Head] = index;
        GameObject head = Instantiate(head_prefab);
        head.transform.SetParent(transform, false);
        // If a head already exists, prepare for swap
        if (head_sr != null) {
            // Get current properties
            Color skin = head_sr.color;
            Sprite ear = ear_l_sr.sprite;
            Sprite eye = eye_l_sr.sprite;
            Sprite nose = nose_sr.sprite;
            Sprite mouth = mouth_sr.sprite;
            // Destroy old head and reset references
            DestroyImmediate(head_sr.gameObject);
            FindSpriteRendererReferences();
            // Reset values
            SetSkin(skin, -1);
            SetEars(ear, -1);
            SetEyes(eye, -1);
            SetNose(nose, -1);
            SetMouth(mouth, -1);
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
    public void SetMouth (Sprite mouth, int index) {
        if (index >= 0)
            indices[(int)Feature.Mouth] = index;
        mouth_sr.sprite = mouth;
    }
    public void ToggleFeatureShrinkage (Feature f) {
        // Get transforms of features to toggle
        ShrinkableFeature sf = FeatureToShrinkable(f);
        Transform feature_t = null;
        Transform feature_t2 = null;
        switch (sf) {
            case ShrinkableFeature.Ears:
                feature_t = ear_l_sr.transform;
                feature_t2 = ear_r_sr.transform;
                break;
            case ShrinkableFeature.Eyes:
                feature_t = eye_l_sr.transform;
                feature_t2 = eye_r_sr.transform;
                break;
            case ShrinkableFeature.Nose:
                feature_t = nose_sr.transform;
                break;
            case ShrinkableFeature.Mouth:
                feature_t = mouth_sr.transform;
                break;
            default:
                Debug.LogError("Tried to shrink NonShrinkable feature " + f);
                return;
        }
        // Toggle feature size
        bool currentlyShrunk = shrunkFeatures[(int)sf];
        if (currentlyShrunk) {
            feature_t.localScale = new Vector3(1, 1, 1);
            if (feature_t2)
                feature_t2.localScale = new Vector3(1, 1, 1);
        }
        else {
            float scaleFactor = ShrunkFeatureScales[(int)sf];
            feature_t.localScale = new Vector3(scaleFactor, scaleFactor, 1);
            if (feature_t2)
                feature_t2.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
        // Adjust shrunkFeatures value
        shrunkFeatures[(int)sf] = !currentlyShrunk;
    }


    /* SHRINKABLE HELPERS */
    public static ShrinkableFeature FeatureToShrinkable (Feature f) {
        switch (f) {
            case Feature.Ears:
                return ShrinkableFeature.Ears;
            case Feature.Eyes:
                return ShrinkableFeature.Eyes;
            case Feature.Nose:
                return ShrinkableFeature.Nose;
            case Feature.Mouth:
                return ShrinkableFeature.Mouth;
            default:
                return ShrinkableFeature.NonShrinkable;
        }
    }
    public static bool IsShrinkableFeature (Feature f) {
        return FeatureToShrinkable(f) >= 0;
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
        mouth_sr = head_sr.transform.Find(MouthName).GetComponent<SpriteRenderer>();
    }


    /* STATIC HELPER METHODS */
    public static int EnumCount (System.Type enumType) {
        return System.Enum.GetNames(enumType).Length;
    }
    public static int NumFeatures () {
        return EnumCount(typeof(Feature));
    }
    public static List<Feature> BuildFeatureList () {
        System.Array arr = System.Enum.GetValues(typeof(Feature));
        List<Feature> list = new List<Feature>();
        foreach (Feature f in arr) {
            list.Add(f);
        }
        return list;
    }
    public static List<Feature> BuildShrinkableFeatureList () {
        List<Feature> list = BuildFeatureList();
        for(int i=0; i<list.Count; i++) {
            if (!IsShrinkableFeature(list[i])) {
                list.RemoveAt(i);
                i--;
            }
        }
        return list;
    } 
}
