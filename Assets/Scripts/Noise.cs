using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

static public class Noise
{
    public static float GenerateNoise(float x, float y, float Seed)
    {
        float height = 0;

        height = Mathf.PerlinNoise((x + Seed) / 300, (y + Seed) / 300) - 0.5f;

        return height;
    }

    public static float GenerateNoise(float x, float y, float Seed, float skylandRadius, float declinePrecent, float precentDistanceFromTheEdge)
    {
        float height = 0;

        if (precentDistanceFromTheEdge < 0.00001)
        {
            height = (Mathf.PerlinNoise((x + Seed) / 300, (y + Seed) / 300) - 0.5f) * declinePrecent * 8;
            if (height < 0) return 0;
        }
        else
        {
            height = (Mathf.PerlinNoise((x + Seed) / 300, (y + Seed) / 300) - 0.5f);
            Debug.Log("not zero");
        }
        return height;
    }




}
