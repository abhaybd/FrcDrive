using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class RemoteSwerve : MonoBehaviour {

    const string RemoteSwerveObjName = "test.RemoteSwerve";

    public WheelCollider lfWheel;
    public WheelCollider rfWheel;
    public WheelCollider lrWheel;
    public WheelCollider rrWheel;

    public float maxWheelTorque;
    public float maxBrakeTorque;

    private string objectName = "remoteSwerve";
    private SwerveStatus status = new SwerveStatus();

	// Use this for initialization
	void Start ()
    {
        float width = transform.localScale.x;
        float length = transform.localScale.z;
        object obj = RPC.Instance.InstantiateObject<object>(RemoteSwerveObjName, objectName,
            new string[] { "java.lang.Double", "java.lang.Double" },
            new object[] { width, length });
        Debug.Log("Instantiating remote object, success: " + (obj != null));
        StartCoroutine(CallRemote());
	}
	
	// Update is called once per frame
	void Update ()
    {
        SetWheel(lfWheel, status.lfPower, status.lfAngle);
        SetWheel(rfWheel, status.rfPower, status.rfAngle);
        SetWheel(lrWheel, status.lrPower, status.lrAngle);
        SetWheel(rrWheel, status.rrPower, status.rrAngle);
    }

    private void SetWheel(WheelCollider wheel, float power, float angle)
    {
        if(power != 0)
        {
            power = Mathf.Clamp(power, -1, 1);
            wheel.motorTorque = power * maxWheelTorque;
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
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float turn = Input.GetAxis("Turn");
            float heading = transform.eulerAngles.y;

            status = RPC.Instance.ExecuteMethod<SwerveStatus>(objectName, "getStatus",
                new string[] { "java.lang.Double",  "java.lang.Double", "java.lang.Double", "java.lang.Double" },
                new object[] { x, y, turn, heading });
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
