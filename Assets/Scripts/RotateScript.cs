using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    public float speed = 1f;
    public float amplitude = 1f;
    public float floatSpeed = 1.0f;
    public float offset = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.PingPong(Time.time * floatSpeed, 1) * amplitude - amplitude / 2 + offset;
        Vector3 pos = new Vector3(transform.position.x, y, transform.position.z);
        transform.position = pos;
        transform.Rotate(speed * Time.deltaTime, speed * Time.deltaTime, 0);
    }
}
