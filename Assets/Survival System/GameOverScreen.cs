using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Polyperfect.Crafting.Demo;

public class GameOverScreen : MonoBehaviour
{

    public void RestartGame()
    {
        SceneManager.LoadScene("Planet");
    }
}
