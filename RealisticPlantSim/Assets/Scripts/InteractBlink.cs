using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractBlink : Interactable
{
    public int waitForSeconds = 2;

    public override void OnInteraction(GameObject source)
    {
        StartCoroutine(EnableAfterDelay());
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    private IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(waitForSeconds);
        gameObject.GetComponent<Renderer>().enabled = true;

    }
}
