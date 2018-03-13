using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface ableincar {

    void MoveInCar();

    void hitincar(float vel);

    void getincar(GameObject car, float slowpercent);

    void getoutcar();

    bool checkincar();

    float checkslowpercent();
}
