using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDemonAnimationSounds : MonoBehaviour
{
    AudioSource animationSoundFireDemon;
    
    // Start is called before the first frame update
    void Start()
    {
        animationSoundFireDemon = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FireDemonAttackSound(){
        animationSoundFireDemon.Play();
    }
}
