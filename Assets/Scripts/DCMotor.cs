using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCMotor {
    public float Kv { get; private set; }
    public float Kt { get; private set; }
    public float Voltage { get; private set; }
    public float Resistance { get; private set; }
    public float FreeSpeed { get; private set; }
    public float StallTorque { get; private set; }

    public DCMotor(float voltage, float freeSpeed, float freeCurrent, float stallTorque, float stallCurrent)
    {
        FreeSpeed = freeSpeed;
        Voltage = voltage;
        Resistance = voltage / stallCurrent;
        Kt = stallTorque / stallCurrent;
        Kv = freeSpeed / (voltage - freeCurrent * Resistance);
        StallTorque = stallTorque;
    }
}
