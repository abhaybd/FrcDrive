using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PIDMaster : MonoBehaviour {

    public Rigidbody robot;
    public GameObject target;
    public InputField kPInput, kIInput, kDInput;

    private float kP, kI, kD;
    private float targetPos;
    private float lastError = 0;
    private float accumulator = 0;
    private bool started = true;
    private bool running = false;

    void Start()
    {
        StartClosedLoop();
    }

    void FixedUpdate()
    {
        if (running)
        {
            robot.AddForce(0, 0, GetOutput());
        }
    }

    public void StartClosedLoop()
    {
        targetPos = Random.Range(5f, 30);
        robot.drag = Random.Range(0.01f, 0.15f);
        robot.mass = Random.Range(1f, 5f);
        Reset();
    }

    public void StartTrial()
    {
        kP = float.Parse(kPInput.text == "" ? "0.0" : kPInput.text);
        kI = float.Parse(kIInput.text == "" ? "0.0" : kIInput.text);
        kD = float.Parse(kDInput.text == "" ? "0.0" : kDInput.text);
        running = true;
    }

    public void Reset()
    {
        robot.transform.position = new Vector3(0, 0.5f, 0);
        robot.velocity = new Vector3(0f, 0f, 0f);
        target.transform.position = new Vector3(0, target.transform.position.y, targetPos);
        accumulator = 0;
        lastError = 0;
        started = true;
        running = false;
    }

    private float GetOutput()
    {
        float error = targetPos - robot.transform.position.z;
        accumulator += error * Time.fixedDeltaTime;
        float derivative;
        if (started)
        {
            derivative = 0;
            started = false;
        }
        else
        {
            derivative = (error - lastError) / Time.fixedDeltaTime;
        }
        float output = kP * error + kI * accumulator + kD * derivative;
        lastError = error;
        return output;
    }
}
