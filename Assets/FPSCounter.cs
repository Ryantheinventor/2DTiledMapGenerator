using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text text;

    private float[] lastFrames = new float[100];
    private int index = 0;
    void Update() 
    {
        lastFrames[index] = (1f/Time.deltaTime);
        text.text = "FPS:" + (int)Avg(lastFrames);
        index++;
        if(index >= lastFrames.Length)
        {
            index = 0;
        }
    }

    private float Avg(float[] numbers)
    {
        float total = 0;
        foreach(float f in numbers)
        {
            total += f;
        }
        return total / numbers.Length;
    }

}
