﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyToExit : MonoBehaviour {

	public KeyCode key;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(key))
		{
			Application.Quit();
		}
	}
}
