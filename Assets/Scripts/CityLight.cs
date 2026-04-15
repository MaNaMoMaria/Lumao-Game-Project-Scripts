using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// This script changes day/night in the city! 🌞🌚
// When you click the UI button or press a key, it changes the sky and turns city lights on/off.


namespace Lumao.Core
{
    public class CityLight : MonoBehaviour
    {
        [Header("Skybox Stuff")] // Skybox settings 🌤️
        [Tooltip("Put day skybox material here")]
        public Material daySkyboxMaterial; // Drag day skybox here
        [Tooltip("Put night skybox material here")]
        public Material nightSkyboxMaterial; // Drag night skybox here

        [Header("City Lights Stuff")] // City lights settings 💡
        [Tooltip("Tag for all city light objects (like 'CityLight')")]
        public string cityLightsTag = "CityLight"; // Tag for city lights

        // List for all city lights found in scene
        private List<Light> allCityLights = new List<Light>();

        [Header("UI Button Stuff")] // UI button settings 🖱️
        [Tooltip("Put the UI button here to change day/night")]
        public Button toggleDayNightButton; // Drag UI button here

        [Header("Sun Light Stuff")] // Sun light settings ☀️
        [Tooltip("Main Directional Light (the sun)")]
        public Light mainDirectionalLight; // Drag sun light here

        [Header("Keyboard Key Stuff")] // Keyboard key settings ⌨️
        [Tooltip("Key to change day/night (like KeyCode.P)")]
        public KeyCode toggleKey = KeyCode.P; // Default is P

        // Is it day now? true = day, false = night
        private bool isCurrentlyDay = true;

        // Runs once at start
        void Start ()
        {
            // Check if skybox materials are set
            if (daySkyboxMaterial == null || nightSkyboxMaterial == null)
            {
                Debug.LogError("CityLight: No skybox materials set! Set them in Inspector plz.");
                enabled = false;
                return;
            }

            // Check if UI button is set
            if (toggleDayNightButton == null)
            {
                Debug.LogError("CityLight: No UI button set! Drag the button in Inspector plz.");
                enabled = false;
                return;
            }

            // Check if sun light is set
            if (mainDirectionalLight == null)
            {
                Debug.LogWarning("CityLight: No sun light set! Can't change sun intensity.");
            }

            // Add listener to UI button
            toggleDayNightButton.onClick.AddListener(ToggleDayNightTheme);

            // Find all city lights by tag
            GameObject [] lightObjectsWithTag = GameObject.FindGameObjectsWithTag(cityLightsTag);

            if (lightObjectsWithTag.Length == 0)
            {
                Debug.LogWarning("CityLight: No objects with tag '" + cityLightsTag + "' found! Check your city lights tags.");
            }

            // Get Light component from each object and add to list
            foreach (GameObject go in lightObjectsWithTag)
            {
                Light lightComponent = go.GetComponent<Light>();
                if (lightComponent != null)
                {
                    allCityLights.Add(lightComponent);
                }
                else
                {
                    Debug.LogWarning("CityLight: Object with tag '" + cityLightsTag + "' has no Light component: " + go.name);
                }
            }

            // Set scene to day at start: day skybox, city lights off, sun on
            RenderSettings.skybox = daySkyboxMaterial;
            TurnCityLights(false); // Turn off city lights
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = 1f; // Sun bright for day
            }
        }

        // Runs every frame
        void Update ()
        {
            // If key pressed, change day/night
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDayNightTheme();
            }
        }

        // This changes day/night theme
        // Called by UI button or keyboard
        public void ToggleDayNightTheme ()
        {
            isCurrentlyDay = !isCurrentlyDay; // Flip day/night

            if (isCurrentlyDay)
            {
                RenderSettings.skybox = daySkyboxMaterial;
                TurnCityLights(false); // Turn off city lights
                if (mainDirectionalLight != null)
                {
                    mainDirectionalLight.intensity = 1f; // Sun bright for day
                }
            }
            else
            {
                RenderSettings.skybox = nightSkyboxMaterial;
                TurnCityLights(true); // Turn on city lights
                if (mainDirectionalLight != null)
                {
                    mainDirectionalLight.intensity = 0f; // Sun off for night
                }
            }
        }

        // Turns all city lights on or off
        private void TurnCityLights (bool turnOn)
        {
            foreach (Light light in allCityLights)
            {
                if (light != null)
                {
                    light.enabled = turnOn; // true = on, false = off
                }
            }
        }
    }
}
