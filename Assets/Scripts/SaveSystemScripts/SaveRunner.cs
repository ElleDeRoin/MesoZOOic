using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveRunner : MonoBehaviour
{
    [SerializeField] private float autosaveInterval = 15f;

    private float timer;

    private void Start()
    {
        _ = DinoCollection.Instance;
        DecorationStore.Changed += SaveNow;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        DecorationStore.Changed -= SaveNow;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= autosaveInterval)
        {
            timer = 0f;
            SaveSystem.Save();
        }
    }

    private void SaveNow()
    {
        SaveSystem.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveSystem.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveSystem.Save();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}