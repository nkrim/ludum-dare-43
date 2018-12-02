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


    /* LIFECYCLE METHODS */
    private void Awake () {
        spotlight = transform.Find(SpotlightName).gameObject;
    }


    /* VISUAL EFFECTS */
    public void SetSpotlightActive (bool active) {
        spotlight.SetActive(active);
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
