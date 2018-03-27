using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatorcloth : MonoBehaviour {

    public bool diffbackimg;
    public int parentstack = 1;
    public Sprite frontimg ,backimg;

    private Transform parent;
    private enemyai enemyscript;
    private SpriteRenderer imgRenderer;

    private int[] curdir = { 1, -1 }, targetdir = { 1, -1 };

    // Use this for initialization
    void Awake () {
        imgRenderer = GetComponent<SpriteRenderer>();

        parent = transform;
        for(int i = 0; i < parentstack; i++)
        {
            parent = parent.parent;
        }
        enemyscript = parent.GetComponent<enemyai>();
        frontimg = imgRenderer.sprite;
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
        if (dir == -1)
        {
            imgRenderer.sprite = frontimg;
        }
        else
        {
            imgRenderer.sprite = backimg;
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
}
