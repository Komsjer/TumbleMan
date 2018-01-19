using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanWalkAgent : Agent {

    public Transform body; // The point from wich the agent gets its velocity state
    public Limb[] limbs; // The limbs of the agent

    Vector3 past_velocity; // Used to calculate the velocity change in state

    public override void InitializeAgent()
    {
        //Innitialize all the limbs
        foreach (Limb limb in limbs)
        {
            limb.Innit();
        }
    }

    public override List<float> CollectState()
	{
		List<float> state = new List<float>();
        //Add body state

        state.Add(body.transform.rotation.eulerAngles.x);
        state.Add(body.transform.rotation.eulerAngles.y);
        state.Add(body.transform.rotation.eulerAngles.z);

        state.Add(body.gameObject.GetComponent<Rigidbody>().velocity.x);
        state.Add(body.gameObject.GetComponent<Rigidbody>().velocity.y);
        state.Add(body.gameObject.GetComponent<Rigidbody>().velocity.z);

        state.Add((body.gameObject.GetComponent<Rigidbody>().velocity.x - past_velocity.x) / Time.fixedDeltaTime);
        state.Add((body.gameObject.GetComponent<Rigidbody>().velocity.y - past_velocity.y) / Time.fixedDeltaTime);
        state.Add((body.gameObject.GetComponent<Rigidbody>().velocity.z - past_velocity.z) / Time.fixedDeltaTime);
        past_velocity = body.gameObject.GetComponent<Rigidbody>().velocity;

        //Add Lib states
        foreach (Limb limb in limbs)
        {
            state.AddRange(limb.GetState());
        }

		return state;
	}

	public override void AgentStep(float[] act)
	{
        //Let each limb take it's respective action
        int i = 0;
        foreach (Limb limb in limbs)
        {
            float action1 = Mathf.Max(Mathf.Min(act[i], 1), -1);
            i++;
            float action2 = Mathf.Max(Mathf.Min(act[i], 1), -1);

            limb.LimbStep(action1, action2);
            i++;
        }

        //reward
        //reward = 0.1f + body.transform.position.y;
        reward = 1.0f * body.GetComponent<Rigidbody>().velocity.x;

        //Check if the agent has died by touching the floor
        foreach (Limb limb in limbs)
        {
            if (limb.hasFataled)
            {
                reward = -1f;
                done = true;
            }
        }
	}

	public override void AgentReset()
	{
        //Reset all the limbs
        foreach (Limb limb in limbs)
        {
            limb.LimbReset();
        }
    }

	public override void AgentOnDone()
	{

	}
}
