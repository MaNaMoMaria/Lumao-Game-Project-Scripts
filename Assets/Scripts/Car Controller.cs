    using UnityEngine;
    using System.Collections.Generic; 
    using UnityEngine.UI;

// This script moves the car with keyboard! 🚗
// It also does stuff like turning on headlights, brake lights, reverse lights, day/night switch, and boost particles! ✨


namespace Lumao.Core
{
    public class CarController : MonoBehaviour
    {
        [Header("Main Car Movement Settings")] // Main movement settings 🚗
        [Tooltip("Normal forward speed")]
        public float forwardSpeed = 20f;
        [Tooltip("Backward speed")]
        public float backwardSpeed = 10f;
        [Tooltip("Turn speed")]
        public float turnSpeed = 100f;
        [Tooltip("Brake force (for slowing down when braking)")]
        public float brakeForce = 30f;
        [Tooltip("Acceleration rate to reach max speed")]
        public float accelerationRate = 10f;
        [Tooltip("Natural deceleration rate (when no input)")]
        public float decelerationRate = 5f;

        [Header("Boost Settings")] // Boost settings 💨
        [Tooltip("Speed multiplier when boosting")]
        public float boostSpeedMultiplier = 1.5f;
        [Tooltip("Acceleration rate when boosting")]
        public float boostAccelerationRate = 20f;
        [Tooltip("Particle system for boost (smoke/fire)")]
        public ParticleSystem boostParticleSystem;

        [Header("Auto-Flip Settings")] // Anti-flip settings 🤸‍♂️
        [Tooltip("Angle to start auto-right (degrees)")]
        public float autoRightThreshold = 60f;
        [Tooltip("Speed to auto-right car")]
        public float autoRightSpeed = 1f;
        [Tooltip("Min speed for auto-right to work (optional)")]
        public float minSpeedForAutoRight = 1f;

        [Header("Car Lights Settings")] // Car lights settings 💡
        [Tooltip("Headlights components go here")]
        public Light [] headlights;
        [Tooltip("Brake lights components go here")]
        public Light [] brakeLights;
        [Tooltip("UI button for headlights")]
        public Button headlightToggleButton;
        [Tooltip("Keyboard key for headlights (like L)")]
        public KeyCode headlightToggleKey = KeyCode.L;

        [Header("Reverse Lights Settings")] // Reverse lights settings 🔙
        [Tooltip("Reverse lights components go here")]
        public Light [] reverseLights;

        [Header("Day/Night Cycle Settings")] // Day/Night settings 🌞🌚
        [Tooltip("Day skybox material")]
        public Material daySkyboxMaterial;
        [Tooltip("Night skybox material")]
        public Material nightSkyboxMaterial;
        [Tooltip("Tag for all city lights (like 'CityLight')")]
        public string cityLightsTag = "CityLight";
        [Tooltip("UI button for day/night switch")]
        public Button dayNightToggleButton;
        [Tooltip("Keyboard key for day/night switch (like P)")]
        public KeyCode dayNightToggleKey = KeyCode.P;
        [Tooltip("Main directional light (sun)")]
        public Light mainDirectionalLight;

        [Header("Sound Management Settings")] // Sound settings 🔊
        [Tooltip("CarAudioController script for car sounds")]
        public CarAudioController audioController;

        // Private variables for state
        private Rigidbody rb;
        private float currentForwardVelocity; // Tracks current forward/backward speed
        private float verticalInput; // Vertical axis input (W/S)
        private float horizontalInput; // Horizontal axis input (A/D)
        private bool isBrakingInput; // Is brake key (Space) pressed
        private bool isBoostingInput; // Is boost key (Shift) pressed
        private bool areHeadlightsOn = false; // Tracks headlights state
        private bool isCurrentlyDay = true; // Tracks day/night state

        // List for all city lights found in scene
        private List<Light> allCityLights = new List<Light>();

        // For horn sound
        [Tooltip("UI button for horn")] // New line
        public Button HornToggleButton;
        [Tooltip("Keyboard key for horn (like H)")]
        public KeyCode hornKey = KeyCode.H;

        void Start ()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("CarController: Rigidbody not found for car");
                enabled = false;
                return;
            }

