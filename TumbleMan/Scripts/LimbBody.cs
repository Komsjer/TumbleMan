using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbBody : MonoBehaviour
{
    public bool endOnTouch;
    public bool touchToState;
    public bool bodyToState = true;

    public int maxAngularVelocity = 7;

    [SpaceAttribute]
    public bool hasTorqueMotor;
    public float TorqueStrength;

    private bool isTouching;
    [HideInInspector]
    public bool hasFataled;

    [HideInInspector]
    public int stateCount;
    [HideInInspector]
    public int actionCount;

    Vector3 resetPosition;
    Quaternion resetRotation;

    public void Innit()
    {
        resetPosition = transform.position;
        resetRotation = transform.rotation;
        GetComponent<Rigidbody>().maxAngularVelocity = maxAngularVelocity;

        //Count the number of states and actions for this limb.
        stateCount = 13;
        if (touchToState) stateCount++;
        if (hasTorqueMotor) actionCount=3;
    }

    public List<float> GetState()
    {
        List<float> state = new List<float>();

        state.Add(transform.localPosition.x);
        state.Add(transform.localPosition.y);
        state.Add(transform.localPosition.z);

        state.Add(transform.localRotation.x);
        state.Add(transform.localRotation.y);
        state.Add(transform.localRotation.z);
        state.Add(transform.localRotation.w);

        Rigidbody rb = GetComponent<Rigidbody>();

        state.Add(rb.velocity.x);
        state.Add(rb.velocity.y);
        state.Add(rb.velocity.z);

        state.Add(rb.angularVelocity.x);
        state.Add(rb.angularVelocity.y);
        state.Add(rb.angularVelocity.z);

        // Touching the floor?
        if (touchToState)
        {
            state.Add(isTouching ? 1f : 0f);
        }

        isTouching = false;

        return state;

    }

    public void LimbStep(float xtorque, float ytorque, float ztorque)
    {
        GetComponent<Rigidbody>().AddTorque(transform.right * TorqueStrength * xtorque);
        GetComponent<Rigidbody>().AddTorque(Vector3.up * TorqueStrength * ytorque);
        GetComponent<Rigidbody>().AddTorque(transform.forward * TorqueStrength * ztorque);
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.collider.name == "Ground")
        {
            isTouching = true;
            if (endOnTouch)
            {
                hasFataled = true;
            }
        }
    }

    public void LimbReset()
    {
        isTouching = false;
        hasFataled = false;

        transform.position = resetPosition;
        transform.rotation = resetRotation;
        GetComponent<Rigidbody>().velocity = default(Vector3);
        GetComponent<Rigidbody>().angularVelocity = default(Vector3);
    }

}