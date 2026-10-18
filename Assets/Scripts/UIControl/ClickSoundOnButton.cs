using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickSoundOnButton : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private static AudioSource sharedSource;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        if (clickSound == null)
            return;

        if (sharedSource == null)
        {
            GameObject go = new GameObject("ClickSoundPlayer");
            DontDestroyOnLoad(go);

            sharedSource = go.AddComponent<AudioSource>();
            sharedSource.playOnAwake = false;
            sharedSource.spatialBlend = 0f;
        }

        sharedSource.PlayOneShot(clickSound, volume);
    }
}