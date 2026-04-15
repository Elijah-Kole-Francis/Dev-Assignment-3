using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sound : MonoBehaviour
{
    public UnityEvent<Sound> OnSoundEmitted = new UnityEvent<Sound>();

    AudioSource audioSource;

    public virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Character>() != null)
        {
            audioSource.Play();
            OnSoundEmitted.Invoke(this);
        }
    }

}
