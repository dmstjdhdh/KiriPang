using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroll : MonoBehaviour
{
    public Vector3 initPos;
    public Vector3 destPos;

    public Vector3 dir;

    Transform tr;

    private void Awake() 
    {
        tr = this.transform;
    }

    private void Update()
    {
        tr.Translate(dir);

        if (Vector3.Distance(destPos, tr.localPosition) <= 1f)
        {
            tr.localPosition = initPos;
        }            
    }
}
