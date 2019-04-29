using System.Collections;
using UnityEngine;
using System;

public class RemoteSwerve : MonoBehaviour
{

    const string RemoteSwerveObjName = "drivesim.RemoteSwerve";
    const string GyroObjName = "drivesim.MockGyro";

    public WheelCollider lfWheel;
    public WheelCollider rfWheel;
    public WheelCollider lrWheel;
    public WheelCollider rrWheel;

    public float maxWheelSpeed;
    public float maxBrakeTorque;

    private DCMotor motor = new Vex775Pro();

    private Encoder lfEncoder;
    private Encoder rfEncoder;
    private Encoder rrEncoder;
    private Encoder lrEncoder;

    private string objectName = "remoteSwerve";
    private string remoteGyroName = "gyro";
    private SwerveStatus status = new SwerveStatus();
    private float gearing;

    // Use this for initialization
    void Start()
    {
        lfEncoder = lfWheel.GetComponent<Encoder>();
        rfEncoder = rfWheel.GetComponent<Encoder>();
        rrEncoder = rrWheel.GetComponent<Encoder>();
        lrEncoder = lrWheel.GetComponent<Encoder>();

        gearing = maxWheelSpeed / motor.FreeSpeed; // output over input
        Debug.Log("Gearing: " + gearing);
        float width = (rfWheel.transform.position - lfWheel.transform.position).magnitude;
        float length = (rfWheel.transform.position - rrWheel.transform.position).magnitude;
        RPC.Instance.InstantiateObject<object>(GyroObjName, remoteGyroName, new string[] { "java.lang.String" }, new object[] { "MockGyro" });
        object obj = RPC.Instance.InstantiateObject<object>(RemoteSwerveObjName, objectName,
            new string[] { "java.lang.Double", "java.lang.Double", "REMOTE:trclib.TrcGyro" },
            new object[] { width, length, remoteGyroName });
        Debug.Log("Instantiating remote object, success: " + (obj != null));
        StartCoroutine(CallRemote());
    }

    private float GetTorque(WheelCollider wheel, float volts)
    {
        // V = (T/Kt)*R + w/Kv
        // T = (V-w/Kv)*Kt/R
        float volts_abs = Math.Abs(volts);
        float rpm = Math.Abs(wheel.rpm / gearing);
        float back_emf = Math.Min(rpm / motor.Kv, volts_abs);
        float torque_abs = (volts_abs - back_emf) * motor.Kt / motor.Resistance;
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

        //Debug.LogFormat("X={0},Y={1},heading={2}", status.x, status.y, status.heading);
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
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float turn = Input.GetAxis("Turn");
            float heading = transform.eulerAngles.y;

            RPC.Instance.ExecuteMethod<object>(remoteGyroName, "setHeading", new string[] { "java.lang.Double" }, new object[] { heading });

            status = RPC.Instance.ExecuteMethod<SwerveStatus>(objectName, "getStatus",
                new string[] { "java.lang.Double", "java.lang.Double", "java.lang.Double", "java.lang.Double", "java.lang.Double", "java.lang.Double", "java.lang.Double" },
                new object[] { x, y, turn, lfEncoder.GetTangentialVelocity(), rfEncoder.GetTangentialVelocity(), lrEncoder.GetTangentialVelocity(), rrEncoder.GetTangentialVelocity() });
            yield return new WaitForSeconds(0.01f);
        }
    }

    [Serializable]
    private class SwerveStatus
    {
        public float lfPower, lfAngle;
        public float rfPower, rfAngle;
        public float lrPower, lrAngle;
        public float rrPower, rrAngle;
        public float x, y, heading;
    }
}
