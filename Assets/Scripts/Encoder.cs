using System;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class Encoder : MonoBehaviour
{
    private float dist;
    private WheelCollider wheel;

    void Start()
    {
        dist = 0f;
        wheel = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        dist += GetVelocity() * Time.deltaTime * 2f * (float)Math.PI * wheel.radius * transform.localScale.y;
    }

    public void Reset()
    {
        dist = 0f;
    }

    public float GetPosition()
    {
        return dist;
    }

    public float GetVelocity()
    {
        return wheel.rpm / 60f;
    }
}
