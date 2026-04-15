using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 
using UnityEngine.Video;


// It plays the tutorial video and starts the main game when button is clicked.
namespace Lumao.Core
{
    public class TutorialManager : MonoBehaviour
    {
        // Set these in Inspector plz!
        public string mainGameSceneName = "MainGameScene"; // Name of main game scene
        public VideoPlayer tutorialVideoPlayer; // Reference to VideoPlayer

        // Runs at start
        void Start ()
        {
            // Make sure VideoPlayer is set
            if (tutorialVideoPlayer == null)
            {
                Debug.LogError("TutorialManager: No VideoPlayer set! Set it in Inspector plz.");
                // Try to find VideoPlayer in scene
                tutorialVideoPlayer = FindObjectOfType<VideoPlayer>();
                if (tutorialVideoPlayer == null)
                {
                    Debug.LogError("TutorialManager: No VideoPlayer found in scene. Can't play tutorial video.");
                    return;
                }
            }

            // Play video if not already playing
            if (!tutorialVideoPlayer.isPlaying)
            {
                tutorialVideoPlayer.Play();
            }
        }

        // Called when "Start Game" button is clicked
        public void OnStartGameButtonClicked ()
        {
            Debug.Log("TutorialManager: Starting main game!");
            SceneManager.LoadScene(mainGameSceneName);
        }
    }
}