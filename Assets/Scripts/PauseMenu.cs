using UnityEngine;
using UnityEngine.UI;


namespace Lumao.Core
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Stuff")] // UI stuff 🖥️
        public GameObject pausePanel; // Pause menu panel
        public Button resumeButton;   // Resume button
        public Button exitButton;     // Exit button

        private bool isPaused = false; // Is game paused?

        void Start ()
        {
            // Check UI fields
            if (pausePanel == null || resumeButton == null || exitButton == null)
            {
                Debug.LogError("PauseMenu: Set Pause Panel, Resume Button, and Exit Button in Inspector plz.");
                enabled = false;
                return;
            }

            pausePanel.SetActive(false); // Hide pause menu at start
        }

        void Update ()
        {
            // Press Escape to pause/unpause
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

        // Pause the game and show menu
        public void PauseGame ()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Resume the game and hide menu
        public void ResumeGame ()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
 }