            // Headlights setup
            if (headlightToggleButton != null)
            {
                headlightToggleButton.onClick.AddListener(ToggleHeadlights);
            }
            else
            {
                Debug.LogWarning("Headlights UI button is not defined in Inspector");
            }
            SetHeadlightsState(false); // Headlights off at start
            SetBrakeLightsState(false); // Brake lights off at start
            SetReverseLightsState(false); // Reverse lights off at start

            // Day/Night setup
            if (daySkyboxMaterial == null || nightSkyboxMaterial == null)
            {
                Debug.LogError("Day/Night Skybox Materials are not assigned");
                enabled = false;
                return;
            }
            if (dayNightToggleButton != null)
            {
                dayNightToggleButton.onClick.AddListener(ToggleDayNightTheme);
            }
            else
            {
                Debug.LogWarning("Day/Night cycle will not be controllable via UI");
            }
            if (mainDirectionalLight == null)
            {
                Debug.LogWarning("Main Directional Light is not assigned Sun intensity control is disabled");
            }

            // Find all city lights by tag
            GameObject [] lightObjectsWithTag = GameObject.FindGameObjectsWithTag(cityLightsTag);
            foreach (GameObject go in lightObjectsWithTag)
            {
                Light lightComponent = go.GetComponent<Light>();
                if (lightComponent != null)
                {
                    allCityLights.Add(lightComponent);
                }
                else
                {
                    Debug.LogWarning("No Light components found with tag " + cityLightsTag + " City lights control disabled " + go.name);
                }
            }

