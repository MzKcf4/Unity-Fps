using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMarker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        if(r != null)
            r.enabled = false;

        FallToGround();
    }

    private void FallToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 100f, LayerMask.GetMask(Constants.LAYER_GROUND))) {
            transform.position = hit.point;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
