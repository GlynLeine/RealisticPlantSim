using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTracks : MonoBehaviour
{
    private RaycastHit hit;
    // Update is called once per frame

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                GameObject chunk = hit.collider.gameObject;
                Material chunkMaterial = chunk.GetComponent<MeshRenderer>().material;
                Debug.Log(chunk);
                RenderTexture temp = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(chunkMaterial.GetTexture("_HeightMap"), temp);
                RenderTexture.ReleaseTemporary(temp);
            }
        }
    }
}
