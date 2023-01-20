using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCrabAnimationSounds : MonoBehaviour
{
    AudioSource animationSoundStoneCrab;

    // Start is called before the first frame update
    void Start()
    {
        animationSoundStoneCrab = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StoneCrabAttackSound(){
        animationSoundStoneCrab.Play();
    }
}
