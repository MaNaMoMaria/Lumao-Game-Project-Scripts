using UnityEngine;
using TMPro;

// This script is for the player's car fuel system! 😅
// It handles fuel level, fuel usage, refueling, and UI. 

namespace Lumao.Core
{
    public class PlayerFuelSystem : MonoBehaviour
    {
        [Header("Fuel Settings")] // Fuel stuff ⛽
        public float maxFuel = 100f; // Max fuel tank
        public float currentFuel; // Current fuel
        public float fuelConsumptionRate = 1f; // How fast fuel goes down
        public float consumptionMultiplierAtSpeed = 0.1f; // More speed = more fuel used

        [Header("UI Settings")] // UI stuff 🖥️
        public TextMeshProUGUI fuelText; // Shows fuel amount
        public TextMeshProUGUI outOfFuelText; // Shows "Out of Fuel!"

        [Header("References")] // Other scripts
        public PlayerCarPassengerSystem passengerSystem; // For money stuff
        private Rigidbody playerRigidbody; // For car movement
        public RaceTimerManager raceTimerManager; // For game over stuff

        // Key for saving fuel in PlayerPrefs
        private const string FUEL_SAVE_KEY = "CurrentFuel";

        // Called when script wakes up
        void Awake ()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            if (playerRigidbody == null)
            {
                Debug.LogError("PlayerFuelSystem: No Rigidbody on player car! Add one plz.");
                enabled = false;
                return;
            }
        }

        // Called at start
        void Start ()
        {
            if (fuelText == null || outOfFuelText == null || passengerSystem == null || raceTimerManager == null)
            {
                Debug.LogError("PlayerFuelSystem: Set all UI, Passenger System, and Race Timer Manager fields in Inspector plz.");
                enabled = false;
                return;
            }

            LoadFuel(); // Load fuel from PlayerPrefs
            outOfFuelText.gameObject.SetActive(false); // Hide "Out of Fuel!" at start
            UpdateFuelUI(); // Update fuel UI
        }

        // Called every frame
        void Update ()
        {
            // Use fuel while moving
            if (currentFuel > 0)
            {
                float speed = playerRigidbody.linearVelocity.magnitude;
                float consumption = fuelConsumptionRate * Time.deltaTime;
                if (speed > 0.1f) // If car is moving
                {
                    consumption += speed * consumptionMultiplierAtSpeed * Time.deltaTime;
                }

                currentFuel -= consumption;
                currentFuel = Mathf.Max(0, currentFuel); // Don't go below zero
                UpdateFuelUI();
            }
            else // Out of fuel
            {
                HandleOutOfFuel();
            }
        }

        // Refuel the car (called by GasStation)
        public bool Refuel (float amount, int cost)
        {
            if (passengerSystem == null)
            {
                Debug.LogError("PlayerFuelSystem: Passenger System reference missing for refueling.");
                return false;
            }

            if (passengerSystem.playerMoney >= cost) // Has enough money
            {
                passengerSystem.playerMoney -= cost; // Pay for fuel
                passengerSystem.UpdatePassengerUI(); // Update money UI

                currentFuel += amount; // Add fuel
                currentFuel = Mathf.Min(currentFuel, maxFuel); // Don't go above max
                SaveFuel(); // Save fuel
                UpdateFuelUI(); // Update fuel UI
                Debug.Log("Car refueled! Cost: " + cost + ". Current fuel: " + currentFuel.ToString("F1"));
                return true;
            }
            else
            {
                Debug.Log("Not enough money for fuel! Money: " + passengerSystem.playerMoney + ". Cost: " + cost);
                return false;
            }
        }

        // Handle when fuel runs out
        void HandleOutOfFuel ()
        {
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero; // Stop car
                playerRigidbody.angularVelocity = Vector3.zero; // Stop rotation
            }
            if (outOfFuelText != null)
            {
                outOfFuelText.gameObject.SetActive(true); // Show "Out of Fuel!"
            }
            Debug.LogWarning("Out of fuel! Car stopped.");

            GameOverDueToFuel(); // Call game over stuff
        }

        // Game over when fuel is gone
        void GameOverDueToFuel ()
        {
            if (raceTimerManager == null) return; // If no race manager, do nothing

            // Set player money to zero
            if (passengerSystem != null)
            {
                passengerSystem.playerMoney = 0;
                passengerSystem.UpdatePassengerUI();
                Debug.Log("Player money set to zero because of out of fuel.");
            }




            enabled = false; // Stop this script
        }

        // Update fuel amount in UI
        void UpdateFuelUI ()
        {
            if (fuelText != null)
            {
                fuelText.text = currentFuel.ToString("F1") + " / " + maxFuel.ToString("F0");
            }
        }

        // Save fuel to PlayerPrefs
        void SaveFuel ()
        {
            PlayerPrefs.SetFloat(FUEL_SAVE_KEY, currentFuel);
            PlayerPrefs.Save();
            Debug.Log("Fuel saved: " + currentFuel.ToString("F1"));
        }

        // Load fuel from PlayerPrefs
        void LoadFuel ()
        {
            currentFuel = PlayerPrefs.GetFloat(FUEL_SAVE_KEY, maxFuel);
            Debug.Log("Fuel loaded: " + currentFuel.ToString("F1"));
        }

        // Save fuel when game quits
        void OnApplicationQuit ()
        {
            SaveFuel();
        }
    }
}