using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensing : MonoBehaviour
{
    private const int WallLayer = 9;

    private float ViewRange;
    AgentActions agentScript;

    // Use this for initialization
    void Start ()
    {
        ViewRange = GetComponent<SphereCollider>().radius;
        agentScript = transform.parent.gameObject.GetComponent<AgentActions>();
    }

    // Perceptual field collision event
    private void OnTriggerStay(Collider other)
    {
        // We only want to track enemies, powerups and health kits

        if (other.gameObject.CompareTag(Constants.EnemyTag) || other.gameObject.CompareTag(Constants.PowerUpTag) || other.gameObject.CompareTag(Constants.HealthKitTag))
        {

            // This layer mask should only register collisions with walls

            int layerMask = 1 << WallLayer;

            // Do this to prevent the raycast finding a wall behind the object and therefore treating the object as obstructed

            float objectDistance = Mathf.Min(Vector3.Distance(transform.position, other.gameObject.transform.position), ViewRange);

            // Ensure we are not looking through a wall

            if (!Physics.Raycast(transform.position, other.gameObject.transform.position - transform.position, objectDistance, layerMask))

            {
                // We can see it
                agentScript.AddToPercievedObjectsList(other.gameObject);

            }

        }


    }

    // Something has left our view
    private void OnTriggerExit(Collider other)
    {
        agentScript.RemoveFromPercievedObjectList(other.gameObject);
    }

}
