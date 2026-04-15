using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script is for race timer! 😅
// It shows timer, best record, start/finish gates, and "Good Job!" message.


namespace Lumao.Core
{
    public class RaceTimerManager : MonoBehaviour
    {
        [Header("UI Stuff")] // UI stuff 🖥️
        public TextMeshProUGUI theRaceTimerText;
        public TextMeshProUGUI bestRecordText;
        public Button startRaceBigButton;
        public TextMeshProUGUI countdownDisplay;
        public TextMeshProUGUI goodJobText;

        [Header("Race Gates")] // Gates 🚧
        public GameObject startGate;
        public GameObject endGate;

        [Header("Game Settings")] // Settings ⚙️
        public float countdownTime = 3f;
        public float buttonAutoHideDelay = 4f;
        public string bestScoreKey = "MyBestRaceTimeEver";

        [Header("Sound Effects")] // Sounds 🔊
        public AudioSource raceAudioPlayer;
        public AudioClip countdownBeepSoundEffect;
        public AudioClip goSoundEffect;
        public AudioClip raceFinishSoundEffect;

        private bool raceIsOn = false;
        private bool playerIsAtStartSpot = false;
        private float raceStartTime;
        private float currentLapTime;
        private float myBestTimeSoFar;
        private Coroutine hideButtonCoroutineHandle;

        // Called at start
        void Start ()
        {
            // Check all UI and gate fields
            if (theRaceTimerText == null || bestRecordText == null || startRaceBigButton == null || countdownDisplay == null ||
                startGate == null || endGate == null || goodJobText == null)
            {
                Debug.LogError("Hey! Some UI or Gate stuff is missing in Inspector! Check your assignments, dude.", this);
                this.enabled = false;
                return;
            }

            // Check AudioSource
            if (raceAudioPlayer == null)
            {
                raceAudioPlayer = GetComponent<AudioSource>();
            }

            // Hide UI at start
            startRaceBigButton.gameObject.SetActive(false);
            countdownDisplay.gameObject.SetActive(false);
            goodJobText.gameObject.SetActive(false);
            theRaceTimerText.text = "00:00.00";

            // Load best time from PlayerPrefs
            myBestTimeSoFar = PlayerPrefs.GetFloat(bestScoreKey, Mathf.Infinity);
            ShowBestTimeOnScreen();
        }

        // Called every frame
        void Update ()
        {
            // If race is on, show timer
            if (raceIsOn)
            {
                currentLapTime = Time.time - raceStartTime;
                ShowCurrentTimeOnScreen();
            }

            // If player at start and presses Enter, start race
            if (playerIsAtStartSpot && !raceIsOn && Input.GetKeyDown(KeyCode.Return))
            {
                StartRaceButtonClickAction();
            }
        }

        // When player enters a gate
        private void OnTriggerEnter (Collider otherCollision)
        {
            if (otherCollision.CompareTag("StartTrigger"))
            {
                // Show start button if not racing
                if (!raceIsOn)
                {
                    playerIsAtStartSpot = true;
                    startRaceBigButton.gameObject.SetActive(true);
                    theRaceTimerText.text = "Get Ready!";

                    if (hideButtonCoroutineHandle != null) StopCoroutine(hideButtonCoroutineHandle);
                    hideButtonCoroutineHandle = StartCoroutine(HideStartButtonAfterDelay());
                }
            }
            else if (otherCollision.CompareTag("EndTrigger"))
            {
                // Finish race if racing
                if (raceIsOn)
                {
                    FinishTheRace();
                }
            }
        }

        // When player exits start gate
        private void OnTriggerExit (Collider otherCollision)
        {
            if (otherCollision.CompareTag("StartTrigger"))
            {
                if (!raceIsOn)
                {
                    playerIsAtStartSpot = false;
                    startRaceBigButton.gameObject.SetActive(false);
                    if (hideButtonCoroutineHandle != null)
                    {
                        StopCoroutine(hideButtonCoroutineHandle);
                        hideButtonCoroutineHandle = null;
                    }
                    theRaceTimerText.text = "00:00.00";
                }
            }
        }

