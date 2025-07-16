using UnityEngine;
using TMPro; // For TextMeshPro
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Choice UI")]
    [SerializeField] private GameObject choicePanel; // A panel to hold choice buttons
    [SerializeField] private Button choiceButtonPrefab; // Prefab for individual choice buttons

    [SerializeField] private float typingSpeed = 0.05f;

    private Queue<string> currentDialogueLines;
    private Queue<string> currentSpeakerNames;
    private DialogueInteractionDefinition currentDialogueDefinition; // Store the active definition

    private bool isTyping = false;
    private bool dialogueActive = false;

    // Callbacks for when choices are made or dialogue ends
    public delegate void DialogueChoiceMade(int choiceIndex);
    public static event DialogueChoiceMade onDialogueChoiceMade;

    public delegate void OnDialogueStarted();
    public static event OnDialogueStarted onDialogueStarted;

    public delegate void OnDialogueEnded();
    public static event OnDialogueEnded onDialogueEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        currentDialogueLines = new Queue<string>();
        currentSpeakerNames = new Queue<string>();
        continueButton.gameObject.SetActive(false);
    }


    public void StartDialogue(List<string> dialogue, List<string> speakers = null, DialogueInteractionDefinition definition = null)
    {
        dialogueActive = true;
        onDialogueStarted?.Invoke();

        currentDialogueDefinition = definition;

        dialoguePanel.SetActive(true);
        choicePanel.SetActive(false); // Ensure choices are hidden when a new dialogue starts
        ClearChoiceButtons(); // Clear any old choice buttons

        currentDialogueLines.Clear();
        currentSpeakerNames.Clear();

        foreach (string line in dialogue)
        {
            currentDialogueLines.Enqueue(line);
        }

        if (speakers != null && speakers.Count > 0 && speakers.Count == dialogue.Count)
        {
            foreach (string speaker in speakers)
            {
                currentSpeakerNames.Enqueue(speaker);
            }
        }
        else
        {
            currentSpeakerNames.Clear();
        }

        continueButton.gameObject.SetActive(false); // Hide continue button at the start of typing
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            isTyping = false;
            // Decide whether to show continue button or choices after typing
            CheckForChoicesOrContinue();
            return;
        }

        if (currentDialogueLines.Count == 0)
        {
            // If no more lines, check for choices or end dialogue
            CheckForChoicesOrEndDialogue();
            return;
        }

        string sentence = currentDialogueLines.Dequeue();
        string speaker = "";

        if (currentSpeakerNames.Count > 0)
        {
            speaker = currentSpeakerNames.Dequeue();
        }

        speakerNameText.text = speaker;

        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        continueButton.gameObject.SetActive(false);
        StartCoroutine(TypeSentence(sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i < sentence.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        CheckForChoicesOrContinue(); // Check after typing
    }

    private void CheckForChoicesOrContinue()
    {
        if (currentDialogueDefinition != null && currentDialogueDefinition.offerChoices && currentDialogueLines.Count == 0)
        {
            // If this is the last line and choices are offered
            ShowChoices(currentDialogueDefinition.choiceTexts);
            // Ensure continue button is hidden when choices are active
            continueButton.gameObject.SetActive(false);
        }
        else
        {
            // Otherwise, show continue button
            continueButton.gameObject.SetActive(true);
            // Ensure choice panel is hidden and buttons cleared if no choices are active
            choicePanel.SetActive(false);
            ClearChoiceButtons();
        }
    }

    private void CheckForChoicesOrEndDialogue()
    {
        if (currentDialogueDefinition != null && currentDialogueDefinition.offerChoices)
        {
            ShowChoices(currentDialogueDefinition.choiceTexts);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowChoices(List<string> choices)
    {
        continueButton.gameObject.SetActive(false); // Hide continue button
        choicePanel.SetActive(true); // Show choice panel

        ClearChoiceButtons();

        for (int i = 0; i < choices.Count; i++)
        {
            Button newButton = Instantiate(choiceButtonPrefab, choicePanel.transform);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = choices[i];
            int choiceIndex = i; // Capture loop variable for closure
            newButton.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"Choice {choiceIndex} selected: {currentDialogueDefinition.choiceTexts[choiceIndex]}");

        // Immediately hide and clear the choice UI when a choice is made
        choicePanel.SetActive(false);
        ClearChoiceButtons();

        // Notify listeners that a choice was made
        onDialogueChoiceMade?.Invoke(choiceIndex);
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false); // Ensure choice panel is hidden
        ClearChoiceButtons();
        dialogueActive = false;
        continueButton.gameObject.SetActive(false);
        currentDialogueDefinition = null; // Clear active definition
        onDialogueEnded?.Invoke();
        Debug.Log("Dialogue Ended.");
    }

    public void OnContinueButtonClicked()
    {
        if (dialogueActive)
        {
            DisplayNextSentence();
        }
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}