            // Set scene to day at start
            RenderSettings.skybox = daySkyboxMaterial;
            TurnCityLights(false); // City lights off
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = 1f; // Sun intensity for day
            }
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f, 1f);
            isCurrentlyDay = true;

            // Sound setup
            audioController = GetComponent<CarAudioController>();
            if (audioController == null)
            {
                Debug.LogError("CarAudioController not found on this object Car sounds won't play");
                enabled = false;
                return;
            }

            // Horn button setup
            if (HornToggleButton != null)
            {
                HornToggleButton.onClick.AddListener(() =>
                {
                    if (audioController != null) audioController.PlayHornSound();
                });
            }
            else
            {
                Debug.LogWarning("Horn UI button is not defined in Inspector Horn won't work from UI");
            }

            // Boost particle system setup
            if (boostParticleSystem == null)
            {
                Debug.LogWarning("Boost Particle System is not assigned");
            }
            else
            {
                boostParticleSystem.Stop(); // Stop at first 🚫
            }
        }

        void Update ()
        {
            // Get keyboard input and save to variables
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");
            isBrakingInput = Input.GetKey(KeyCode.Space);
            isBoostingInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Brake lights
            SetBrakeLightsState(isBrakingInput);

            // Reverse lights (when going backward)
            SetReverseLightsState(verticalInput < -0.1f);

            // Headlights toggle with key
            if (Input.GetKeyDown(headlightToggleKey))
            {
                ToggleHeadlights();
            }

            // Day/Night toggle with key
            if (Input.GetKeyDown(dayNightToggleKey))
            {
                ToggleDayNightTheme();
            }

            // Horn sound
            if (Input.GetKeyDown(hornKey))
            {
                if (audioController != null) audioController.PlayHornSound();
            }

            // Boost sound and particles
            bool isBoostActive = isBoostingInput && verticalInput > 0.1f;

            if (isBoostActive)
            {
                if (audioController != null) audioController.StartBoostSound();
                if (boostParticleSystem != null && !boostParticleSystem.isPlaying)
                {
                    boostParticleSystem.Play();
                }
            }
            else
            {
                if (audioController != null) audioController.StopBoostSound();
                if (boostParticleSystem != null && boostParticleSystem.isPlaying)
                {
                    boostParticleSystem.Stop();
                }
            }

            // Acceleration and deceleration
            float currentMaxSpeed = forwardSpeed;
            float currentAccelRate = accelerationRate;

            if (isBoostingInput && verticalInput > 0)
            {
                currentMaxSpeed = forwardSpeed * boostSpeedMultiplier;
                currentAccelRate = boostAccelerationRate;
            }

            if (verticalInput > 0) // Forward
            {
                currentForwardVelocity = Mathf.MoveTowards(currentForwardVelocity, currentMaxSpeed, currentAccelRate * Time.deltaTime);
            }
            else if (verticalInput < 0) // Backward
            {
                currentForwardVelocity = Mathf.MoveTowards(currentForwardVelocity, -backwardSpeed, accelerationRate * Time.deltaTime);
            }
            else if (!isBrakingInput) // No input and not braking, slow down
            {
                currentForwardVelocity = Mathf.MoveTowards(currentForwardVelocity, 0, decelerationRate * Time.deltaTime);
            }

            // Braking
            if (isBrakingInput)
            {
                currentForwardVelocity = Mathf.MoveTowards(currentForwardVelocity, 0, brakeForce * Time.deltaTime);
                // Play brake sound if car is moving
                if (audioController != null && rb.linearVelocity.magnitude > 0.5f)
                {
                    audioController.PlayBrakeSound();
                }
            }
        }

        void FixedUpdate ()
        {
            // Move car with Rigidbody.linearVelocity
            Vector3 targetHorizontalVelocity = transform.forward * currentForwardVelocity;
            rb.linearVelocity = new Vector3(targetHorizontalVelocity.x, rb.linearVelocity.y, targetHorizontalVelocity.z);

            // Steering
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                float turnAmount = horizontalInput * turnSpeed * Time.fixedDeltaTime;
                if (Vector3.Dot(rb.linearVelocity, transform.forward) < 0)
                {
                    turnAmount *= -1;
                }
                Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
                rb.MoveRotation(rb.rotation * turnRotation);
            }

            // Auto-right (anti-flip)
            HandleAutoRight();

            // Engine sound by speed
            if (audioController != null)
            {
                float maxRelevantSpeed = isBoostingInput ? (forwardSpeed * boostSpeedMultiplier) : forwardSpeed;
                audioController.SetEngineSoundProperties(rb.linearVelocity.magnitude, maxRelevantSpeed);
            }
        }

        // Prevent car from flipping over
        void HandleAutoRight ()
        {
            if (rb.linearVelocity.magnitude < minSpeedForAutoRight && !Input.anyKey)
            {
                return;
            }

            float angleX = transform.localEulerAngles.x;
            float angleZ = transform.localEulerAngles.z;

            if (angleX > 180) angleX -= 360;
            if (angleZ > 180) angleZ -= 360;

            if (Mathf.Abs(angleX) > autoRightThreshold || Mathf.Abs(angleZ) > autoRightThreshold)
            {
                Quaternion targetRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * autoRightSpeed);
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Car lights functions

        public void ToggleHeadlights ()
        {
            areHeadlightsOn = !areHeadlightsOn;
            SetHeadlightsState(areHeadlightsOn);
        }

        private void SetHeadlightsState (bool enable)
        {
            foreach (Light headlight in headlights)
            {
                if (headlight != null)
                {
                    headlight.enabled = enable;
                }
            }
        }

        private void SetBrakeLightsState (bool enable)
        {
            foreach (Light brakeLight in brakeLights)
            {
                if (brakeLight != null)
                {
                    brakeLight.enabled = enable;
                }
            }
        }

        // Reverse lights functions
        private void SetReverseLightsState (bool enable)
        {
            foreach (Light reverseLight in reverseLights)
            {
                if (reverseLight != null)
                {
                    reverseLight.enabled = enable;
                }
            }
        }

        // Day/Night functions

        public void ToggleDayNightTheme ()
        {
            isCurrentlyDay = !isCurrentlyDay;

            if (isCurrentlyDay)
            {
                RenderSettings.skybox = daySkyboxMaterial;
                TurnCityLights(false);
                if (mainDirectionalLight != null)
                {
                    mainDirectionalLight.intensity = 1f;
                }
                RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f, 1f);// a notmal night
            }
            else
            {
                RenderSettings.skybox = nightSkyboxMaterial;
                TurnCityLights(true);
                if (mainDirectionalLight != null)
                {
                    mainDirectionalLight.intensity = 0f;
                }
                RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.15f, 1f);
            }
        }

        private void TurnCityLights (bool turnOn)
        {
            foreach (Light light in allCityLights)
            {
                if (light != null)
                {
                    light.enabled = turnOn;
                }
            }
        }
    }
}