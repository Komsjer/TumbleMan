using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbTestScript : MonoBehaviour {

    public float strength;


    ConfigurableJoint joint;
    float defaultspring;
    float maxforce;
    float damper; 

    // Use this for initialization
    void Start () {
        joint = GetComponent<ConfigurableJoint>();
        defaultspring = joint.slerpDrive.positionSpring;
        maxforce = joint.slerpDrive.maximumForce;
        damper = joint.slerpDrive.positionDamper;
    }
	
	// Update is called once per frame
	void Update () {

        Vector3 rotationVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            rotationVector.x = joint.highAngularXLimit.limit;
        }

        else if (Input.GetKey(KeyCode.S))
        {
            rotationVector.x = joint.lowAngularXLimit.limit;
        }

        if (Input.GetKey(KeyCode.D))
        {
            rotationVector.y = joint.angularYLimit.limit;
        }

        else if (Input.GetKey(KeyCode.A))
        {
            rotationVector.y = -joint.angularYLimit.limit;
            //rotationVector.z = - 1;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rotationVector.z = joint.angularZLimit.limit;
            //rotationVector.y = 1;
        }

        else if (Input.GetKey(KeyCode.E))
        {
            rotationVector.z = -joint.angularZLimit.limit;
            //rotationVector.y = - 1;
        }


        if (rotationVector == Vector3.zero)
        {
            joint.targetRotation = Quaternion.identity;
            JointDrive nslerpd = new JointDrive();
            nslerpd.positionSpring = defaultspring;
            nslerpd.maximumForce = maxforce;
            nslerpd.positionDamper = damper;
            joint.slerpDrive = nslerpd;
        }
        else
        {

            joint.targetRotation = Quaternion.Euler(Quaternion.identity.eulerAngles + rotationVector);
            //joint.targetRotation = new Quaternion(rotationVector.x, rotationVector.y, rotationVector.z, 1);
            JointDrive nslerpd = new JointDrive();
            nslerpd.positionSpring = defaultspring* strength;
            nslerpd.maximumForce = maxforce;
            nslerpd.positionDamper = damper;
            joint.slerpDrive = nslerpd;

        }

    }
}
