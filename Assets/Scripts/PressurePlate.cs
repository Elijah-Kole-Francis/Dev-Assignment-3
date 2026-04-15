using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Sound
{


    MeshRenderer meshRenderer;
    public Material materialNormal;
    public Material materialPressed;

    public override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponent<MeshRenderer>();

    }


    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        meshRenderer.material = materialPressed;
    }

    public void OnTriggerExit(Collider other)
    {
        meshRenderer.material = materialNormal;
    }


}
