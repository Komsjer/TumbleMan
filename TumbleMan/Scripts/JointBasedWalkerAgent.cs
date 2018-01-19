using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointBasedWalkerAgent : Agent
{

    [Header("MoveIt")]
    public Transform body; // The point from wich the agent gets its velocity state
    public bool bodyState;
    LimbBody[] limbs; // The limbs of the agent
    LimbJoint[] joints; // The joints of the agent

    Vector3 past_velocity; // Used to calculate the velocity change in state

    Vector3 reset_body_position; // The reset location for the body
    Quaternion reset_body_rotation; // The reset rotation for the body

    public bool magicForce; // Magic force adds a torque and force to the body component of your agent.
    public Vector3 magicForceMagnitude; // Magic force added to the agent based on the agents action.
    public Vector3 magicTorqueMagnitude; // Magic torque added to the agent based on the agents action.

    public enum gametypes
    {
        stand,
        walk,
        path,
        mouse,
    }

    public gametypes gametype;



    [Header("Walk")]
    public bool showWalkDirection;
    public bool walkRandomDirection;
    Vector3 walkDirection;


    [Header("Path")]
    public bool showPath;
    public float fieldSize = 5f;
    public float capture_distance = 1f;
    Vector3 pathingPoint;



    public override void InitializeAgent()
    {
        //Innitialize the limbs and joints
        limbs = GetComponentsInChildren<LimbBody>();
        joints = GetComponentsInChildren<LimbJoint>();

        reset_body_position = body.transform.position;
        reset_body_rotation = body.transform.rotation;
        foreach (LimbBody limb in limbs)
        {
            limb.Innit();
        }

        foreach (LimbJoint joint in joints)
        {
            joint.Innit();
        }
        //pick a walking direction
        if (walkRandomDirection)
        {
            Vector2 circle = Random.insideUnitCircle.normalized;
            walkDirection = new Vector3(circle.x, 0, circle.y);
        }
        else
        {
            walkDirection = Vector2.right;
        }
        if (gametype == gametypes.path)
        {
            pathingPoint = new Vector3(reset_body_position.x + Random.Range(-fieldSize, fieldSize), reset_body_position.y, reset_body_position.z + Random.Range(-fieldSize, fieldSize));
        }

    }

    public override List<float> CollectState()
    {
        List<float> state = new List<float>();
        //Body state
        if (bodyState)
        {
            state.Add(body.transform.rotation.eulerAngles.x);
            state.Add(body.transform.rotation.eulerAngles.y);
            state.Add(body.transform.rotation.eulerAngles.z);

            state.Add(body.gameObject.GetComponent<Rigidbody>().velocity.x);
            state.Add(body.gameObject.GetComponent<Rigidbody>().velocity.y);
            state.Add(body.gameObject.GetComponent<Rigidbody>().velocity.z);

            state.Add((body.gameObject.GetComponent<Rigidbody>().velocity.x - past_velocity.x) / Time.fixedDeltaTime);
            state.Add((body.gameObject.GetComponent<Rigidbody>().velocity.y - past_velocity.y) / Time.fixedDeltaTime);
            state.Add((body.gameObject.GetComponent<Rigidbody>().velocity.z - past_velocity.z) / Time.fixedDeltaTime);
        }
            past_velocity = body.gameObject.GetComponent<Rigidbody>().velocity;

        //Limb states
        foreach (LimbBody limb in limbs)
        {
            state.AddRange(limb.GetState());
        }


        //Calculating forward vector
        Vector3 center = body.position;
        center.y = 0;
        Vector3 foreward = body.forward;
        foreward.y = 0;
        foreward.Normalize();
        Vector3 towardTarget = foreward;

        //state for walking game
        if (gametype == gametypes.walk)
        {
            state.Add(walkDirection.x);
            state.Add(walkDirection.z);
            Debug.Log(walkDirection.x.ToString() + "," + walkDirection.z.ToString());

            towardTarget = walkDirection;
            towardTarget.y = 0;

        }
        //state for path game
        else if (gametype == gametypes.path)
        {


            Vector3 pathing_direction = pathingPoint - body.position;
            pathing_direction.Normalize();
            state.Add(pathing_direction.x);
            state.Add(pathing_direction.z);

            towardTarget = pathing_direction;
            towardTarget.y = 0;

            Debug.DrawLine(center, center + foreward * 3, Color.green);
            Debug.DrawLine(center, center + towardTarget * 3, Color.magenta);
            Debug.Log(Vector3.SignedAngle(foreward, towardTarget,Vector3.up)/180f);




        }
        //state for mouse game
        else if (gametype == gametypes.mouse)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 mouse_pathing_direction = hit.point - body.position;
                mouse_pathing_direction.Normalize();
                state.Add(mouse_pathing_direction.x);
                state.Add(mouse_pathing_direction.z);
            }
            else
            {
                state.Add(0f);
                state.Add(0f);
            }
        }
        //state for standing
        else
        {
            state.Add(0f);
            state.Add(0f);
        }

        //Add a heading angle state
        state.Add(Vector3.SignedAngle(foreward, towardTarget, Vector3.up) / 180f);


        return state;
    }

    public override void AgentStep(float[] act)
    {
        
        //Let each joint and limb take it's respective action
        int i = 0;
        foreach (LimbJoint joint in joints)
        {
            float xrot = Mathf.Max(Mathf.Min(act[i], 1), -1);
            i++;
            float yrot = Mathf.Max(Mathf.Min(act[i], 1), -1);
            i++;
            float zrot = Mathf.Max(Mathf.Min(act[i], 1), -1);
            i++;
            float tension = Mathf.Max(Mathf.Min(act[i], 1), -1);

            joint.LimbStep(xrot, yrot, zrot, tension);
            i++;
        }
        foreach (LimbBody limb in limbs)
        {
            if (limb.hasTorqueMotor)
            {
                float xtorque = Mathf.Max(Mathf.Min(act[i], 1), -1);
                i++;
                float ytorque = Mathf.Max(Mathf.Min(act[i], 1), -1);
                i++;
                float ztorque = Mathf.Max(Mathf.Min(act[i], 1), -1);

                limb.LimbStep(xtorque, ytorque, ztorque);
                i++;
            }
        }

        //Magic force, adds a force and torque to the body component of the agent
        if (magicForce)
        {
            Vector3 mforce = new Vector3(magicForceMagnitude.x * Mathf.Max(Mathf.Min(act[i], 1), -1),
                                        magicForceMagnitude.y * Mathf.Max(Mathf.Min(act[i+1], 1), -1),
                                        magicForceMagnitude.z * Mathf.Max(Mathf.Min(act[i+2], 1), -1));
            body.GetComponent<Rigidbody>().AddForce(mforce);
            i += 3;

            Vector3 mtorque = new Vector3(magicTorqueMagnitude.x * Mathf.Max(Mathf.Min(act[i], 1), -1),
                                                    magicTorqueMagnitude.y * Mathf.Max(Mathf.Min(act[i + 1], 1), -1),
                                                    magicTorqueMagnitude.z * Mathf.Max(Mathf.Min(act[i + 2], 1), -1));
            body.GetComponent<Rigidbody>().AddTorque(mtorque);
        }

        //reward

        //reward for stand game
        if (gametype == gametypes.stand)
        {
            reward = 0.1f;
        }

        //reward for walk game
        if (gametype == gametypes.walk)
        {
            if (walkRandomDirection)
            {
                reward = 0.1f
                + 1.0f * (body.GetComponent<Rigidbody>().velocity.x * walkDirection.x + body.GetComponent<Rigidbody>().velocity.z * walkDirection.z);
                Debug.Log(reward);
            }
            else
            {
                reward = 0.1f
                + 1f * body.GetComponent<Rigidbody>().velocity.x;
            }

        }
        //reward for path game
        if (gametype == gametypes.path)
        {
            Vector3 pathing_direction = pathingPoint - body.position;
            float pathing_distance = pathing_direction.magnitude;
            pathing_direction.Normalize();
            //create a new path
            if (pathing_distance < capture_distance)
            {
                reward = 10f;
                pathingPoint = new Vector3(reset_body_position.x + Random.Range(-fieldSize, fieldSize), reset_body_position.y, reset_body_position.z + Random.Range(-fieldSize, fieldSize));
            }
            else
            {
                reward = 0.1f
                + 1.0f * (body.GetComponent<Rigidbody>().velocity.x * pathing_direction.x + body.GetComponent<Rigidbody>().velocity.z * pathing_direction.z);
            }

        }


        //Check if the agent has died by touching the floor
        foreach (LimbBody limb in limbs)
        {
            if (limb.hasFataled)
            {
                reward = -1f ;
                done = true;
            }
        }

        //ShowDebugLines
        if (gametype == gametypes.walk && showWalkDirection)
        {
            Debug.DrawLine(body.position, body.position + walkDirection * 4, Color.red);
        }
        if (gametype == gametypes.path && showPath)
        {
            Debug.DrawLine(body.position, pathingPoint, Color.blue);
        }
        else if (gametype == gametypes.mouse)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Debug.DrawLine(body.position, hit.point , Color.cyan);
            }
        }
        }

    public override void AgentReset()
    {
        //Resets body component
        body.transform.position = reset_body_position;
        body.transform.rotation = reset_body_rotation;
        body.GetComponent<Rigidbody>().velocity = default(Vector3);
        body.GetComponent<Rigidbody>().angularVelocity = default(Vector3);

        //Reset all the limbs
        foreach (LimbBody limb in limbs)
        {
            limb.LimbReset();
        }

        //Reset joints
        foreach (LimbJoint joint in joints)
        {
            joint.LimbReset();
        }

        //reset for walk game
        if (gametype == gametypes.walk)
        {
            if (walkRandomDirection)
            {
                Vector2 circle = Random.insideUnitCircle.normalized;
                walkDirection = new Vector3(circle.x, 0, circle.y);
            }
            else
            {
                walkDirection = Random.value > 0.5 ? Vector2.right : -Vector2.right;
            }
        }
        //reset for path game
        if (gametype == gametypes.path)
        {
            pathingPoint = new Vector3(reset_body_position.x + Random.Range(-fieldSize, fieldSize), reset_body_position.y, reset_body_position.z + Random.Range(-fieldSize, fieldSize));
        }

    }

    public override void AgentOnDone()
    {

    }
}