        // When start button or Enter is pressed
        public void StartRaceButtonClickAction ()
        {
            if (playerIsAtStartSpot && !raceIsOn)
            {
                if (hideButtonCoroutineHandle != null)
                {
                    StopCoroutine(hideButtonCoroutineHandle);
                    hideButtonCoroutineHandle = null;
                }
                startRaceBigButton.gameObject.SetActive(false);
                StartCoroutine(DoCountdownAndBeginRace());
            }
        }

        // Hide start button after delay (coroutine)
        private System.Collections.IEnumerator HideStartButtonAfterDelay ()
        {
            yield return new WaitForSeconds(buttonAutoHideDelay);
            if (playerIsAtStartSpot && !raceIsOn)
            {
                startRaceBigButton.gameObject.SetActive(false);
                playerIsAtStartSpot = false;
                theRaceTimerText.text = "00:00.00";
            }
        }

        // Countdown and start race (coroutine)
        private System.Collections.IEnumerator DoCountdownAndBeginRace ()
        {
            countdownDisplay.gameObject.SetActive(true);
            float currentCountdown = countdownTime;

            while (currentCountdown > 0)
            {
                countdownDisplay.text = Mathf.CeilToInt(currentCountdown).ToString();
                if (raceAudioPlayer != null && countdownBeepSoundEffect != null)
                {
                    raceAudioPlayer.PlayOneShot(countdownBeepSoundEffect);
                }
                yield return new WaitForSeconds(1f);
                currentCountdown--;
            }

            countdownDisplay.text = "GO!";
            if (raceAudioPlayer != null && goSoundEffect != null)
            {
                raceAudioPlayer.PlayOneShot(goSoundEffect);
            }
            yield return new WaitForSeconds(0.5f);
            countdownDisplay.gameObject.SetActive(false);

            ActuallyStartTheRace();
        }

        // Start the race
        private void ActuallyStartTheRace ()
        {
            raceIsOn = true;
            raceStartTime = Time.time;
            theRaceTimerText.color = Color.white;
        }

        // Finish the race
        private void FinishTheRace ()
        {
            raceIsOn = false;
            playerIsAtStartSpot = false;
            startRaceBigButton.gameObject.SetActive(false);
            theRaceTimerText.color = Color.green;

            if (raceAudioPlayer != null && raceFinishSoundEffect != null)
            {
                raceAudioPlayer.PlayOneShot(raceFinishSoundEffect);
            }

            // If new record, save it
            if (currentLapTime < myBestTimeSoFar)
            {
                myBestTimeSoFar = currentLapTime;
                PlayerPrefs.SetFloat(bestScoreKey, myBestTimeSoFar);
                ShowBestTimeOnScreen();
                theRaceTimerText.text = "Your Time: " + currentLapTime.ToString("F2") + "\nNEW RECORD!";
            }
            else
            {
                theRaceTimerText.text = "Your Time: " + currentLapTime.ToString("F2");
            }

            // Show "Good Job!" message
            if (goodJobText != null)
            {
                goodJobText.text = "Good Job!";
                goodJobText.gameObject.SetActive(true);
                StartCoroutine(HideGoodJobTextAfterDelay(3f));
            }
        }

        // Hide "Good Job!" after delay (coroutine)
        private System.Collections.IEnumerator HideGoodJobTextAfterDelay (float delay)
        {
            yield return new WaitForSeconds(delay);
            if (goodJobText != null)
            {
                goodJobText.gameObject.SetActive(false);
            }
        }

        // Show current race time in UI
        void ShowCurrentTimeOnScreen ()
        {
            int minutes = (int)(currentLapTime / 60);
            int seconds = (int)(currentLapTime % 60);
            int milliseconds = (int)((currentLapTime * 100) % 100);
            theRaceTimerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }

        // Show best time in UI
        void ShowBestTimeOnScreen ()
        {
            if (myBestTimeSoFar == Mathf.Infinity)
            {
                bestRecordText.text = "Best Record:\n --:--.--";
            }
            else
            {
                int minutes = (int)(myBestTimeSoFar / 60);
                int seconds = (int)(myBestTimeSoFar % 60);
                int milliseconds = (int)((myBestTimeSoFar * 100) % 100);
                bestRecordText.text = string.Format("Best Record: {0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }
        }

        // Save PlayerPrefs when quitting game
        void OnApplicationQuit ()
        {
            PlayerPrefs.Save();
        }
    }
}
