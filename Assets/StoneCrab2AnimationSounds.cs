using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCrab2AnimationSounds : MonoBehaviour
{
    AudioSource animationSoundStoneCrab2;

    // Start is called before the first frame update
    void Start()
    {
        animationSoundStoneCrab2 = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StoneCrab2AttackSound(){
        animationSoundStoneCrab2.Play();
    }
}
