using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    public TerrainGenerator generator;
    private int rotateSpeed = 100;
    private void Start()
    {
        generator = GameObject.FindGameObjectWithTag("Terrain").GetComponent<TerrainGenerator>();
        
        
    }
    private void Update()
    {
        transform.Rotate(0, transform.rotation.z + Input.GetAxisRaw("Horizontal") * rotateSpeed * Time.deltaTime, 0);
        if (Input.GetMouseButtonDown(0))
        {
            generator.Seed = Random.Range(0, 2121213);
            generator.Initiate();
        }
    }
}
