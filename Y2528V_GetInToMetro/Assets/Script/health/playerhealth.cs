using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerhealth : MonoBehaviour, health {

    //*****get normal info******//
    private Animator anim;
    //**************************//

    public float maxhp = 100;

    [SerializeField]
    private float hp; //curhp
    private float nothurttime = 5f, startrecovertime;

    [SerializeField]
    private bool isdeath = false, isexhaust = false;

    private GameObject healthbar;
    private Image bar;
    private float howmuchgreen = 110;

    // Use this for initialization
    void Start()
    {
        Recover();
        healthbar = GameObject.Find("Main Camera/UI/healthbar");
        bar = healthbar.transform.Find("bar").GetComponent<Image>();
        bar.color = Color.HSVToRGB((howmuchgreen / 359), 1, (float)200 / 255);
        anim = GetComponent<Animator>();
        healthbarupdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isexhaust)
        {
            if (hp <= 0)
            {
                Exhaust(true);
            }
            else
            {
                if (Time.time > startrecovertime+ 5f)
                {
                    hp = Mathf.Min(maxhp, hp + 1f);
                }
            }
        }
        else
        {
            if (Time.time > startrecovertime -3f)
            {
                hp = Mathf.Min(maxhp, hp + 2f);
                if (hp == maxhp)
                {
                    Exhaust(false);
                }
            }
        }
        healthbarupdate();
    }

    public void Hurt(float damage)
    {
        if (!isexhaust)
        {
            hp = Mathf.Max(0f, hp - damage);
        }
        if (hp == 0)
        {
            Exhaust(true); //need to do it here so it can send exhaust massage as soon as possible
        }
        startrecovertime = Time.time + nothurttime;
    }

    void healthbarupdate()
    {
        bar.fillAmount = hp / maxhp;
        bar.color = Color.HSVToRGB((howmuchgreen * (hp / maxhp) / 359), 1, (float)200 / 255);
    }

    void Death()
    {
        if (!isdeath)
        {
            isdeath = true;
            anim.SetBool("isdeath", true);
            transform.GetComponent<movementctrl>().Death();       
        }
    }

    void Exhaust(bool isexhaust)
    {
        this.isexhaust = isexhaust;
        //anim.SetBool("isdeath", true);
        transform.GetComponent<movementctrl>().Exhaust(isexhaust);
    }

    public void Rescued()
    {

    }

    public void Rescued(Vector2 hitvel)
    {

    }

    public bool IsDeath()
    {
        return isdeath;
    }

    public void Recover()
    {
        hp = maxhp;
    }

    public void Crushed(float coefficient)
    {

    }
}
