using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpUse : MonoBehaviour
{
    public int PowerUpMultiplier = 2;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    // The agent has moved onto the power up and can use it
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Constants.EnemyTag))
        {
            other.gameObject.GetComponent<AgentActions>().UsePowerUp(PowerUpMultiplier);
        }
    }
}
