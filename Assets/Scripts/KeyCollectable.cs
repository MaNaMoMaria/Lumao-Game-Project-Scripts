using UnityEngine;

// This script is for key objects! 😅
// When player touches the key, it gets collected and disappears. 


namespace Lumao.Core
{
    public class KeyCollectable : MonoBehaviour
    {
        // Only collect the key once
        private bool isCollected = false;

        // When something enters the trigger
        void OnTriggerEnter (Collider other)
        {
            // If it's the player and key not collected yet
            if (other.CompareTag("Player") && !isCollected)
            {
                isCollected = true; // Mark as collected
                Debug.Log("KeyCollectable: Player touched a key!");

                // Try to call CollectKey on player
                PlayerCarPassengerSystem playerSystem = other.GetComponent<PlayerCarPassengerSystem>();
                if (playerSystem != null)
                {
                    playerSystem.CollectKey();
                    Debug.Log("KeyCollectable: CollectKey called on PlayerCarPassengerSystem.");
                }
                else
                {
                    Debug.LogWarning("KeyCollectable: No PlayerCarPassengerSystem found on player!");
                }

                Destroy(gameObject); // Remove key from scene
                Debug.Log("KeyCollectable: Key GameObject destroyed.");
            }
        }
    }
}