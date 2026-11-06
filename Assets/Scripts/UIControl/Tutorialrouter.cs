using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialRouter
{
    public const string TutorialSceneName = "TutorialStart";



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RouteFirstLaunch()
    {
        if (SaveSystem.TutorialComplete) return;
        if (SceneManager.GetActiveScene().buildIndex != 0) return;

        if (!Application.CanStreamedLevelBeLoaded(TutorialSceneName))
        {
            Debug.LogError($"'{TutorialSceneName}' not in settings silly.");
            return;
        }
        SceneManager.LoadScene(TutorialSceneName);

    }
}