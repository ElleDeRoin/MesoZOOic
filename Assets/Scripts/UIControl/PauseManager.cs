using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private static int activeCount;

    public static bool IsPaused => activeCount > 0;
    [SerializeField] private bool pauseAudio = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        activeCount = 0;
    }

    private void OnEnable()
    {
        activeCount++;
        Apply();
    }

    private void OnDisable()
    {
        activeCount = Mathf.Max(0, activeCount - 1);
        Apply();
    }

    private void Apply()
    {
        bool paused = IsPaused;

        Time.timeScale = paused ? 0f : 1f;

        if (pauseAudio)
            AudioListener.pause = paused;
    }
}