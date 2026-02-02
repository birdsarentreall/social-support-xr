using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public EncounterDef encounter;

    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        // If no dialogue data or the game is paused and no dialogue is active
        if (encounter.dialogue == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(encounter.dialogue.npcName);
        portraitImage.sprite = encounter.dialogue.npcPortrait;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            // Skip typing animation and show the full line
            StopAllCoroutines();
            dialogueText.SetText(encounter.dialogue.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if(++dialogueIndex < encounter.dialogue.dialogueLines.Length)
        {
            //If another line, type next line
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach(char letter in encounter.dialogue.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            //SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicePitch);
            yield return new WaitForSeconds(encounter.dialogue.typingSpeed);
        }

        isTyping = false;

        if(encounter.dialogue.autoProgressLines.Length > dialogueIndex && encounter.dialogue.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(encounter.dialogue.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);

        var enc = EncounterSequence.Instance.GetNextEncounter();
        if (enc == null) return;

        BattleContext.CurrentEncounter = enc;
        GameStateTransition.Instance.StartBattle();

        // Optional: disable this NPC so it can’t be repeated
        //gameObject.SetActive(false);
    }


}
