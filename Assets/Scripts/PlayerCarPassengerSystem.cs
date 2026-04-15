// PlayerCarPassengerSystem.cs
// This script is for the player car passenger system! 😅
// It handles passengers, fuel, money, and collecting keys for the city gate.

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Lumao.Core
{
    public class PlayerCarPassengerSystem : MonoBehaviour
    {
        [Header("Passenger Stuff")] // Passenger settings 🚗
        public List<GameObject> passengerPrefabs;
        public Transform passengerSpawnParent;
        public int maxPassengersCapacity = 4; // How many passengers can fit
        public float passengerDropOffTime = 20f; // How long before passenger leaves
        public float newPassengerSpawnDelay = 5f; // Delay before new passenger spawns
        public int paymentPerPassenger = 10; // Money for each passenger

        [Header("Spawn Points")] // Where to spawn stuff
        [Tooltip("Spawn points for passengers")]
        public List<Transform> pickUpSpawnPoints; // Only for passengers
        [Tooltip("Spawn points for keys (need at least 2!)")]
        public List<Transform> keySpawnPoints; // Only for keys
        public float spawnSpreadRadius = 2f; // How far from spawn point

        [Header("Money and Sound Stuff")] // Money and sounds 💰🔊
        public int playerMoney = 0;
        public AudioClip coinSound;
        public AudioClip alarmSound;
        public AudioClip lowFuelWarningSound;
        public AudioClip keyCollectedSound;
        public AudioClip gameOverSound;

        [Header("Fuel Stuff")] // Fuel settings ⛽
        public float maxFuel = 100f;
        public float currentFuel;
        public float fuelConsumptionRate = 1f;
        public float consumptionMultiplierAtSpeed = 0.1f;
        public float lowFuelThreshold = 15f;
        public float fuelEpsilon = 0.01f;
        private Rigidbody playerRigidbody;
        private bool lowFuelAlarmActive = false;
        private bool isOutOfFuel = false;

        [Header("Key Stuff")] // Key settings 🔑
        [Tooltip("Did player collect the key for the city gate?")]
        public bool hasKeyForDoor = false;
        [Tooltip("Key prefab")]
        public GameObject keyPrefab;
        [Tooltip("How often key moves before collected")]
        public float keyMovementInterval = 10f;

        private GameObject currentKeyInstance; // Only one key in scene
        private Transform lastKeySpawnPoint; // Don't spawn key at same spot
        private Coroutine keyMovementCoroutine; // For moving key

        [Header("UI Stuff")] // UI settings 🖥️
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI capacityText;
        public TextMeshProUGUI capacityAlarmText;
        public Slider fuelSlider;
        public RawImage lowFuelWarningImage;
        public TextMeshProUGUI fuelText;
        [Tooltip("Image for key icon in UI")]
        public RawImage keyDisplayImage;

        [Header("UI Warning Stuff")]
        public float alarmDisplayTime = 2f;

        [Header("Money Text Motion Stuff")]
        public float moneyTextPulseScale = 2f; // How big money text gets
        public float moneyTextPulseDuration = 0.2f; // How fast pulse is            

        [Header("Game Over UI")]
        public GameObject gameOverPanel;
        public Button restartButton;
        public Button exitButton;

        [Header("City Gate Reference")] // City gate script
        [Tooltip("CityGate script for the city gate")]
        public CityGate cityGateScript;

        private Rigidbody rb;

        private int currentPassengersCount = 0; // How many passengers now
        private List<float> seatTimers; // Timers for each seat

        void Awake ()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("PlayerCarPassengerSystem: No Rigidbody on player car! Add one plz.");
                enabled = false;
                return;
            }

            seatTimers = new List<float>();
            for (int i = 0; i < maxPassengersCapacity; i++)
            {
                seatTimers.Add(-1f);
            }
        }

        void Start ()
        {
            Debug.Log("PlayerCarPassengerSystem: Script started!");

            // Check all fields in Inspector
            if (passengerPrefabs == null || passengerPrefabs.Count == 0 || pickUpSpawnPoints == null || pickUpSpawnPoints.Count == 0 ||
                moneyText == null || capacityText == null || capacityAlarmText == null ||
                fuelSlider == null || lowFuelWarningImage == null || fuelText == null ||
                gameOverPanel == null || restartButton == null || exitButton == null ||
                gameOverSound == null || keyPrefab == null || keySpawnPoints == null || keyDisplayImage == null ||
                cityGateScript == null)
            {
                Debug.LogError("PlayerCarPassengerSystem: Set all fields in Inspector plz.");
                enabled = false;
                return;
            }
            if (keySpawnPoints.Count < 2)
            {
                Debug.LogError("PlayerCarPassengerSystem: Need at least 2 key spawn points for random movement!");
                enabled = false;
                return;
            }
            Debug.Log("PlayerCarPassengerSystem: Key spawn points: " + keySpawnPoints.Count);
            Debug.Log("PlayerCarPassengerSystem: All fields set. Good to go!");

            playerMoney = 0;
            hasKeyForDoor = false;
            currentFuel = maxFuel;

            capacityAlarmText.gameObject.SetActive(false);
            lowFuelWarningImage.gameObject.SetActive(false);
            gameOverPanel.SetActive(false);

            fuelSlider.maxValue = maxFuel;
            fuelSlider.minValue = 0f;

            Debug.Log("PlayerCarPassengerSystem: Passenger and Fuel system ready. Car capacity: " + maxPassengersCapacity);
            UpdatePassengerUI();
            UpdateFuelUI();

            SpawnInitialPassengers();

            // CityGate script sets gate closed at start, no need to do it here

            keyDisplayImage.gameObject.SetActive(false);

            SpawnSingleKeyAtRandomPoint();
            keyMovementCoroutine = StartCoroutine(MoveKeyRandomly());
        }

        void Update ()
        {
            if (isOutOfFuel) return;

            // Update seat timers and drop off passengers
            for (int i = 0; i < seatTimers.Count; i++)
            {
                if (seatTimers [i] > 0)
                {
                    seatTimers [i] -= Time.deltaTime;
                    if (seatTimers [i] <= 0)
                    {
                        DropOffPassenger(i);
                    }
                }
            }

            // Fuel consumption
            if (currentFuel > fuelEpsilon)
            {
                float speed = rb.linearVelocity.magnitude;
                float consumption = 0f;

                if (speed > 0.1f)
                {
                    consumption = fuelConsumptionRate * Time.deltaTime;
                    consumption += speed * consumptionMultiplierAtSpeed * Time.deltaTime;
                }

                currentFuel -= consumption;
                currentFuel = Mathf.Max(0, currentFuel);
                UpdateFuelUI();

                if (currentFuel <= lowFuelThreshold && !lowFuelAlarmActive)
                {
                    ActivateLowFuelAlarm();
                }
                else if (currentFuel > lowFuelThreshold && lowFuelAlarmActive)
                {
                    DeactivateLowFuelAlarm();
                }
            }
            else
            {
                currentFuel = 0;
                UpdateFuelUI();
                HandleOutOfFuel();
            }
        }

        // When car hits a passenger
        void OnTriggerEnter (Collider other)
        {
            if (other.CompareTag("Passenger"))
            {
                PassengerOnGround passenger = other.GetComponent<PassengerOnGround>();
                if (passenger != null && !passenger.isPickedUp)
                {
                    if (currentPassengersCount < maxPassengersCapacity)
                    {
                        TryPickUpPassenger(passenger);
                    }
                    else
                    {
                        Debug.LogWarning("Car is full! Can't pick up more passengers.");
                        PlayAlarmSound();
                    }
                }
            }
        }

        // Spawn passengers at start
        void SpawnInitialPassengers ()
        {
            foreach (Transform spawnPoint in pickUpSpawnPoints)
            {
                int numToSpawn = Random.Range(1, 3);
                for (int i = 0; i < numToSpawn; i++)
                {
                    SpawnSinglePassenger(spawnPoint);
                }
            }
            Debug.Log("PlayerCarPassengerSystem: Initial passengers spawned.");
        }

        // Spawn one passenger at a spawn point
        void SpawnSinglePassenger (Transform spawnPoint)
        {
            if (passengerPrefabs.Count == 0 || spawnPoint == null)
            {
                Debug.LogWarning("SpawnSinglePassenger: Passenger Prefab or spawn point missing.");
                return;
            }

            GameObject randomPassengerPrefab = passengerPrefabs [Random.Range(0, passengerPrefabs.Count)];

            Vector2 randomCirclePoint = Random.insideUnitCircle * spawnSpreadRadius;
            Vector3 spawnPosition = spawnPoint.position + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);
            spawnPosition.y = spawnPoint.position.y;

            GameObject passengerObj = Instantiate(randomPassengerPrefab, spawnPosition, Quaternion.identity, passengerSpawnParent);
            passengerObj.tag = "Passenger";

            Collider passengerCol = passengerObj.GetComponent<Collider>();
            if (passengerCol != null) passengerCol.isTrigger = true;
            Rigidbody passengerRb = passengerObj.GetComponent<Rigidbody>();
            if (passengerRb == null) passengerRb = passengerObj.AddComponent<Rigidbody>();
            passengerRb.isKinematic = true;

            if (passengerObj.GetComponent<PassengerOnGround>() == null)
            {
                passengerObj.AddComponent<PassengerOnGround>();
            }
        }

        // Try to pick up a passenger
        void TryPickUpPassenger (PassengerOnGround passenger)
        {
            int seatIndex = -1;
            for (int i = 0; i < seatTimers.Count; i++)
            {
                if (seatTimers [i] <= 0)
                {
                    seatIndex = i;
                    break;
                }
            }

            if (seatIndex != -1)
            {
                passenger.PickUpPassenger();
                currentPassengersCount++;
                seatTimers [seatIndex] = passengerDropOffTime;

                playerMoney += paymentPerPassenger;
                PlayCoinSound();

                Debug.Log("Passenger picked up! Money: " + paymentPerPassenger + ". Total: " + playerMoney + ". Empty seats: " + (maxPassengersCapacity - currentPassengersCount));
                UpdatePassengerUI();

                StartCoroutine(MoneyTextPulse());
            }
        }

        // Drop off a passenger
        void DropOffPassenger (int seatIndex)
        {
            currentPassengersCount--;
            seatTimers [seatIndex] = -1f;
            Debug.Log("Passenger dropped off! Seats left: " + (maxPassengersCapacity - currentPassengersCount));
            UpdatePassengerUI();

            Invoke("SpawnSinglePassengerAtRandomPoint", newPassengerSpawnDelay);
        }

        // Spawn a passenger at a random point
        void SpawnSinglePassengerAtRandomPoint ()
        {
            if (pickUpSpawnPoints.Count == 0) return;
            Transform randomSpawnPoint = pickUpSpawnPoints [Random.Range(0, pickUpSpawnPoints.Count)];
            SpawnSinglePassenger(randomSpawnPoint);
        }

        // Update money and capacity UI
        public void UpdatePassengerUI ()
        {
            if (moneyText != null)
            {
                moneyText.text = "Money: " + playerMoney.ToString();
            }
            if (capacityText != null)
            {
                capacityText.text = "Capacity: " + currentPassengersCount + "/" + maxPassengersCapacity;
            }
        }

        // Update fuel UI
        void UpdateFuelUI ()
        {
            if (fuelSlider != null)
            {
                fuelSlider.value = currentFuel;
            }
            if (fuelText != null)
            {
                fuelText.text = currentFuel.ToString("F0") + "/" + maxFuel.ToString("F0");
            }
        }

        // Play coin sound
        void PlayCoinSound ()
        {
            if (AudioListener.volume > 0f && coinSound != null)
            {
                AudioSource.PlayClipAtPoint(coinSound, transform.position);
            }
        }

        // Play alarm sound and show capacity full message
        void PlayAlarmSound ()
        {
            if (AudioListener.volume > 0f && alarmSound != null)
            {
                AudioSource.PlayClipAtPoint(alarmSound, transform.position);
            }

            if (capacityAlarmText != null)
            {
                capacityAlarmText.text = "Capacity is full !";
                capacityAlarmText.gameObject.SetActive(true);
                CancelInvoke("HideCapacityAlarmText");
                Invoke("HideCapacityAlarmText", alarmDisplayTime);
            }
        }

        // Hide capacity full message
        void HideCapacityAlarmText ()
        {
            if (capacityAlarmText != null)
            {
                capacityAlarmText.gameObject.SetActive(false);
            }
        }

        // Show low fuel warning
        void ActivateLowFuelAlarm ()
        {
            lowFuelAlarmActive = true;
            if (lowFuelWarningImage != null)
            {
                lowFuelWarningImage.gameObject.SetActive(true);
            }
            if (AudioListener.volume > 0f && lowFuelWarningSound != null)
            {
                AudioSource.PlayClipAtPoint(lowFuelWarningSound, transform.position);
            }
        }

        // Hide low fuel warning
        void DeactivateLowFuelAlarm ()
        {
            lowFuelAlarmActive = false;
            if (lowFuelWarningImage != null)
            {
                lowFuelWarningImage.gameObject.SetActive(false);
            }
        }

        // Refuel the car
        public string Refuel (float amount, int cost)
        {
            if (Mathf.Approximately(currentFuel, maxFuel) || currentFuel > maxFuel)
            {
                return "Tank is Full!";
            }

            if (playerMoney < cost)
            {
                return "Not enough money! Need " + cost + " Money.";
            }

            playerMoney -= cost;
            UpdatePassengerUI();

            float fuelToAdd = Mathf.Min(amount, maxFuel - currentFuel);
            currentFuel += fuelToAdd;

            currentFuel = Mathf.Min(currentFuel, maxFuel);
            UpdateFuelUI();

            if (lowFuelAlarmActive && currentFuel > lowFuelThreshold)
            {
                DeactivateLowFuelAlarm();
            }

            return "Refuel! -" + cost + " Money";
        }

        // Handle out of fuel (game over)
        void HandleOutOfFuel ()
        {
            if (isOutOfFuel) return;

            isOutOfFuel = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            DeactivateLowFuelAlarm();

            AudioSource [] allAudioSources = GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in allAudioSources)
            {
                source.Stop();
            }

            if (AudioListener.volume > 0f && gameOverSound != null)
            {
                AudioSource.PlayClipAtPoint(gameOverSound, transform.position);
            }

            this.enabled = false;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            Time.timeScale = 0f;
        }

        // Collect the key for the city gate
        public void CollectKey ()
        {
            hasKeyForDoor = true;
            PlayKeyCollectedSound();

            if (keyDisplayImage != null)
            {
                keyDisplayImage.gameObject.SetActive(true);
            }

            if (cityGateScript != null)
            {
                cityGateScript.OpenGate();
            }

            if (currentKeyInstance != null)
            {
                Destroy(currentKeyInstance);
                currentKeyInstance = null;
            }

            if (keyMovementCoroutine != null)
            {
                StopCoroutine(keyMovementCoroutine);
            }
        }

        // Play key collected sound
        void PlayKeyCollectedSound ()
        {
            if (AudioListener.volume > 0f && keyCollectedSound != null)
            {
                AudioSource.PlayClipAtPoint(keyCollectedSound, transform.position);
            }
        }

        // Reset money after game over
        public void ResetMoneyForGameOver ()
        {
            playerMoney = 0;
            UpdatePassengerUI();
        }

        // Pulse effect for money text (gets big then small)
        IEnumerator MoneyTextPulse ()
        {
            if (moneyText == null) yield break;

            Vector3 originalScale = moneyText.rectTransform.localScale;
            Vector3 targetScale = originalScale * moneyTextPulseScale;

            float timer = 0f;
            while (timer < moneyTextPulseDuration / 2f)
            {
                timer += Time.deltaTime;
                moneyText.rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (moneyTextPulseDuration / 2f));
                yield return null;
            }

            timer = 0f;
            while (timer < moneyTextPulseDuration / 2f)
            {
                timer += Time.deltaTime;
                moneyText.rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (moneyTextPulseDuration / 2f));
                yield return null;
            }

            moneyText.rectTransform.localScale = originalScale;
        }

        // Move key randomly before collected
        IEnumerator MoveKeyRandomly ()
        {
            while (!hasKeyForDoor)
            {
                yield return new WaitForSeconds(keyMovementInterval);

                if (currentKeyInstance != null)
                {
                    Destroy(currentKeyInstance);
                    currentKeyInstance = null;
                }

                SpawnSingleKeyAtRandomPoint();
            }
        }

        // Spawn key at a random point
        void SpawnSingleKeyAtRandomPoint ()
        {
            if (keyPrefab == null) return;
            if (keySpawnPoints.Count < 2)
            {
                if (keySpawnPoints.Count == 0) return;
            }

            if (currentKeyInstance != null && !hasKeyForDoor)
            {
                return;
            }

            Transform randomSpawnPoint = null;
            if (keySpawnPoints.Count > 1)
            {
                int randomIndex = Random.Range(0, keySpawnPoints.Count);
                int attempts = 0;
                while (keySpawnPoints [randomIndex] == lastKeySpawnPoint && attempts < 10 && keySpawnPoints.Count > 1)
                {
                    randomIndex = Random.Range(0, keySpawnPoints.Count);
                    attempts++;
                }
                randomSpawnPoint = keySpawnPoints [randomIndex];
                lastKeySpawnPoint = randomSpawnPoint;
            }
            else if (keySpawnPoints.Count == 1)
            {
                randomSpawnPoint = keySpawnPoints [0];
                lastKeySpawnPoint = randomSpawnPoint;
            }
            else
            {
                return;
            }

            Vector2 randomCirclePoint = Random.insideUnitCircle * spawnSpreadRadius;
            Vector3 spawnPosition = randomSpawnPoint.position + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);
            spawnPosition.y = randomSpawnPoint.position.y + 0.5f;

            currentKeyInstance = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
            currentKeyInstance.name = "Key_Instance_" + randomSpawnPoint.name;

            if (currentKeyInstance.GetComponent<KeyCollectable>() == null)
            {
                currentKeyInstance.AddComponent<KeyCollectable>();
            }
        }
    }
}
