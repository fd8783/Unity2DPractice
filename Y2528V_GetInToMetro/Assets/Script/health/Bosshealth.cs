using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bosshealth : MonoBehaviour, health {

    public float maxhp = 1000;

    private float hp;
    private bool isdeath = false;

    private GameObject healthbar;
    private Image bar;
    private float howmuchgreen = 110;

    // Use this for initialization
    void Awake()
    {
        Recover();
        healthbar = GameObject.Find("Main Camera/UI/healthbar");
        bar = healthbar.transform.Find("bar").GetComponent<Image>();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update() {

    }

    public void Hurt(float damage)
    {

    }

    void Recover()
    {
        hp = maxhp;
    }

    public bool IsDeath()
    {
        return isdeath;
    }

    public void Rescued()
    {

    }

    public void Rescued(Vector2 vel)
    {

    }

    public void Crushed(float coefficient)
    {

    }
}
