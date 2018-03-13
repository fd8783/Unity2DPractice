using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface health {

    void Hurt(float damage);

    bool IsDeath();

    void Rescued();

    void Rescued(Vector2 hitvel);

    void Crushed(float coefficient);
}
