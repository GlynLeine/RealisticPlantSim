using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInteraction : MonoBehaviour
{

    public KeyCode interactKey = KeyCode.F;
    public float forwardOffset = 1f;
    public float maxDistance = 15f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Debug.DrawRay(transform.position + transform.forward * forwardOffset, transform.forward * maxDistance, Color.red);
            RaycastHit hit;
            if(Physics.Raycast(transform.position + transform.forward * forwardOffset, transform.forward, out hit, maxDistance))
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log(hitObject.name);

                Interactable interactable = hitObject.GetComponent<Interactable>();
                if (interactable != null)
                {
                    interactable.OnInteraction(gameObject);

                }
            }
        }
        
    }
}
