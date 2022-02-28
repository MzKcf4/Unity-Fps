using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FallToGround : MonoBehaviour
{
    private void Start()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 100f, LayerMask.GetMask(Constants.LAYER_GROUND)))
        {
            transform.position = hit.point;
        }
    }
}

