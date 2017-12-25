using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUse : MonoBehaviour
{
    // The health kit will heal 50 hit points
    public int _healingAmount = 50;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    // The agent has moved onto the health and will heal
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Constants.EnemyTag))
        {
            other.gameObject.GetComponent<AgentActions>().HealDamage(_healingAmount);
        }
    }
}
