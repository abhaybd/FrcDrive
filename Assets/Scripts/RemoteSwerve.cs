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

    private DCMotor motor = new Neo();

    private Rigidbody rb;

    private Encoder lfEncoder;
    private Encoder rfEncoder;
    private Encoder rrEncoder;
    private Encoder lrEncoder;

    private string objectName = "remoteSwerve";
    private string remoteGyroName = "gyro";
    private SwerveStatus status = new SwerveStatus();
    private float gearing;

    private Vector3[,] rays = new Vector3[4,2];

    // Use this for initialization
    void Start()
    {
        lfEncoder = lfWheel.GetComponent<Encoder>();
        rfEncoder = rfWheel.GetComponent<Encoder>();
        rrEncoder = rrWheel.GetComponent<Encoder>();
        lrEncoder = lrWheel.GetComponent<Encoder>();

        rb = GetComponent<Rigidbody>();

        gearing = maxWheelSpeed / motor.FreeSpeed; // output over input
        Debug.Log("Gearing: " + gearing);
        float width = (rfWheel.transform.position - lfWheel.transform.position).magnitude;
        float length = (rfWheel.transform.position - rrWheel.transform.position).magnitude;
        Debug.Log(rfWheel.transform.position + ", " + lfWheel.transform.position);
        Debug.Log(rfWheel.transform.position + ", " + rrWheel.transform.position);
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


        DrawWheelVecs(lfWheel, status.lfPower, 0);
        DrawWheelVecs(rfWheel, status.rfPower, 1);
        DrawWheelVecs(lrWheel, status.lrPower, 2);
        DrawWheelVecs(rrWheel, status.rrPower, 3);

        Vector3 vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Debug.DrawRay(transform.position, vel, Color.green);
    }

    private void DrawWheelVecs(WheelCollider wheel, float power, int index)
    {
        if (Input.anyKey)
        {
            Vector3 tangent = Vector3.Cross(wheel.transform.parent.position - wheel.transform.position, Vector3.up);
            float omega = rb.angularVelocity.y;
            tangent *= omega;
            Vector3 vel = tangent + rb.velocity;
            vel.y = 0f;
            Debug.DrawRay(wheel.transform.position, vel, Color.red);

            Vector3 steerVec = new Vector3(Mathf.Sin(wheel.transform.eulerAngles.y * Mathf.Deg2Rad), 0f, Mathf.Cos(wheel.transform.eulerAngles.y * Mathf.Deg2Rad));
            steerVec *= wheel.gameObject.GetComponent<Encoder>().GetTangentialVelocity();
            Debug.DrawRay(wheel.transform.position, steerVec, Color.cyan);
            rays[index, 0] = vel;
            rays[index, 1] = steerVec;
        }
        else
        {
            Debug.DrawRay(wheel.transform.position, rays[index,0], Color.red);
            Debug.DrawRay(wheel.transform.position, rays[index,1], Color.cyan);
        }
    }

    void OnDestroy()
    {
        RPC.Instance.Close();
    }

    private void SetWheel(WheelCollider wheel, float power, float angle)
    {
        angle = angle % 360f;
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
            float turn = Input.GetAxis("Turn") / 2f;
            float heading = transform.eulerAngles.y;

            RPC.Instance.ExecuteMethod<object>(remoteGyroName, "setHeading", new string[] { "java.lang.Double" }, new object[] { heading });

            status = RPC.Instance.ExecuteMethod<SwerveStatus>(objectName, "getStatus",
                new string[] { "java.lang.Double", "java.lang.Double", "java.lang.Double" },
                new object[] { x, y, turn });

            RPC.Instance.ExecuteMethod<SwerveStatus>(objectName, "updateOdometry",
                new string[] { "java.lang.Double", "java.lang.Double", "java.lang.Double", "java.lang.Double" },
                new object[] { lfEncoder.GetTangentialVelocity(), rfEncoder.GetTangentialVelocity(), lrEncoder.GetTangentialVelocity(), rrEncoder.GetTangentialVelocity() });
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
    }
}
