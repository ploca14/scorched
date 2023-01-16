using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenWormAnimationSounds : MonoBehaviour
{
    AudioSource animationSoundQueenWorm;

    // Start is called before the first frame update
    void Start()
    {
        animationSoundQueenWorm = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void QueenWormAttackSound(){
        animationSoundQueenWorm.Play();
    }
}
