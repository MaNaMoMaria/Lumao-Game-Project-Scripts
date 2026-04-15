using UnityEngine;

// This script is for the city gate! 😅
// It can open or close the gate, change the look, and make it solid or not 🚪


namespace Lumao.Core
{
    public class CityGate : MonoBehaviour
    {
        [Header("Gate Stuff")] // Gate things here 🚪
        [Tooltip("Put the MeshRenderer for the gate here")]
        public MeshRenderer gateRenderer; // The thing that shows the gate
        [Tooltip("Put the Collider for the gate here")]
        public Collider gateCollider; // The thing that blocks or lets you go

        [Header("Gate Materials")] // How gate looks 🎨
        [Tooltip("Material for closed gate (like locked)")]
        public Material closedGateMaterial; // Looks when gate is closed
        [Tooltip("Material for open gate (like unlocked)")]
        public Material openGateMaterial;   // Looks when gate is open

        void Start ()
        {
            // Check if everything is set in Inspector
            if (gateRenderer == null || gateCollider == null || closedGateMaterial == null || openGateMaterial == null)
            {
                Debug.LogError("CityGate: Something missing! Set all stuff in Inspector plz", this);
                enabled = false;
                return;
            }

            // Gate starts closed
            SetGateState(false);
            Debug.Log("CityGate: Started! Gate is closed now");
        }

        // Call this to open the gate 🚪
        public void OpenGate ()
        {
            SetGateState(true);
        }

        // This sets if gate is open or closed
        private void SetGateState (bool isOpen)
        {
            if (gateRenderer == null || gateCollider == null)
            {
                Debug.LogError("CityGate: Gate stuff missing, can't set state");
                return;
            }

            if (isOpen)
            {
                gateRenderer.material = openGateMaterial; // Change look to open
                gateCollider.isTrigger = true; // Let people go through
                Debug.Log("CityGate: Gate is OPEN now (looks open, can go through)");
            }
            else
            {
                gateRenderer.material = closedGateMaterial; // Change look to closed
                gateCollider.isTrigger = false; // Block the way
                Debug.Log("CityGate: Gate is CLOSED now (looks closed, can't go through)");
            }
        }
    }
}
