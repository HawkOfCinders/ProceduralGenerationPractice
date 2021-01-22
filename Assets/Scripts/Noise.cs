using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

static public class Noise 
{
    public static float GenerateNoise(float x, float y, float Seed)
    {
        float height = 0;

        height = Mathf.PerlinNoise((x + Seed)/300, (y + Seed)/300);

        return height;
    }





}
