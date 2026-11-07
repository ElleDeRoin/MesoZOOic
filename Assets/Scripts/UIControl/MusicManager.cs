using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneTrack
{
    public string label;

    public AudioClip clip;
    public string[] scenes;
}
public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    public static MusicManager Instance => instance;

    [Header("Tracks")]
    [SerializeField] private SceneTrack[] tracks;
    [SerializeField] private AudioClip defaultClip;

    [Header("Mix")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.6f;
    [SerializeField] private float fadeSeconds = 1f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource active;
    private AudioClip currentClip;
    private Coroutine fadeRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        MusicManager prefab = Resources.Load<MusicManager>("MusicManager");
        if (prefab == null)
        {
            Debug.LogWarning("not in first scene silly");
            return;
        }

        Instantiate(prefab);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;
        DontDestroyOnLoad(gameObject);

        sourceA = CreateSource();
        sourceB = CreateSource();
        active = sourceA;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        if (instance == this && currentClip == null)
            Play(ClipFor(SceneManager.GetActiveScene().name));
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private AudioSource CreateSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = 0f;
        return src;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        Play(ClipFor(scene.name));
    }
    private AudioClip ClipFor(string sceneName)
    {
        if (tracks != null)
        {
            foreach (SceneTrack t in tracks)
            {
                if (t == null || t.scenes == null) continue;

                foreach (string s in t.scenes)
                    if (s == sceneName)
                        return t.clip;
            }
        }


        return defaultClip;
    }

    public void Play(AudioClip clip)
    {
        if (clip == currentClip && (clip == null || active.isPlaying))
            return;

        currentClip = clip;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade(clip));
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);

        if (fadeRoutine == null && active != null)
            active.volume = volume;
    }

    private IEnumerator Crossfade(AudioClip clip)
    {
        AudioSource from = active;
        AudioSource to = active == sourceA ? sourceB : sourceA;

        if (clip != null)
        {
            to.clip = clip;
            to.volume = 0f;
            to.Play();
            active = to;
        }
        float fromStart = from.volume;
        float time = 0f;
        float duration = Mathf.Max(0.01f, fadeSeconds);

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(time / duration);

            from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (clip != null) to.volume = Mathf.Lerp(0f, volume, k);

            yield return null;
        }

        from.Stop();
        from.volume = 0f;
        if (clip != null) to.volume = volume;

        fadeRoutine = null;
    }
}