using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Rex's Lines")]
    [TextArea(2, 5)]
    [SerializeField] private string[] lines;
    [SerializeField] private float secondsPerLetter = 0.03f;

    private int index;
    private bool isTyping;
    private Coroutine typingRoutine;

    void Start()
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
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
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
    }

    private void FinishTyping()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}