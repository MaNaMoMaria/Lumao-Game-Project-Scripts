using UnityEngine;
using System.Collections.Generic;


// This script is for AI cars that drive by themself! 😅
// The cars follow waypoints and try not to crash into stuff like player or buildings 🚗
// It uses trigger colliders to know when something is close, no raycast here lol
// The AI car also tries to match speed with the player car sometimes

namespace Lumao.Core
{
    public class AICarController : MonoBehaviour
    {
        [Header("Move Settings")] // How fast car goes and turns 🏎️
        public float moveSpeed = 5f; // Normal speed for car (max speed)
        public float turnSpeed = 2f; // How fast car turns to avoid or follow
        public float waypointThreshold = 3f; // How close to waypoint before going to next one

        [Header("Waypoint Settings")] // Where the car should go 👈
        public List<Transform> waypoints;
        private int currentWaypointIndex = 0; // Which waypoint car is going to

        [Header("Avoid Settings")] // For avoiding stuff with trigger 😬
        public float minFollowSpeed = 1f; // Slowest speed when near obstacle
        public float speedMatchFactor = 0.9f; // How much to match speed with car in front (0-1)

        public AudioSource crashAudioSource; // Drag your AudioSource here in Inspector
        public AudioClip crashClip; // Drag your crash sound here in Inspector

        private float currentMoveSpeed; // How fast car is moving now
        private bool isAvoidingAnything = false; // Is car trying to avoid something?
        private int avoidTurnDirection = 0; // -1 left, 1 right, 0 straight
        private Transform avoidingTargetTransform = null; // What car is avoiding

        void Start ()
        {
            currentMoveSpeed = moveSpeed; // Start at normal speed

            if (waypoints == null || waypoints.Count == 0)
            {
                Debug.LogWarning("AICarController: No waypoints for " + gameObject.name + ". Set waypoints in Inspector plz");
                enabled = false; // Turn off script if no waypoints
                return;
            }

            transform.LookAt(waypoints [currentWaypointIndex].position);
        }

        void Update ()
        {
            // If car is avoiding something (like player or building)
            if (isAvoidingAnything && avoidingTargetTransform != null)
            {
                ApplyAvoidanceLogic(avoidingTargetTransform);
            }
            else // If not avoiding, just follow waypoints
            {
                FollowWaypoints();
                currentMoveSpeed = moveSpeed; // Go back to normal speed
                avoidTurnDirection = 0; // Reset turn direction
            }


            // Move the car forward
            MoveCar();
        }

        // When something enters the trigger collider
        void OnTriggerEnter (Collider other)
        {
            // If it's the player or a building
            // You can add more tags here like "NPC_Car" if you want
            if (other.CompareTag("Player") || other.CompareTag("Building"))
            {
                Debug.Log(gameObject.name + " - Something entered my trigger: " + other.name + " tag: " + other.tag);
                isAvoidingAnything = true;
                avoidingTargetTransform = other.transform; // Save what to avoid
            }
        }

        // While something stays in the trigger collider
        void OnTriggerStay (Collider other)
        {
            // If it's still the thing we're avoiding
            if (isAvoidingAnything && other.transform == avoidingTargetTransform)
            {
                ApplyAvoidanceLogic(other.transform);
            }
        }

        // When something leaves the trigger collider
        void OnTriggerExit (Collider other)
        {
            // If it's the thing we were avoiding
            if (other.transform == avoidingTargetTransform)
            {
                Debug.Log(gameObject.name + " - Thing left my trigger: " + other.name);
                isAvoidingAnything = false;
                avoidingTargetTransform = null; // Clear what to avoid
                currentMoveSpeed = moveSpeed; // Go back to normal speed
                avoidTurnDirection = 0; // Stop turning
            }
        }

        void ApplyAvoidanceLogic (Transform target)
        {
            if (target == null) return;

            // If target has Rigidbody, match speed with it
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                float targetSpeed = targetRb.linearVelocity.magnitude;
                currentMoveSpeed = Mathf.Lerp(minFollowSpeed, targetSpeed, speedMatchFactor);
                currentMoveSpeed = Mathf.Min(currentMoveSpeed, moveSpeed);

                Debug.Log(gameObject.name + " - Target speed: " + targetSpeed.ToString("F2") + ", NPC speed set to: " + currentMoveSpeed.ToString("F2"));
            }
            else
            {
                // If no Rigidbody , slow down based on distance
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                float triggerRadius = GetComponent<Collider>().bounds.extents.z;
                currentMoveSpeed = Mathf.Lerp(minFollowSpeed, moveSpeed, distanceToTarget / (triggerRadius * 2f));
                Debug.Log(gameObject.name + " - No Rigidbody, speed by distance: " + currentMoveSpeed.ToString("F2"));
            }

            // Figure out which way to turn to avoid

            Vector3 directionToTarget = target.position - transform.position;
            float dotProduct = Vector3.Dot(transform.right, directionToTarget.normalized);

            if (dotProduct > 0.1f)
            {
                avoidTurnDirection = -1;
            }
            else if (dotProduct < -0.1f)
            {
                avoidTurnDirection = 1;
            }
            else
            {
                avoidTurnDirection = Random.Range(0, 2) * 2 - 1;
            }

            // Turn the car
            transform.Rotate(Vector3.up, avoidTurnDirection * turnSpeed * Time.deltaTime);
        }

        void FollowWaypoints ()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            if (Vector3.Distance(transform.position, waypoints [currentWaypointIndex].position) < waypointThreshold)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            }

            Vector3 targetDirection = waypoints [currentWaypointIndex].position - transform.position;
            targetDirection.y = 0;

            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        void MoveCar ()
        {
            transform.Translate(Vector3.forward * currentMoveSpeed * Time.deltaTime);
        }

        // This plays crash sound when called 😵
        void PlayCrashSound ()
        {
            if (crashAudioSource != null && crashClip != null)
            {
                crashAudioSource.PlayOneShot(crashClip); // Play the crash sound
            }
            else
            {
                Debug.LogWarning("No crash sound or AudioSource set for " + gameObject.name);
            }
        }

        // When car hits something (like player)

        void OnCollisionEnter (Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log(gameObject.name + " - Hit the player! 😬");
                PlayCrashSound();
                // SpawnCrashParticles(); ⌚Maybe later if i have time
            }
            else if (collision.gameObject.CompareTag("Building"))
            {
                Debug.Log(gameObject.name + " - Hit a building!");
            }
        }

        // This draws blue circles and lines in the editor so I can see the car path 😅
        // It helps me know where my AI car will go in the scene 👀
        void OnDrawGizmos ()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints [i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints [i].position, 0.5f);
                    if (i < waypoints.Count - 1 && waypoints [i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints [i].position, waypoints [i + 1].position);
                    }
                    else if (i == waypoints.Count - 1 && waypoints [0] != null)
                    {
                        Gizmos.DrawLine(waypoints [i].position, waypoints [0].position);
                    }
                }
            }
        }
    }
}
