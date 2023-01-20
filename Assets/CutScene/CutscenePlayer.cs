using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutscenePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextScene;
    private AsyncOperation asyncOperation;

    void Start()
    {
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Play();
        asyncOperation = SceneManager.LoadSceneAsync(nextScene);
        asyncOperation.allowSceneActivation = false;
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        asyncOperation.allowSceneActivation = true;
    }
}
