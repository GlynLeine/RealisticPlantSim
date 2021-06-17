using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDriving : MonoBehaviour
{
    public KeyCode forwardKey;
    public KeyCode backwardKey;
    public KeyCode leftKey;
    public KeyCode rightKey;

    public bool superSecretKartMode = false;

    public GameObject terrainGenerator;

    public float moveSpeed = 5;
    public float turnSpeed = 20;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();
        Vector3 rotation = new Vector3();

        if (Input.GetKey(forwardKey))
        {
            movement += transform.forward * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(backwardKey))
        {
            movement -= transform.forward * moveSpeed * Time.deltaTime;
        }

        if(Input.GetKey(leftKey))
        {
            rotation -= Vector3.up * turnSpeed * Time.deltaTime;
        }

        if(Input.GetKey(rightKey))
        {
            rotation += Vector3.up * turnSpeed * Time.deltaTime;
        }

        if (superSecretKartMode)
        {
            movement *= 5;
            rotation *= 5;
        }

        transform.position += movement;
        transform.Rotate(rotation);
    }
}
