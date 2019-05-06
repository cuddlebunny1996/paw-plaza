using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAddonChat : MonoBehaviour
{
    [Header("Trigger Message System")]
    public bool activeTriggerMessages = true;
    public string[] messages;

    void OnTriggerEnter2D(Collider2D co)
    {
        // check if system is enabled
        if (!activeTriggerMessages)
            return;

        // collider with player
        Player player = co.GetComponentInParent<Player>();

        if (player != null)
        {
            // pick a message
            string msg = SelectMessage(messages);

            // server? show trigger message
            if (player.isServer)
                player.RpcShowChatBubble(msg);
        }
    }

    string SelectMessage(string[] msgs)
    {
        // Create a Random object  
        System.Random rand = new System.Random();

        // Generate a random index less than the size of the array.  
        int index = rand.Next(msgs.Length);

        // Pick the result.  
        string pickMsg = msgs[index];

        // Return message
        return pickMsg;
    }
}
