using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HatchResultScreen : MonoBehaviour
{
    [Header("Dino")]
    public Image dinoImage;
    public TMP_Text nameText;
    public TMP_Text nextButtonLabel;
    public Button nextButton;
    public UnityEvent onFinished;

    [Header("Egg animation")]
    public Image eggImage;
    public Sprite[] regularEggFrames;
    public Sprite[] paidEggFrames;
    public float framesPerSecond = 12f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hatchSound;
    [Range(0f, 1f)]
    [SerializeField] private float hatchSoundVolume = 1f;

    private int index;
    private bool revealed;
    private bool hatching;

    private Sprite[] CurrentFrames =>
        EggHatcher.LastHatchWasPaid ? paidEggFrames : regularEggFrames;

    private void OnEnable()
    {
        index = 0;
        hatching = false;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNext);
        nextButton.interactable = true;
        ShowEgg();
    }

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void OnNext()
    {
        if (hatching) return;

        if (!revealed)
        {
            StartCoroutine(HatchRoutine());
            return;
        }

        index++;
        if (index >= EggHatcher.LastHatch.Count) { onFinished?.Invoke(); return; }
        ShowEgg();
    }

    private void ShowEgg()
    {
        dinoImage.gameObject.SetActive(false);
        eggImage.gameObject.SetActive(true);

        var frames = CurrentFrames;
        if (frames != null && frames.Length > 0)
            eggImage.sprite = frames[0];
        else
            Debug.LogWarning("You forgot to assign the frames Dawg.");

        nameText.text = "[???]!";
        nextButtonLabel.text = "Reveal";
        revealed = false;
    }

    private IEnumerator HatchRoutine()
    {
        hatching = true;
        nextButton.interactable = false;

        PlayHatchSound();

        var frames = CurrentFrames;
        if (frames != null && frames.Length > 0)
        {
            float delay = 1f / framesPerSecond;
            for (int i = 0; i < frames.Length; i++)
            {
                eggImage.sprite = frames[i];
                yield return new WaitForSecondsRealtime(delay);
            }
        }

        Reveal();
        nextButton.interactable = true;
        hatching = false;
    }

    private void PlayHatchSound()
    {
        if (audioSource == null || hatchSound == null)
            return;

        audioSource.PlayOneShot(hatchSound, hatchSoundVolume);
    }
    
    private void Reveal()
    {
        var dino = EggHatcher.LastHatch[index];

        eggImage.gameObject.SetActive(false);
        dinoImage.sprite = dino.icon;
        dinoImage.color = Color.white;
        dinoImage.gameObject.SetActive(true);

        nameText.text = $"[{dino.displayName}]!";
        bool last = index >= EggHatcher.LastHatch.Count - 1;
        nextButtonLabel.text = last ? "Done" : "Next";
        revealed = true;
    }
}