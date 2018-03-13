using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyweaponfacing : MonoBehaviour { //this script mainly use to update the direction the weapon facing
                                                   // now it may involved in some attack control too 
    [SerializeField]
    private Vector3 startpos;
    private Transform parent;
    private enemyai enemyscript;
    private SpriteRenderer[] sprite;
    private int spritecount;

    private int[] curdir = { 1, -1 }, targetdir = { 1, -1 };

    // Use this for initialization
    void Awake()
    {
        startpos = transform.localPosition;
        transform.localPosition = Vector3.zero;
        parent = transform.parent.parent.parent;
        enemyscript = parent.GetComponent(typeof(enemyai)) as enemyai;
        sprite = GetComponentsInChildren<SpriteRenderer>();
        spritecount = sprite.Length;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        changedir(targetdir, enemyscript.getcurdir());
        if (curdir[1] != targetdir[1])
        {
            FilpVer(targetdir[1]);
        }
        changedir(curdir, targetdir);
    }

    void FilpVer(int dir)
    {
        for (int i = 0; i < spritecount; i++)
        {
            //sprite[i].sortingOrder -= (dir * 15);
            if (sprite[i].sortingOrder < 10)
            {
                sprite[i].sortingOrder += 15;
            }
            else
            {
                sprite[i].sortingOrder -= 15;
            }
        }
    }

    void changedir(int[] dirarr, int firstnum, int secondnum)
    {
        dirarr[0] = firstnum;
        dirarr[1] = secondnum;
    }

    void changedir(int[] dirarr, int[] targetdir)
    {
        dirarr[0] = targetdir[0];
        dirarr[1] = targetdir[1];
    }

    public Vector3 getstartpos()
    {
        Debug.Log(startpos.ToString("F4"));
        return startpos;
    }
}
