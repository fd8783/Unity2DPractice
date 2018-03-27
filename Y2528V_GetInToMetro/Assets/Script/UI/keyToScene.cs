using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class keyToScene : MonoBehaviour {

	public KeyCode key;
	public int sceneNum;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(key))
		{
			SceneManager.LoadScene(sceneNum);
		}
	}
}
