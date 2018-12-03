using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoDecay : MonoBehaviour {

    /* VARS AND PROPERTIES */
    public List<Sprite> decayStates;

    /* REFERENCE VARS */
    SpriteRenderer sr;

    /* WRAPPER REFS */
    Transform bodyWrapper;
    /* WRAPPER NAMES */
    public static readonly string BodyWrapperName = "body-wrapper";

    /* INTERNAL VARS */
    private int cur_state;
    Body cur_body;


    /* LIFECYCLE METHODS */
    private void Awake () {
        sr = GetComponent<SpriteRenderer>();
        bodyWrapper = transform.Find(BodyWrapperName);
        sr.sprite = decayStates[cur_state];
    }


    /* CONTROL METHODS */
    public void SetBody (Body b) {
        ClearBody();
        cur_body = b.Clone();
        cur_body.transform.SetParent(bodyWrapper, false);
    }
    public void ClearBody () {
        if (cur_body) {
            DestroyImmediate(cur_body.gameObject);
            cur_body = null;
        }
    }
    public bool AdvanceDecay () {
        if (cur_state >= decayStates.Count - 1)
            return false;
        sr.sprite = decayStates[++cur_state];
        return true;
    }
    public void ResetDecay () {
        cur_state = 0;
        sr.sprite = decayStates[cur_state];
    }
    public void Reset () {
        ResetDecay();
        ClearBody();
    }


    /* ACCESSOR METHODS */
    public Body GetBody () {
        return cur_body;
    }
}
