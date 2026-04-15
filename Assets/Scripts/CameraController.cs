using UnityEngine;


// It controls 3 cameras: main camera (can move with mouse and zoom), hood camera (inside car), and chase camera (follows car from behind)
// The FOV (field of view) changes when car go fast 🚗💨


namespace Lumao.Core
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Refrences")] // Camera stuff here 👀
        public Camera mainCamera;      // Main camra (third person)  put it in Inspector
        public Camera hoodCamera;      // Hood camera (first person) put it in Inspector
        public Camera chaseCamera;     // Chase camera (follows car) put it in Inspector
        public Transform cameraTarget; //The thing camera looks at (car) - set in Inspector

        // Audio listeners for cameras
        private AudioListener mainCameraAudioListener;
        private AudioListener hoodCameraAudioListener;

        private AudioListener chaseCameraAudioListener; // For chase camera too

        private int currentCameraIndex = 0; // Which camera is active (0=main, 1=hood, 2=chase)

        [Header("Main Camera (Third-Person) Settings")] // Main camera settings 😎
        [Range(0.1f, 10f)]
        public float mouseSensitivity = 3f; // Mouse sensitivity for camera turn
        [Range(1f, 20f)]
        public float scrollSensitivity = 5f; // Mouse scroll for zoom
        public float minZoomDistance = 3f; // Min zoom
        public float maxZoomDistance = 10f; // Max zoom
        public float currentDistance = 5.0f; // How far camera is from car

        // Orbit camera stuff
        private float currentYaw = 0f;    // Camera turn left/right
        private float currentPitch = 15f; // Camera up/down
        public float minPitch = 5f;    // Don't look under ground
        public float maxPitch = 80f;   // Don't look at sky too much

        // Auto follow for main camera
        public float autoFollowDelay = 1.0f; // Wait before auto follow after mouse stop
        [Range(1f, 10f)]
        public float autoFollowRotationSpeed = 3f; // How fast camera turns to follow car
        private float mouseInputTimer = 0f; // Timer for mouse not moving

        [Header("Chase Camera Settings")] // Chase camera settings 🏃‍♂️
        public Vector3 chaseCameraOffset = new Vector3(0f, 3f, -7f); // Where chase camera sits behind car
        [Range(1f, 20f)]
        public float chaseCameraSmoothSpeed = 5f; // How smooth chase camera moves

        [Header("FOV (Field of View) Settings")] // FOV settings 👓
        public float defaultFOV = 60f;         // Normal FOV
        public float maxFOV = 90f;             // FOV when car is super fast
        [Range(0.01f, 0.2f)]
        public float fovSpeedEffectMultiplier = 0.05f; // How much speed changes FOV
        [Range(1f, 10f)]
        public float fovSmoothSpeed = 5f;    // How smooth FOV changes
        public float maxCarSpeedForFOV = 50f; // Max car speed for FOV math

        void Start ()
        {
            // Check if all cameras and target are set
            if (mainCamera == null || hoodCamera == null || chaseCamera == null || cameraTarget == null)
            {
                Debug.LogError("CameraManager: Please put all cameras and target in Inspector");
                enabled = false;
                return;
            }

            // Get audio listeners
            mainCameraAudioListener = mainCamera.GetComponent<AudioListener>();
            hoodCameraAudioListener = hoodCamera.GetComponent<AudioListener>();
            chaseCameraAudioListener = chaseCamera.GetComponent<AudioListener>();

            // Make sure all cameras have audio listener
            if (mainCameraAudioListener == null || hoodCameraAudioListener == null || chaseCameraAudioListener == null)
            {
                Debug.LogError("CameraManager: Some cameras missing AudioListener, add it plz");
                enabled = false;
                return;
            }

            // Start with main camera on, others off
            mainCamera.gameObject.SetActive(true);
            mainCameraAudioListener.enabled = true;
            hoodCamera.gameObject.SetActive(false);
            hoodCameraAudioListener.enabled = false;
            chaseCamera.gameObject.SetActive(false);
            chaseCameraAudioListener.enabled = false;

            // Set FOV for all cameras
            mainCamera.fieldOfView = defaultFOV;
            hoodCamera.fieldOfView = defaultFOV;
            chaseCamera.fieldOfView = defaultFOV;

            // Set zoom for main camera
            currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);

            // Hide mouse for main camera
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Set camera yaw to car rotation
            currentYaw = cameraTarget.eulerAngles.y;
        }

        void Update ()
        {
            // Press C to switch camera
            if (Input.GetKeyDown(KeyCode.C))
            {
                SwitchCamera();
            }

            // Press R to reset main camera
            if (Input.GetKeyDown(KeyCode.R) && mainCamera.gameObject.activeSelf)
            {
                ResetMainCameraRotation();
            }
        }

        // LateUpdate is good for camera follow
        void LateUpdate ()
        {
            if (cameraTarget == null) return;

            // Which camera is active
            if (mainCamera.gameObject.activeSelf)
            {
                HandleMainCameraOrbit();
                HandleCameraFOV(mainCamera);
            }
            else if (hoodCamera.gameObject.activeSelf)
            {
                HandleCameraFOV(hoodCamera);
            }
            else if (chaseCamera.gameObject.activeSelf)
            {
                HandleChaseCamera();
                HandleCameraFOV(chaseCamera);
            }
        }

        public void SwitchCamera ()
        {
            Debug.Log("Switching camera. Current index: " + currentCameraIndex);

            // Turn off current camera
            if (currentCameraIndex == 0)
            {
                mainCamera.gameObject.SetActive(false);
                mainCameraAudioListener.enabled = false;
            }
            else if (currentCameraIndex == 1)
            {
                hoodCamera.gameObject.SetActive(false);
                hoodCameraAudioListener.enabled = false;
            }
            else if (currentCameraIndex == 2)
            {
                chaseCamera.gameObject.SetActive(false);
                chaseCameraAudioListener.enabled = false;
            }

            // Go to next camera
            currentCameraIndex = (currentCameraIndex + 1) % 3;

            // Turn on new camera
            if (currentCameraIndex == 0)
            {
                mainCamera.gameObject.SetActive(true);
                mainCameraAudioListener.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                currentYaw = cameraTarget.eulerAngles.y;
                mouseInputTimer = 0f;
            }
            else if (currentCameraIndex == 1)
            {
                hoodCamera.gameObject.SetActive(true);
                hoodCameraAudioListener.enabled = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (currentCameraIndex == 2)
            {
                chaseCamera.gameObject.SetActive(true);
                chaseCameraAudioListener.enabled = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            Debug.Log("Camera switched to index: " + currentCameraIndex);
        }

        void ResetMainCameraRotation ()
        {
            // Put main camera behind car again
            currentYaw = cameraTarget.eulerAngles.y;
            currentPitch = 15f;
            currentDistance = 5.0f;
            mouseInputTimer = 0f;
            Debug.Log("Main camera rotation reset");
        }

        void HandleMainCameraOrbit ()
        {
            // Zoom with mouse scroll
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            currentDistance = Mathf.Clamp(currentDistance - scrollInput * scrollSensitivity, minZoomDistance, maxZoomDistance);

            // Turn camera with mouse
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // If mouse moved
            if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
            {
                currentYaw += mouseX * mouseSensitivity;
                currentPitch -= mouseY * mouseSensitivity;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
                mouseInputTimer = 0f;
            }
            else
            {
                mouseInputTimer += Time.deltaTime;
                if (mouseInputTimer >= autoFollowDelay)
                {
                    // Camera slowly turns to follow car
                    float targetYaw = cameraTarget.eulerAngles.y;
                    currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * autoFollowRotationSpeed);
                }
            }

            // Set camera rotation and position
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
            Vector3 desiredPosition = cameraTarget.position - (rotation * Vector3.forward * currentDistance);
            mainCamera.transform.position = desiredPosition;
            mainCamera.transform.LookAt(cameraTarget.position);
        }

        // Chase camera follows car from behind
        void HandleChaseCamera ()
        {
            Vector3 targetPosition = cameraTarget.TransformPoint(chaseCameraOffset);
            chaseCamera.transform.position = Vector3.Lerp(chaseCamera.transform.position, targetPosition, Time.deltaTime * chaseCameraSmoothSpeed);
            chaseCamera.transform.LookAt(cameraTarget.position);
        }

        // FOV changes when car go fast
        void HandleCameraFOV (Camera activeCamera)
        {
            Rigidbody rb = cameraTarget.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float speed = rb.linearVelocity.magnitude;
                float speedNormalized = Mathf.InverseLerp(0, maxCarSpeedForFOV, speed);
                float targetFOV = Mathf.Lerp(defaultFOV, maxFOV, speedNormalized * fovSpeedEffectMultiplier);
                activeCamera.fieldOfView = Mathf.Lerp(activeCamera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
            }
        }
    }
}


















































