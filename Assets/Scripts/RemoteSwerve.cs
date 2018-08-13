using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class RemoteSwerve : MonoBehaviour {

    public WheelCollider lfWheel;
    public WheelCollider rfWheel;
    public WheelCollider lrWheel;
    public WheelCollider rrWheel;

    public float maxWheelTorque;
    public float maxBrakeTorque;

    private SwerveStatus status;

	// Use this for initialization
	void Start () {
        StartCoroutine(CallRemote());
	}
	
	// Update is called once per frame
	void Update () {
        SetWheel(lfWheel, status.lfPower, status.lfAngle);
        SetWheel(rfWheel, status.rfPower, status.rfAngle);
        SetWheel(lrWheel, status.lrPower, status.lrAngle);
        SetWheel(rrWheel, status.rrPower, status.rrAngle);
    }

    private void SetWheel(WheelCollider wheel, float power, float angle)
    {
        if(power > 0)
        {
            wheel.motorTorque = power * maxWheelTorque;
            wheel.brakeTorque = 0;
        }
        else
        {
            wheel.motorTorque = 0;
            wheel.brakeTorque = maxBrakeTorque;
        }
        wheel.steerAngle = angle;
    }

    IEnumerator CallRemote()
    {
        while (true)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float turn = Input.GetAxis("Turn");
            float heading = transform.eulerAngles.y;

            status = (SwerveStatus) RPC.Instance.ExecuteMethod(typeof(SwerveStatus), "swerveDrive_Cartesian", x, y, turn, false, heading);
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
