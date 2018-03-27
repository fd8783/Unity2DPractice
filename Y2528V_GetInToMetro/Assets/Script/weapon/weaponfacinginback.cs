using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class weaponfacinginback : MonoBehaviour {  //this script mainly use to update the direction the weapon facing in back
    private Transform parent;
    private movementctrl parentscript;
    private SpriteRenderer[] sprite;
    private int spritecount;
    private SortingGroup sortinggroup;

    private int[] curdir = { 1, -1 }, targetdir = { 1, -1 };

    // Use this for initialization
    void Start()
    {
        parent = transform.root.root.root;
        parentscript = parent.GetComponent<movementctrl>();
        sprite = GetComponentsInChildren<SpriteRenderer>();
        spritecount = sprite.Length;
        sortinggroup = transform.parent.GetComponent<SortingGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        changedir(targetdir, parentscript.getcurdir());
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
            sprite[i].sortingOrder += (dir * 15);
        }
        if (dir == 1)
        {
            sortinggroup.sortingOrder = 16;
        }
        else
        {
            sortinggroup.sortingOrder = 1;
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
