using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour {

    /* VARS AND PROPERTIES */
    public Color shirtColor {
        get {
            return _shirtColor;
        }
        set {
            GetComponent<SpriteRenderer>().color = value;
            _shirtColor = value;
        }
    }
    private Color _shirtColor;

    /* LINKING VAR NAMES */
    public static readonly string SpotlightName = "spotlight";
    /* LINKING REFERENCE VARS */
    GameObject spotlight;

    /* FOCUS EFFECT VARIABLES */
    public static readonly float focus_zoom = 1.25f;


    /* LIFECYCLE METHODS */
    private void Awake () {
        spotlight = transform.Find(SpotlightName).gameObject;
    }


    /* VISUAL EFFECTS */
    public void SetFocusState (bool active) {
        spotlight.SetActive(active);
        transform.localScale = active ? new Vector3(focus_zoom, focus_zoom, 1) : new Vector3(1,1,1);
    }



    /* FACE METHODS */
    public void SetFace (Face face) {
        Face old_face = GetFace();
        if (old_face)
            DestroyImmediate(old_face.gameObject);
        face.transform.SetParent(transform, false);
    }

    public Face GetFace () {
        return GetComponentInChildren<Face>();
    }
}
