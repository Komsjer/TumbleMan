using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbTester : MonoBehaviour, Decision
{
    public int index;
    public int actionsize;
    public Quaternion tenserinputs;
    public bool tense;

    public float[] Decide(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
    {
        float[] action = new float[actionsize];
        if (tense && index*4 < actionsize && index >=0)
        {
            action[4 * index] = tenserinputs.x;
            action[4 * index + 1] = tenserinputs.y;
            action[4 * index + 2] = tenserinputs.z;
            action[4 * index + 3] = tenserinputs.w;
        }
        return action;

    }

    public float[] MakeMemory(List<float> state, List<Camera> observation, float reward, bool done, float[] memory)
    {
        return new float[0];
		
    }
}
