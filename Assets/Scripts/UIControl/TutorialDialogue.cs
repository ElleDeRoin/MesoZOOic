using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Lines")]
    [TextArea(2, 5)]
    [SerializeField] private string[] lines;
    [SerializeField] private float secondsPerLetter = 0.03f;

    [Header("Walter Bob")]
    [SerializeField] private RectTransform characterImage;
    [SerializeField] private float bobHeight = 20f;
    [SerializeField] private float bobDuration = 0.3f;

    [Header("Animalcrossing Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip talkClip;
    [SerializeField] private float talkVolume = 1f;

    [Header("Load Sccene")]
    [SerializeField] private bool completesTutorial = true;
    [SerializeField] private string nextSceneName;

    private int index;
    private bool isTyping;
    private Coroutine typingRoutine;
    private Coroutine bobRoutine;
    private Vector2 characterRestPosition;
    private bool talkPaused;

    private void Awake()
    {
        if (characterImage != null)
            characterRestPosition = characterImage.anchoredPosition;

        if (audioSource == null && talkClip != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
            audioSource.volume = talkVolume;
            if (talkClip != null) audioSource.clip = talkClip;
        }
    }
    private void Start()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        index = 0;
        dialoguePanel.SetActive(true);
        ShowLine();
    }
    public void NextLine()
    {
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        index++;

        if (index < lines.Length)
            ShowLine();
        else
            EndDialogue();
    }


    private void ShowLine()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
        StartBob();
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        StartTalking();
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;

        dialogueText.ForceMeshUpdate();
        int total = dialogueText.textInfo.characterCount;

        for (int i = 1; i <= total; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(secondsPerLetter);
        }

        isTyping = false;
        PauseTalking();
    }
    private void StartTalking()
    {
        if (audioSource == null || talkClip == null) return;

        if (talkPaused)
        {
            audioSource.UnPause();
            talkPaused = false;
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void PauseTalking()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        audioSource.Pause();
        talkPaused = true;
    }

    private void FinishTyping()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        PauseTalking();
    }



    private void StartBob()
    {
        if (characterImage == null) return;

        if (bobRoutine != null) StopCoroutine(bobRoutine);
        characterImage.anchoredPosition = characterRestPosition;
        bobRoutine = StartCoroutine(Bob());
    }

    private IEnumerator Bob()
    {
        float time = 0f;

        while (time < bobDuration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / bobDuration);
            float offset = Mathf.Sin(progress * Mathf.PI) * bobHeight;

            characterImage.anchoredPosition = characterRestPosition + new Vector2(0f, offset);
            yield return null;
        }

        characterImage.anchoredPosition = characterRestPosition;
    }
    

    private void EndDialogue()
    {
        if (audioSource != null)
            audioSource.Stop();
        talkPaused = false;

        dialoguePanel.SetActive(false);

        if (completesTutorial)
            SaveSystem.TutorialComplete = true;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}