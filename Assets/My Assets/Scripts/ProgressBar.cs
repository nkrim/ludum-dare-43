using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour {

    public float endX = -9;

    Transform fill;

	// Use this for initialization
	void Awake () {
        fill = transform.Find("bar-fill");
	}
	
	public void UpdateProgress(float val) {
        float dest_x = (val * 9) - 9;
        fill.localPosition = new Vector3(dest_x, fill.localPosition.y, 0);
    }
}
