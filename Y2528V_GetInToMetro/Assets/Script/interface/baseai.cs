using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface baseai {

    //no Move() first, don't want to make everything public, it look mess

    void hit();

    void hit(Vector2 vel);

    void PickUp(Transform picker);

    void PutDown(Vector2 vel);  

    void Stun(float time);

    void Scared(bool isscared);

    bool IsScared();

    bool IsHit();
}
