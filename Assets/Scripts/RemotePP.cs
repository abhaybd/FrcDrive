using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePP : MonoBehaviour {
    const string RemotePPClassName = "drivesim.RemotePP";

    public WheelCollider lfWheel;
    public WheelCollider rfWheel;
    public WheelCollider lrWheel;
    public WheelCollider rrWheel;

    public float maxWheelSpeed;
    public float maxMotorSpeed = 18700f; // rev/min
    public float maxBrakeTorque;
    public float Kv = 1570.05f; // rev/volt-min
    public float Kt = 0.00530f; // Nm/A
    public float R = 0.0896f; // ohms

    private string objectName = "remotePP";
    private SwerveStatus status = new SwerveStatus();
    private float gearing;
    private bool started = false;

    // Use this for initialization
    void Start()
    {
        gearing = maxWheelSpeed / maxMotorSpeed; // output over input
        float topSpeed = (maxWheelSpeed / 60f) * 2f * (float)Math.PI * lfWheel.radius * lfWheel.transform.localScale.y;
        Debug.Log("Gearing: " + gearing + ", TopSpeed: " + topSpeed);
        float width = (rfWheel.transform.position - lfWheel.transform.position).magnitude;
        float length = (rfWheel.transform.position - rrWheel.transform.position).magnitude;
        object obj = RPC.Instance.InstantiateObject<object>(RemotePPClassName, objectName,
            new string[] { "java.lang.Double", "java.lang.Double", "java.lang.Double" },
            new object[] { width, length, topSpeed });
        Debug.Log("Instantiating remote object, success: " + (obj != null));
        StartCoroutine(CallRemote());
    }

    private float GetTorque(WheelCollider wheel, float volts)
    {
        // V = (T/Kt)*R + w/Kv
        // T = (V-w/Kv)*Kt/R
        float volts_abs = Math.Abs(volts);
        float rpm = Math.Abs(wheel.rpm / gearing);
        float back_emf = Math.Min(rpm / Kv, volts_abs);
        float torque_abs = (volts_abs - back_emf) * Kt / R;
        float torque = Math.Sign(volts) * torque_abs / gearing;
        return torque;
    }

    // Update is called once per frame
    void Update()
    {
        SetWheel(lfWheel, status.lfPower, status.lfAngle);
        SetWheel(rfWheel, status.rfPower, status.rfAngle);
        SetWheel(lrWheel, status.lrPower, status.lrAngle);
        SetWheel(rrWheel, status.rrPower, status.rrAngle);

        if (!started && Input.anyKeyDown)
        {
            started = true;
            RPC.Instance.ExecuteMethod<object>(objectName, "drive");
        }
    }

    void OnDestroy()
    {
        RPC.Instance.Close();
    }

    private void SetWheel(WheelCollider wheel, float power, float angle)
    {
        if (power != 0)
        {
            float volts = power * 12f;
            wheel.motorTorque = GetTorque(wheel, volts);
            wheel.brakeTorque = 0;
        }
        else
        {
            wheel.motorTorque = 0;
            wheel.brakeTorque = maxBrakeTorque;
        }
        wheel.steerAngle = angle;
        Vector3 rot = wheel.transform.localEulerAngles;
        Vector3 targetRot = new Vector3(rot.x, wheel.steerAngle, rot.z);
        wheel.transform.localEulerAngles = targetRot;
    }

    IEnumerator CallRemote()
    {
        while (true)
        {
            float x = transform.position.x;
            float y = transform.position.z;
            float heading = transform.eulerAngles.y;

            status = RPC.Instance.ExecuteMethod<SwerveStatus>(objectName, "getStatus",
                new string[] { "java.lang.Double", "java.lang.Double", "java.lang.Double" },
                new object[] { x, y, heading });
            yield return new WaitForSeconds(0.05f);
        }
    }

    [Serializable]
    private class SwerveStatus
    {
        public float lfPower, lfAngle;
        public float rfPower, rfAngle;
        public float lrPower, lrAngle;
        public float rrPower, rrAngle;
    }
}
