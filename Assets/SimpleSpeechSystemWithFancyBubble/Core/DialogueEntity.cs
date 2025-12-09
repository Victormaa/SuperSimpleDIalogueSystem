using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEntity : MonoBehaviour
{
    public enum Diagloguer { Bubble_1, Bubble_2 }

    public string DialogueName;

    public FancySpeechBubble bubble_1;
    public FancySpeechBubble bubble_2;

    public List<(Diagloguer, string)> dialogueData = new List<(Diagloguer, string)>();

    private int currentIndex = 0;
    private int dialogueLength;
    void Start()
    {
        dialogueData.Clear();
        foreach (var pair in inspectorDialogue)
        {
            dialogueData.Add((pair.dialoguer, pair.text));
        }
        dialogueLength = dialogueData.Count;

        bubble_1.gameObject.SetActive(false);
        bubble_2.gameObject.SetActive(false);

    }
    public IEnumerator AdvanceDialogue()
    {
        if (currentIndex >= dialogueLength)
        {
            if (bubble_1.gameObject.activeSelf)
            {
                bubble_1.Set("");
                yield return StartCoroutine(HideBubble(bubble_1.gameObject));
            }
            if (bubble_2.gameObject.activeSelf)
            {
                bubble_2.Set("");
                yield return StartCoroutine(HideBubble(bubble_2.gameObject));
            }
                
            //yield return StartCoroutine(HideBothBubble(bubble_1.gameObject, bubble_2.gameObject));
            currentIndex = 0;
            yield break;
        }

        // check if pre typing is finish if no return
        if (currentIndex > 0)
        {
            Diagloguer preSpeaker = dialogueData[currentIndex - 1].Item1;
            string preText = dialogueData[currentIndex - 1].Item2;

            FancySpeechBubble preSpeechBubble = preSpeaker == Diagloguer.Bubble_1 ? bubble_1 : bubble_2;
            if (preSpeechBubble._isTyping)
            {
                FinishBubble(preSpeechBubble, preText);
                yield break;
            }
        }

        Diagloguer curSpeaker = dialogueData[currentIndex].Item1;
        string text = dialogueData[currentIndex].Item2;
        if (curSpeaker == Diagloguer.Bubble_1)
        {
            if (bubble_2.gameObject.activeSelf)
                yield return StartCoroutine(HideBubble(bubble_2.gameObject));

            if (!bubble_1.gameObject.activeSelf)
                yield return StartCoroutine(ShowBubble(bubble_1.gameObject));
            bubble_1.Set(text);
        }
        else if (curSpeaker == Diagloguer.Bubble_2)
        {
            if (bubble_1.gameObject.activeSelf)
                yield return StartCoroutine(HideBubble(bubble_1.gameObject));

            if (!bubble_2.gameObject.activeSelf)
                yield return StartCoroutine(ShowBubble(bubble_2.gameObject));
            bubble_2.Set(text);
        }
        else
        {
            Debug.LogError($"the:{currentIndex} setence has something wrong.");
        }

        currentIndex++;
    }
    [System.Serializable]
    public class DialoguePair
    {
        public Diagloguer dialoguer;
        [TextArea(1, 3)]
        public string text;
    }
    public List<DialoguePair> inspectorDialogue = new List<DialoguePair>();
    IEnumerator ShowBubble(GameObject bubble)
    {
        // Ensure the bubble is active but initially scaled to zero
        bubble.SetActive(true);
        bubble.transform.localScale = Vector3.zero;

        // Animation parameters
        float duration = 0.3f; // Duration of the animation in seconds
        float elapsedTime = 0f;

        // Animate the scale
        while (elapsedTime < duration)
        {
            // Calculate the current scale using a smooth step (easing function)
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation
            bubble.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            // Increment time and wait for next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final scale is exactly (1,1,1)
        bubble.transform.localScale = Vector3.one;
    }
    IEnumerator HideBubble(GameObject bubble)
    {
        // Animation parameters
        float duration = 0.3f; // Duration of the animation in seconds
        float elapsedTime = 0f;

        // Store initial scale
        Vector3 initialScale = bubble.transform.localScale;

        // Animate the scale
        while (elapsedTime < duration)
        {
            // Calculate the current scale using a smooth step (easing function)
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation
            bubble.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            // Increment time and wait for next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final scale is exactly (0,0,0) and deactivate
        bubble.transform.localScale = Vector3.zero;
        bubble.SetActive(false);
    }
    void FinishBubble(FancySpeechBubble bubble, string curText)
    {
        bubble.transform.localScale = Vector3.one;
        bubble.SetTextInstantly(curText);
    }
    IEnumerator HideBothBubble(GameObject bubbleA, GameObject bubbleB)
    {
        // Animation parameters
        float duration = 0.3f; // Duration of the animation in seconds
        float elapsedTime = 0f;

        // Store initial scale
        Vector3 initialScale1 = bubbleA.transform.localScale;
        Vector3 initialScale2 = bubbleB.transform.localScale;

        // Animate the scale
        while (elapsedTime < duration)
        {
            // Calculate the current scale using a smooth step (easing function)
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation
            bubbleA.transform.localScale = Vector3.Lerp(initialScale1, Vector3.zero, t);
            bubbleB.transform.localScale = Vector3.Lerp(initialScale2, Vector3.zero, t);

            // Increment time and wait for next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final scale is exactly (0,0,0) and deactivate
        bubbleA.transform.localScale = Vector3.zero;
        bubbleA.SetActive(false);
        bubbleB.transform.localScale = Vector3.zero;
        bubbleB.SetActive(false);
    }
    public void ProcessDialogue()
    {
        StartCoroutine(AdvanceDialogue());
    }
}