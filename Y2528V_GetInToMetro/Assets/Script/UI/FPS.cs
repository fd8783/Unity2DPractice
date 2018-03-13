using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour {

    private Text FPSvalue;

	// Use this for initialization
	void Awake () {
        FPSvalue = transform.Find("FPSvalue").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        FPSvalue.text = (1 / Time.deltaTime).ToString("n2");
	}
}
