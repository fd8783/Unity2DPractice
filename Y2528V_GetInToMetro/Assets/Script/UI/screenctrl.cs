using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class screenctrl : MonoBehaviour {

    static public float defaultshakecoefficient = 0.01f;
    static public float shakecoefficient = 0.01f;

    static private float stoptime;
    static private bool shake;

    private bool isshaked = false;

    private float curtime;

    private Transform maincamera;
    private Vector3 camstartpos, shakepos;

	// Use this for initialization
	void Awake () {
        stoptime = 0f;
        maincamera = GameObject.Find("Main Camera").transform;
        camstartpos = maincamera.position;
	}
	
	// Update is called once per frame
	void Update () {
        curtime = Time.realtimeSinceStartup;
        if (shake)
        {
            if (curtime > stoptime)
            {
                if (isshaked)
                {
                    Time.timeScale = 1f;
                    maincamera.position = camstartpos;
                    shake = false;
                    isshaked = false;
                }
                else
                {
                    Shake(); //isshaked = true in this function
                }
            }
            else
            {
                Time.timeScale = 0.1f;
                Shake(); //isshaked = true in this function
            }
        }
	}

    void Shake()
    {
        shakepos = Random.insideUnitCircle.normalized * Random.Range(0.8f,1f) * shakecoefficient;
        //Debug.Log(shakepos.ToString("f4"));
        shakepos.z = -10f; // z from camstartpos
        maincamera.position = shakepos;
        isshaked = true;
    }

    static public void StopScreen(float time)
    {
        ShakeScreen(screenctrl.shakecoefficient);
        stoptime = Time.realtimeSinceStartup + time;   
    }

    static public void StopScreen(float time, float coefficient)
    {
        ShakeScreen(coefficient);
        stoptime = Time.realtimeSinceStartup + time;
    }

    static public void ShakeScreen(float coefficient)
    {
        shake = true;
        screenctrl.shakecoefficient = coefficient;
        Debug.Log("shake");
    }

    static public void ShakeScreen()
    {
        shake = true;
        screenctrl.shakecoefficient = screenctrl.defaultshakecoefficient;
    }
}
