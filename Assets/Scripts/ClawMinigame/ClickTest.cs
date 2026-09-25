using UnityEngine;

namespace DinoDig
{
    public class ClickTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log(
                "Physics2D.queriesHitTriggers: "
                + Physics2D.queriesHitTriggers
            );
        }

        private void OnMouseDown()
        {
            Debug.Log("DIRECT CLICK DETECTED: " + gameObject.name);
        }
    }
}