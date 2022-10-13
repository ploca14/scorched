using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyperfect.Common
{
    public class SceneReloader : MonoBehaviour
    {
        public KeyCode ReloadKey = KeyCode.Backslash;

        void Update()
        {
            if (Input.GetKeyDown(ReloadKey))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}