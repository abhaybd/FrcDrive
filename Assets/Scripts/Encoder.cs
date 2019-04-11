using System;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class Encoder : MonoBehaviour
{
    private float dist;
    private WheelCollider wheel;

    void Awake()
    {
        dist = 0f;
        wheel = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        dist += GetTangentialVelocity() * Time.deltaTime;
    }

    public void Reset()
    {
        dist = 0f;
    }

    public float GetPosition()
    {
        return dist;
    }

    public float GetTangentialVelocity()
    {
        return GetRotationalVelocity() * 2f * (float)Math.PI * wheel.radius * transform.localScale.y;
    }

    public float GetRotationalVelocity()
    {
        return wheel.rpm / 60f;
    }
}
