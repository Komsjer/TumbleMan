using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    public float strength;

    public bool endOnTouch;
    public bool touchToState;

    private bool isTouching;
    [HideInInspector]
    public bool hasFataled;

    Vector3 resetPosition;
    Quaternion resetRotation;

    ConfigurableJoint joint;

    public void Innit()
    {
        resetPosition = transform.position;
        resetRotation = transform.rotation;
        GetComponent<Rigidbody>().maxAngularVelocity = 45;
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

    public void LimbStep(float action1, float action2)
    {
        GetComponent<Rigidbody>().AddForce(-transform.right * strength * action1);
        GetComponent<Rigidbody>().AddForce(-transform.forward * strength * action2);

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