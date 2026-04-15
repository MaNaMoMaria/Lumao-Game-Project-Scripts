using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


// It controls sound, restart, exit, pause/resume, and makes sure only one GameManager exists (singleton).
// The class handles both UI button clicks and keyboard shortcuts for a better user experience.


namespace Lumao.Core
{
    public class GameManager : MonoBehaviour
    {

        public static GameManager Instance { get; private set; }

        [Header("Sound Control UI")] // Sound button stuff 🔊
        public Button soundToggleButton; // Button to mute/unmute
        public Sprite soundOnSprite;     // Sprite for sound ON
        public Sprite soundOffSprite;    // Sprite for sound OFF
        private Image soundButtonImage;  // Image on the button

        [Header("Pause/Game State")] // Pause controls ⏸️
        public bool isPaused = false;
        public GameObject pausePanel; // Panel to show when paused

        private bool isMuted = false; // Is sound muted?

        void Awake ()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // If another exists, destroy this one
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this GameManager when changing scenes

            AudioListener.volume = 1f; // Start with sound ON
            isMuted = false;

            // Get Image from button and set sprite
            if (soundToggleButton != null)
            {
                soundButtonImage = soundToggleButton.GetComponent<Image>();
                if (soundOnSprite != null && soundButtonImage != null)
                {
                    soundButtonImage.sprite = soundOnSprite;
                }
            }
        }

        // NEW: Handles keyboard input for Mute and Pause
        void Update ()
        {
            HandlePauseInput();
            HandleMuteInput();
        }

        // NEW: Manages ESC key for pausing and resuming the game
        void HandlePauseInput ()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        // NEW: Manages M key for toggling sound
        void HandleMuteInput ()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                // Calls the existing ToggleSound function
                ToggleSound();
            }
        }

        // NEW: Pauses the game (can be called by UI button or ESC key)
        public void PauseGame ()
        {
            isPaused = true;
            // Make sure you have connected the pausePanel in the Inspector!
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // NEW: Resumes the game (can be called by UI button or ESC key)
        public void ResumeGame ()
        {
            isPaused = false;
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            Time.timeScale = 1f;
            // Locks the mouse back for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Toggle sound ON/OFF (Original Function)
        public void ToggleSound ()
        {
            isMuted = !isMuted;
            AudioListener.volume = isMuted ? 0f : 1f;

            if (soundButtonImage != null)
            {
                if (isMuted && soundOffSprite != null)
                {
                    soundButtonImage.sprite = soundOffSprite;
                }
                else if (!isMuted && soundOnSprite != null)
                {
                    soundButtonImage.sprite = soundOnSprite;
                }
            }
        }

        // Restart the game (Original Function)
        public void RestartGame ()
        {
            //PlayerPrefs.DeleteAll(); // Clear saved stuff
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
        }

        // Exit the game (Original Function)
        public void ExitGame ()
        {
            Application.Quit(); // Quit app (works in build only)
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#endif
        }
    }
}