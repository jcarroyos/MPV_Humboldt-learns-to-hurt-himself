using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip gSound;
    public AudioClip sonidoArbusto;
    AudioSource fuenteAudio;
    void Start()
    {
        fuenteAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) {
            fuenteAudio.clip = gSound;
            fuenteAudio.Play();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            fuenteAudio.clip = gSound;
            fuenteAudio.Play();
        }

        
    }
}
