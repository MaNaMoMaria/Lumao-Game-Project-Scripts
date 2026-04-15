using UnityEngine;


// This script is for car sounds! 😅
// Put it on the same car as CarController 🚗
// It plays engine, brake, horn, and boost sounds. If something is missing, it will cry in console lol

namespace Lumao.Core
{
    public class CarAudioController : MonoBehaviour
    {
        [Header("Engine Sound")] // Engine sound stuff 🛠️
        [Tooltip("AudioSource for engine sound")]
        public AudioSource engineAudioSource;
        [Tooltip("AudioClip for engine sound (should be loop)")]
        public AudioClip engineClip;
        [Tooltip("Lowest pitch for engine (idle/slow)")]
        public float minEnginePitch = 0.8f;
        [Tooltip("Highest pitch for engine (fast/boost)")]
        public float maxEnginePitch = 1.5f;
        [Tooltip("Lowest volume for engine")]
        public float minEngineVolume = 0.3f;
        [Tooltip("Highest volume for engine")]
        public float maxEngineVolume = 1.0f;

        [Header("Brake Sound")] // Brake sound stuff 🛑
        [Tooltip("AudioSource for brake sound (one-shot)")]
        public AudioSource brakeAudioSource;
        [Tooltip("AudioClip for brake sound (like tire screech)")]
        public AudioClip brakeClip;

        [Header("Horn Sound")] // Horn sound stuff 📢
        [Tooltip("AudioSource for horn (one-shot)")]
        public AudioSource hornAudioSource;
        [Tooltip("AudioClip for horn")]
        public AudioClip hornClip;

        [Header("Boost Sound")] // Boost sound stuff 💨
        [Tooltip("AudioSource for boost (should be loop)")]
        public AudioSource boostAudioSource;
        [Tooltip("AudioClip for boost")]
        public AudioClip boostClip;

        void Start ()
        {
            // Check if all AudioSources are set
            if (engineAudioSource == null || brakeAudioSource == null || hornAudioSource == null || boostAudioSource == null)
            {
                Debug.LogError("CarAudioController: Some AudioSource missing! Set them in Inspector plz");
                enabled = false; // Turn off script if missing stuff
                return;
            }

            // Setup engine sound
            if (engineClip != null)
            {
                engineAudioSource.clip = engineClip;
                engineAudioSource.loop = true; // Engine sound should loop
                engineAudioSource.volume = minEngineVolume; // Start at low volume
                engineAudioSource.pitch = minEnginePitch; // Start at low pitch
                engineAudioSource.Play(); // Start engine sound
            }
            else
            {
                Debug.LogWarning("No engine AudioClip set!");
            }

            // Setup boost sound (don't play at start)
            if (boostClip != null)
            {
                boostAudioSource.clip = boostClip;
                boostAudioSource.loop = true; // Boost sound should loop
                boostAudioSource.Stop(); // Make sure it's stopped at start
            }
            else
            {
                Debug.LogWarning("No boost AudioClip set!");
            }

            // Brake and horn sounds are one-shot, no need to setup
        }

        // Change engine sound pitch and volume based on car speed
        // currentSpeed: how fast car is going
        // maxSpeed: max car speed
        public void SetEngineSoundProperties (float currentSpeed, float maxSpeed)
        {
            if (engineAudioSource == null || engineClip == null) return;

            // Normalize speed (0 to 1)
            float normalizedSpeed = Mathf.InverseLerp(0f, maxSpeed, currentSpeed);

            // Set pitch and volume by speed
            engineAudioSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, normalizedSpeed);
            engineAudioSource.volume = Mathf.Lerp(minEngineVolume, maxEngineVolume, normalizedSpeed);
        }

        // Play brake sound when braking
        public void PlayBrakeSound ()
        {
            // Only play if AudioSource and AudioClip are set and not already playing
            if (brakeAudioSource != null && brakeClip != null && !brakeAudioSource.isPlaying)
            {
                brakeAudioSource.PlayOneShot(brakeClip);
            }
        }

        // Play horn sound when honking
        public void PlayHornSound ()
        {
            // Only play if AudioSource and AudioClip are set and not already playing
            if (hornAudioSource != null && hornClip != null && !hornAudioSource.isPlaying)
            {
                hornAudioSource.PlayOneShot(hornClip);
            }
        }

        // Play boost sound when boost is active
        public void StartBoostSound ()
        {
            if (boostAudioSource != null && boostClip != null && !boostAudioSource.isPlaying)
            {
                boostAudioSource.Play();
            }
        }

        // Stop boost sound when boost is off
        public void StopBoostSound ()
        {
            if (boostAudioSource != null && boostAudioSource.isPlaying)
            {
                boostAudioSource.Stop();
            }
        }
    }
}
