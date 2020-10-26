using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Noise 
{
    public float GenerateNoise(float x, float y)
    {
        float height = 0;

        height = Mathf.PerlinNoise(x/150, y/150);

        Debug.Log("Height before leaving function: " + height);

        return height;
    }





}
