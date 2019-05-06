using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    - Addon: Chat Balloon for NPC
    - Dev: GallighanMaker (Leandro Vieira)
    - Email: leandrovieira92@gmail.com
*/

public partial class Entity
{
    [Header("Chat Balloon Settings")]
    public bool activeChatBalllon = true;
    public GameObject chatBubblePrefab;
    public float boundsY = 1.0f;
    public float fadeTime = 0.5f;

    [Header("Automatic Message System")]
    public bool activeAutoMessages = true;
    public float messageMinTime = 10;
    public float messageMaxTime = 30;
    public string[] messages;

    // Private used to start fade coroutine
    private YieldInstruction fadeInstruction = new YieldInstruction();

    void OnStartServer_NPCMessages()
    {
        // Return if chat balloon are disabled
        if (!activeAutoMessages)
            return;

        // Start coroutine with normal messages
        StartCoroutine(ShowMessage(messages));
    }

    [ClientRpc(channel = Channels.DefaultUnreliable)]
    public void RpcShowChatBubble(string message)
    {
        // Return if chat balloon are disabled
        if (!activeChatBalllon)
            return;

        // Check if chat balloon prefab is not null
        if (chatBubblePrefab != null)
        {
            // Find other chat balloons and delete them, because with this function does not allow spawning one chat on top of another, causing a visual bug.

            // Get all chat balloon prefabs
            GameObject[] prevChatBubbles = GameObject.FindGameObjectsWithTag("ChatBubble");

            if (prevChatBubbles.Length > 0)
            {
                // Loop in all objects
                foreach (GameObject obj in prevChatBubbles)
                {
                    // Check if chat balloon name has a delimiter
                    if (obj.name.Contains("_"))
                    {
                        // Get match name
                        string matchName = obj.name.Split('_')[1];

                        // Delete prev message balloon prefab
                        if (matchName == GetComponent<Entity>().netId.ToString())
                            Destroy(obj);
                    }
                }
            }

            // You can modify bounds to fit in your player prefab, in this showcase I used "bounds.max.y + 1f"
            Bounds bounds = collider.bounds;
            Vector2 position = new Vector3(bounds.center.x, bounds.max.y + boundsY, bounds.center.z);

            // Instantiate new chat balloon with player name prefix (we need this for delete correctly game object).
            GameObject popup = Instantiate(chatBubblePrefab, position, Quaternion.identity, gameObject.transform);

            // Set chat balloon with net id
            popup.name = chatBubblePrefab.name + "_" + GetComponent<Entity>().netId;

            // Fix small messages text
            if (message.Length <= 4)
                message = "  "  + message + "  ";

            // Set chat balloon text
            popup.transform.Find("Parent").Find("Image").Find("Content").GetComponent<Text>().text = message;

            // Start new balloon prefab image with alpha 0 (transparent)
            Image balloonImage = popup.transform.Find("Parent").Find("Image").GetComponent<Image>();
            Color colorImage = balloonImage.color;
            colorImage.a = 0;
            balloonImage.color = colorImage;

            // get arrow image
            Image arrowImage = popup.transform.Find("Parent").Find("Image").Find("Arrow").GetComponent<Image>();

            // Start new balloon text with alpha 0 (transparent)
            Text balloonText = popup.transform.Find("Parent").Find("Image").Find("Content").GetComponent<Text>();
            Color colorText = balloonText.color;
            colorText.a = 0;
            balloonText.color = colorText;

            // Start fade coroutine
            StartCoroutine(FadeIn(balloonImage, arrowImage, balloonText));
        }
    }

    IEnumerator FadeIn(Image balloonImage, Image arrowImage, Text balloonText)
    {
        float elapsedTime = 0.0f;

        Color colorBalloonImage = balloonImage.color;
        Color colorArrowImage = arrowImage.color;
        Color colorText = balloonText.color;

        while (elapsedTime < fadeTime)
        {
            yield return fadeInstruction;
            elapsedTime += Time.deltaTime;

            colorBalloonImage.a = Mathf.Clamp01(elapsedTime / fadeTime);
            colorArrowImage.a = Mathf.Clamp01(elapsedTime / fadeTime);
            colorText.a = Mathf.Clamp01(elapsedTime / fadeTime);

            if (balloonImage != null &&  arrowImage != null && balloonText != null)
            {
                balloonImage.color = colorBalloonImage;
                arrowImage.color = colorArrowImage;
                balloonText.color = colorText;
            }
        }
    }

    IEnumerator ShowMessage(string[] msgs)
    {
        while (true)
        {
            // Generate a random time
            float time = UnityEngine.Random.Range(messageMinTime, messageMaxTime);

            // Wait for show message
            yield return new WaitForSeconds(time);

            // Create a Random object  
            System.Random rand = new System.Random();

            // Generate a random index less than the size of the array.  
            int index = rand.Next(msgs.Length);

            // Pick the result.  
            string pickMsg = msgs[index];

            // Fix small messages
            if (pickMsg.Length <= 4)
                pickMsg = "  " + pickMsg + "  ";

            // Show chat balloon with random message
            RpcShowChatBubble(pickMsg);
        }
    }
}