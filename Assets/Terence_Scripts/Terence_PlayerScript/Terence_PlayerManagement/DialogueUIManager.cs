using UnityEngine;
using TMPro; // Assuming TextMeshPro
using System.Collections;
using System.Collections.Generic;
using System; // For Action

public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel; // Your dialogue UI panel GameObject
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.05f; // Speed of text typing

    private List<string> currentDialogueLines;
    private List<string> currentSpeakerNames; // Optional
    private int currentLineIndex;
    private Action onDialogueEndCallback; // Callback when dialogue finishes

    private bool isTyping;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        dialoguePanel.SetActive(false); // Start hidden
    }

    public void StartDialogue(List<string> dialogueLines, List<string> speakerNames, Action onEndCallback)
    {
        currentDialogueLines = dialogueLines;
        currentSpeakerNames = speakerNames;
        onDialogueEndCallback = onEndCallback;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    // Called by a "Continue" button or input in your UI
    public void OnContinueButtonClicked()
    {
        if (isTyping)
        {
            // If typing, skip to the end of the current line
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogueLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            if (currentLineIndex < currentDialogueLines.Count)
            {
                DisplayNextLine();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void DisplayNextLine()
    {
        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        string line = currentDialogueLines[currentLineIndex];
        string speaker = (currentSpeakerNames != null && currentSpeakerNames.Count > currentLineIndex)
                         ? currentSpeakerNames[currentLineIndex]
                         : "UNKNOWN"; // Default or leave blank if no speaker names

        speakerNameText.text = speaker;
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        onDialogueEndCallback?.Invoke(); // Call the registered callback
        currentDialogueLines = null;
        currentSpeakerNames = null;
        onDialogueEndCallback = null;
    }

    // You might also want a way to advance dialogue via player input (e.g., spacebar)
    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Space)) // Or your dedicated "Interact" button
        {
            OnContinueButtonClicked();
        }
    }
}
