using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trainctrl : MonoBehaviour {

    public static bool arrived = false;

    private Transform trainctrlpoint;
    private Transform slowpt, stoppt;
    private float slowpty, stoppty;
    private Vector2 curpos;
    private int dir = -1;

    //shake//
    private bool shake;
    private float shaketime;
    private Vector2 shakepos;
    private Transform carctrl;

    // Use this for initialization
    void Awake()
    {
        trainctrlpoint = GameObject.Find("trainctrlpoint").transform;
        slowpt = trainctrlpoint.Find("slowpoint");
        stoppt = trainctrlpoint.Find("stoppoint");
        slowpty = slowpt.transform.position.y;
        stoppty = stoppt.transform.position.y;
        carctrl = transform.Find("carctrl");

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        curpos = transform.position;
        if (dir == 1)
        {
            if (curpos.y < stoppty)
            {
                if (curpos.y < slowpty)
                {
                    curpos.y += 0.07f;
                }
                else
                {
                    curpos.y = Mathf.Lerp(curpos.y, stoppty - (dir * 0.02f), 0.08f);
                }
            }
            else
            {
                arrived = true;
            }
        }
        else
        {
            if (curpos.y > stoppty)
            {
                if (curpos.y > slowpty)
                {
                    curpos.y -= 0.07f;
                }
                else
                {
                    curpos.y = Mathf.Lerp(curpos.y, stoppty + (dir * 0.02f), 0.08f);
                }
            }
            else
            {
                arrived = true;
            }
        }

        if (shake)
        {
            if (Time.time < shaketime)
            {
                shakepos = (Random.insideUnitCircle * 0.1f);
                carctrl.localPosition = shakepos;
            }
            else
            {
                carctrl.localPosition = Vector2.zero;
                shake = false;
            }
        }
        transform.position = curpos;
    }

    public void Hurt()
    {
        //shake train
        shaketime = Time.time + 0.3f;
        shake = true;
    }
}
















/*{

    public static bool arrived = false;

    private Transform trainctrlpoint;
    private Transform slowpt, stoppt;
    private float slowptx, stopptx;
    private Vector2 curpos;
    private int dir = -1;

    // Use this for initialization
    void Awake()
    {
        trainctrlpoint = GameObject.Find("trainctrlpoint").transform;
        slowpt = trainctrlpoint.Find("slowpoint");
        stoppt = trainctrlpoint.Find("stoppoint");
        slowptx = slowpt.transform.position.x;
        stopptx = stoppt.transform.position.x;

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        curpos = transform.position;
        if (dir == 1)
        {
            if (curpos.x < stopptx)
            {
                if (curpos.x < slowptx)
                {
                    curpos.x += 0.07f;
                }
                else
                {
                    curpos.x = Mathf.Lerp(curpos.x, stopptx + 0.02f, 0.08f);
                }
            }
            else
            {
                arrived = true;
            }
        }
        else
        {
            if (curpos.x > stopptx)
            {
                if (curpos.x > slowptx)
                {
                    curpos.x -= 0.07f;
                }
                else
                {
                    curpos.x = Mathf.Lerp(curpos.x, stopptx - 0.02f, 0.08f);
                }
            }
            else
            {
                arrived = true;
            }
        }
        transform.position = curpos;
    }
}*/
