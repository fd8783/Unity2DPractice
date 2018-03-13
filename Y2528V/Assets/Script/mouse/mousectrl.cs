using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class mousectrl : MonoBehaviour {

    public LayerMask GrabableLayer;

    public Sprite normal, detect, grab;

    //public CursorMode cursorMode = CursorMode.Auto;
    //public Vector2 hotSpot = Vector2.zero; //don't know what hotSpot means, i just know it is mouse spot , **change to  Vector2(16,15) now

    private bool grabing; // current state

    private float mouseSpeed = 0.006f;

    private SpriteRenderer mouseImg;
    private float detectCircleRadius, closestTargetDis, disCheck;
    [SerializeField]
    private Collider2D[] detectTarget;
    private Transform closestTarget;
    private Vector2 mouseCurPos, mouseLastPos, realCurPos, mouseMoveTLpt, mouseMoveDRpt; //TL = topleft, DR = downright
    private int targetCount, closestTargetNum, curCursorState; //0 for normal, 1 for detect, 2 for grab

	// Use this for initialization
	void Awake () {
        Debug.Log(Camera.main.pixelWidth + " " + Screen.height);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        detectCircleRadius = GetComponent<CircleCollider2D>().radius;
        mouseImg = transform.Find("mouseimg").GetComponent<SpriteRenderer>();
        realCurPos = transform.position;
        mouseMoveTLpt = GameObject.Find("stagemanager/mousemovearea/topleftpt").transform.position;
        mouseMoveDRpt = GameObject.Find("stagemanager/mousemovearea/downrightpt").transform.position;
        realCurPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
	
	// Update is called once per frame
	void Update () {
        MouseMove();

        MouseCheck();

        if (!grabing)
        {
            DetectTarget();
        }

    }

    void MouseMove()
    {
        Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        /*mouseCurPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //if curpos is Vector3, z will be -10, cause it's now a Vector2, z will be always 0

        if (mouseCurPos != mouseLastPos) //Moved
        {
            realCurPos = realCurPos + ((mouseCurPos - mouseLastPos) * mouseSpeed);
        }*/

        realCurPos.x = Mathf.Clamp(realCurPos.x + Input.GetAxis("Mouse X") * mouseSpeed, mouseMoveTLpt.x, mouseMoveDRpt.x);
        realCurPos.y = Mathf.Clamp(realCurPos.y + Input.GetAxis("Mouse Y") * mouseSpeed, mouseMoveDRpt.y, mouseMoveTLpt.y);

        transform.position = realCurPos;
        mouseLastPos = mouseCurPos; //this may be better to run in the last of Update 
    }

    void MouseCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grabing = true;
            GrabTarget();
        }
        if (Input.GetMouseButtonUp(0))
        {
            grabing = false;
            DropTarget();
        }
    }

    void DetectTarget()
    {
        detectTarget = Physics2D.OverlapCircleAll(realCurPos, detectCircleRadius, GrabableLayer);
        targetCount = detectTarget.Length;
        if (targetCount == 0)
        {
            SetMouseNormal();
        }
        else
        {
            SetMouseDetect();
        }
    }

    void GrabTarget()
    {
        /////***********************    Find Closest Target   ****************************/////
        if (targetCount > 0)
        {
            closestTargetNum = 0;
            closestTargetDis = Vector2.Distance(realCurPos, detectTarget[0].transform.position);

            for (int i = 1; i < targetCount; i++)
            {
                disCheck = Vector2.Distance(realCurPos, detectTarget[i].transform.position);
                if (closestTargetDis > disCheck)
                {
                    closestTargetDis = disCheck;
                    closestTargetNum = i;
                }
            }

            closestTarget = detectTarget[closestTargetNum].transform;
        }
        /////****************************************************************************/////


        SetMouseGrab();

    }

    void DropTarget()
    {
        if (closestTarget.CompareTag("button"))
        {
            closestTarget.GetComponent<Button>().onClick.Invoke();
        }
    }

    public void SetMouseNormal()
    {
        if (curCursorState != 0)
        {
            mouseImg.sprite = normal;
            curCursorState = 0;
        }
    }

    public void SetMouseDetect()
    {
        if (curCursorState != 1)
        {
            mouseImg.sprite = detect;
            curCursorState = 1;
        }
    }

    public void SetMouseGrab()
    {
        if (curCursorState != 2)
        {
            mouseImg.sprite = grab;
            curCursorState = 2;
        }
    }
    
}
