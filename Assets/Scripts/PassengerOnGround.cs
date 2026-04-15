using UnityEngine;


namespace Lumao.Core
{
    public class PassengerOnGround : MonoBehaviour
    {

        // Is passenger already picked up?
        public bool isPickedUp = false;

        // Call this when player picks up the passenger
        public void PickUpPassenger ()
        {
            isPickedUp = true;
            gameObject.SetActive(false); // Hide passenger from scene
        }
    }
}
