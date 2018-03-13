using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    private float gametime, gamestarttime;

    private Text Timeleft;


    // Use this for initialization
    void Awake()
    {
        Timeleft = transform.Find("Timeleft").GetComponent<Text>();
        gametime = GameObject.Find("stagemanager").GetComponent<stagectrl>().leveltime;
    }

    void Start()
    {
        gamestarttime = Time.time; //seem it is 0
    }

    // Update is called once per frame
    void Update()
    {
        Timeleft.text = (gametime - (Time.time)).ToString("000");
    }
}
