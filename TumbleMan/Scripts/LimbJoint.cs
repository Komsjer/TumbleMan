using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbJoint : MonoBehaviour {

    public float strength; // Joint strength is stength * original positionSpring * agent action 
    public int jointIndex; //Index of the targeted joint, used for objects with multiple joints

    private ConfigurableJoint joint;
    float resetspring;
    float resetmaxforce;
    float resetdamper;

    [HideInInspector]
    public int actionCount;

    // Use this for initialization
    public void Innit()
    {
        joint = GetComponents<ConfigurableJoint>()[jointIndex];
        resetspring = joint.slerpDrive.positionSpring;
        resetmaxforce = joint.slerpDrive.maximumForce;
        resetdamper = 2;// joint.slerpDrive.positionDamper;
    }

    public void LimbStep(float xrot, float yrot, float zrot, float tension)
    {
        Vector3 rotationVector = Vector3.zero;
        rotationVector.x = (xrot > 0) ? joint.highAngularXLimit.limit * xrot : joint.lowAngularXLimit.limit * -xrot;
        rotationVector.y = joint.angularYLimit.limit * yrot;
        rotationVector.z = joint.angularZLimit.limit * zrot;

        joint.targetRotation = Quaternion.Euler(Quaternion.identity.eulerAngles + rotationVector);
        JointDrive nslerpd = new JointDrive();
        nslerpd.positionSpring = (tension>0) ? resetspring * strength * tension : resetspring * (1f- Mathf.Abs(tension));
        nslerpd.maximumForce = resetmaxforce;
        nslerpd.positionDamper = resetdamper;
        joint.slerpDrive = nslerpd;
    }


    public void LimbReset()
    {
        joint.targetRotation = Quaternion.identity;
        JointDrive nslerpd = new JointDrive();
        nslerpd.positionSpring = resetspring;
        nslerpd.maximumForce = resetmaxforce;
        nslerpd.positionDamper = resetdamper;
        joint.slerpDrive = nslerpd;

    }

}
