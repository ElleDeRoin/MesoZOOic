using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public class LineGate
{
    public int lineIndex;
    public TutorialAction waitFor = TutorialAction.None;
    public GameObject highlight;
    public bool makeDirtyOnNext;
    public float setHungerOnShow = 0f;
}

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Lines")]
    [TextArea(2, 5)]
    [SerializeField] private string[] lines;
    [SerializeField] private float secondsPerLetter = 0.03f;

    [Header("Gates")]
    [SerializeField] private LineGate[] gates;
    [SerializeField] private float advanceDelayAfterAction = 0.8f;

    [Header("Walter Bob")]
    [SerializeField] private RectTransform characterImage;
    [SerializeField] private float bobHeight = 20f;
    [SerializeField] private float bobDuration = 0.3f;

    [Header("Animalcrossing Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip talkClip;
    [SerializeField] private float talkVolume = 1f;

    [Header("Hide")]
    [SerializeField] private GameObject[] hideWhenDone;

    [Header("Play Once")]
    [SerializeField] private string tutorialId = "";

    [Header("Load Sccene")]
    [SerializeField] private bool completesTutorial = true;
    [SerializeField] private string nextSceneName;

    private int index;
    private bool isTyping;
    private Coroutine typingRoutine;
    private Coroutine bobRoutine;
    private Coroutine advanceRoutine;
    private Vector2 characterRestPosition;
    private bool talkPaused;

    private CanvasGroup panelGroup;
    private LineGate currentGate;
    private bool highlightIsIndicator;
    private Coroutine pulseRoutine;
    private Transform pulseTarget;
    private Vector3 pulseBaseScale;
    private bool waitingForAction;

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

        if (dialoguePanel != null)
        {
            panelGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (panelGroup == null)
                panelGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }
    }


    private void OnEnable()
    {
        TutorialEvents.Raised += OnTutorialAction;
    }

    private void OnDisable()
    {
        TutorialEvents.Raised -= OnTutorialAction;
    }

    private void Start()
    {
        if (SaveSystem.IsTutorialDone(tutorialId))
        {
            HideTutorialObjects();
            enabled = false;
            return;
        }

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

        // Player still has something to do on this line.
        if (waitingForAction) return;

        if (currentGate != null && currentGate.makeDirtyOnNext)
            MakeDinoDirty();

        Advance();
    }

    private void Advance()
    {
        HideHighlight();
        index++;

        if (index < lines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    private void ShowLine()
    {
        EnterGate();

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
        StartBob();
    }
    private LineGate FindGate(int lineIndex)
    {
        if (gates == null) return null;

        foreach (LineGate g in gates)
            if (g != null && g.lineIndex == lineIndex)
                return g;

        return null;
    }

    private void EnterGate()
    {
        currentGate = FindGate(index);
        waitingForAction = currentGate != null && currentGate.waitFor != TutorialAction.None;

        if (currentGate != null)
        {
            ShowHighlight(currentGate.highlight);

            if (currentGate.setHungerOnShow > 0f)
            {
                DinoStats dino = FindDino();
                if (dino != null) dino.SetHunger(currentGate.setHungerOnShow);
            }
        }
        SetPanelBlocking(!waitingForAction);
    }

    private void ShowHighlight(GameObject target)
    {
        if (target == null) return;

        if (!target.activeSelf)
        {
            highlightIsIndicator = true;
            target.SetActive(true);
            return;
        }

        highlightIsIndicator = false;
        pulseTarget = target.transform;
        pulseBaseScale = pulseTarget.localScale;
        pulseRoutine = StartCoroutine(Pulse());
    }

    private void HideHighlight()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (pulseTarget != null)
        {
            pulseTarget.localScale = pulseBaseScale;
            pulseTarget = null;
        }

        if (highlightIsIndicator && currentGate != null && currentGate.highlight != null)
            currentGate.highlight.SetActive(false);

        highlightIsIndicator = false;
    }

    private IEnumerator Pulse()
    {
        float time = 0f;

        while (pulseTarget != null)
        {
            time += Time.unscaledDeltaTime;
            float k = 1f + Mathf.Sin(time * 6f) * 0.08f;
            pulseTarget.localScale = pulseBaseScale * k;
            yield return null;
        }
    }

    private void SetPanelBlocking(bool blocking)
    {
        if (panelGroup == null) return;

        panelGroup.blocksRaycasts = blocking;
        panelGroup.interactable = blocking;
    }


    private void OnTutorialAction(TutorialAction action)
    {
        if (!waitingForAction || currentGate == null) return;
        if (action != currentGate.waitFor) return;

        waitingForAction = false;
        HideHighlight();

        if (advanceRoutine != null) StopCoroutine(advanceRoutine);
        advanceRoutine = StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(advanceDelayAfterAction);

        if (isTyping) FinishTyping();

        if (currentGate != null && currentGate.makeDirtyOnNext)
            MakeDinoDirty();

        Advance();
    }

    private static DinoStats FindDino()
    {
        foreach (DinoStats d in DinoStats.All)
            if (d != null) return d;

        return null;
    }

    private void MakeDinoDirty()
    {
        DinoStats dino = FindDino();
        if (dino != null) dino.ForceDirty();
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


    private void HideTutorialObjects()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (characterImage != null) characterImage.gameObject.SetActive(false);

        if (hideWhenDone != null)
            foreach (GameObject go in hideWhenDone)
                if (go != null) go.SetActive(false);
    }
    private void EndDialogue()
    {
        if (audioSource != null)
            audioSource.Stop();
        talkPaused = false;

        SetPanelBlocking(true);
        HideTutorialObjects();

        SaveSystem.MarkTutorialDone(tutorialId);

        if (completesTutorial)
            SaveSystem.TutorialComplete = true;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}