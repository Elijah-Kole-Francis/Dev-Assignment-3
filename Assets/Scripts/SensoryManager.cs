using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SensoryManager : MonoBehaviour
{
    public GameObject soundObject;

    public StateMachineSimple npc;
    public WitchStateMachine witch;

    public void OnEnable()
    {
        foreach (Transform transform in soundObject.transform)
        {
            Sound soundObject = transform.GetComponent<Sound>();
            if (soundObject != null)
            {
                soundObject.OnSoundEmitted.AddListener(witch.SoundRecieve);
            }
        }
    }

    public void OnDisable()
    {
        foreach (Transform transform in soundObject.transform)
        {
            Sound soundObject = transform.GetComponent<Sound>();
            if (soundObject != null)
            {
                soundObject.OnSoundEmitted.RemoveListener(witch.SoundRecieve);
            }
        }
    }



}
