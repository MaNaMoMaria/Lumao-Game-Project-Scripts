using UnityEngine;
using TMPro;

// This script is for the gas station! 😅
// Player can enter, refuel, and see messages.



namespace Lumao.Core
{
    public class GasStation : MonoBehaviour
    {
        [Header("Gas Station Settings")] // Gas station stuff ⛽
        public float refuelAmount = 50f;
        public int refuelCost = 20;

        [Header("UI Settings")] // UI stuff 🖥️
        public TextMeshProUGUI interactionPromptText;
        public TextMeshProUGUI transactionMessageText;

        [Header("References")] // Other stuff
        private PlayerCarPassengerSystem playerSystem;

        void Start ()
        {
            // Make sure collider is trigger
            Collider col = GetComponent<Collider>();
            if (col == null || !col.isTrigger)
            {
                Debug.LogError("GasStation: Needs a Collider with Is Trigger = true.");
                enabled = false;
                return;
            }
            // Make sure Rigidbody is kinematic
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            // Check UI fields
            if (interactionPromptText == null || transactionMessageText == null)
            {
                Debug.LogError("GasStation: Set all UI fields in Inspector plz.");
                enabled = false;
                return;
            }

            interactionPromptText.gameObject.SetActive(false);
            transactionMessageText.gameObject.SetActive(false);
        }

        void Update ()
        {
            // If player is in range and presses F, try to refuel
            if (playerSystem != null && Input.GetKeyDown(KeyCode.F))
            {
                TryRefuel();
            }
        }

        // When player enters gas station area
        void OnTriggerEnter (Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerSystem = other.GetComponent<PlayerCarPassengerSystem>();
                if (playerSystem != null)
                {
                    interactionPromptText.text = "Press F";
                    interactionPromptText.gameObject.SetActive(true);
                    Debug.Log("Player entered gas station range.");
                }
            }
        }

        // When player leaves gas station area
        void OnTriggerExit (Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerSystem = null;
                interactionPromptText.gameObject.SetActive(false);
                transactionMessageText.gameObject.SetActive(false);
                Debug.Log("Player exited gas station range.");
            }
        }

        // Try to refuel the car and show message
        void TryRefuel ()
        {
            if (playerSystem == null) return;

            string message = playerSystem.Refuel(refuelAmount, refuelCost);
            Color messageColor = Color.white;

            if (message.Contains("Refueled!"))
            {
                messageColor = Color.green;
            }
            else if (message.Contains("Not enough money!"))
            {
                messageColor = Color.red;
            }
            else if (message.Contains("Tank is Full!"))
            {
                messageColor = Color.yellow;
            }

            ShowTransactionMessage(message, messageColor);
        }

        // Show message for refuel result
        void ShowTransactionMessage (string message, Color color)
        {
            if (transactionMessageText != null)
            {
                transactionMessageText.text = message;
                transactionMessageText.color = color;
                transactionMessageText.gameObject.SetActive(true);
                CancelInvoke("HideTransactionMessage");
                Invoke("HideTransactionMessage", 3f); // Hide after 3 seconds
            }
        }

        // Hide the transaction message (called by Invoke)
        void HideTransactionMessage ()
        {
            if (transactionMessageText != null)
            {
                transactionMessageText.gameObject.SetActive(false);
            }
        }
    }
}
