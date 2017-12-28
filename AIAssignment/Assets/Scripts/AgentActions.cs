using System;
using System.Collections.Generic;
using UnityEngine;

// We'll use a static class to hold global constants
public static class Constants
{
    // The names used to identify important objects in the game
    public const String EnemyTag = "Enemy";
    public const String PowerUpTag = "Powerup";
    public const String HealthKitTag = "HealthKit";
}

public class AgentActions : MonoBehaviour
{
    // Agent stats
    public int maxHitPoints = 100;
    public const float AttackRange = 2.0f;
    public const int NormalAttackDamage = 10;
    public const float HitProbability = 0.5f;
    public const float PickUpRange = 2.0f;

    // How far will the random wander go
    private const int RandomWanderDistance = 200;
    private const int FleeDistance = 100;

    // Are we still alive
    private bool _alive = true;
    public bool Alive
    {
        get { return _alive; }
        set { _alive = value; }
    }

    private bool _fleeing = false;
    public bool Fleeing
    {
        get { return _fleeing; }
        set { _fleeing = value; }
    }

    private float _fleeTimer = 1.0f;
    public float FleeTimer
    {
        get { return _fleeTimer; }
        set { _fleeTimer = value; }
    }

    // Do we have a powerup
    private int _powerUp = 0;
    public bool HasPowerUp
    {
        get { return _powerUp > 0; }
    }

    public int PowerUp
    {
        get { return _powerUp; }
    }

    public Vector3 StartPosition;
    public GameObject Enemy;
    public GameObject powerPickUp;

    // Our current health
    public int _currentHitPoints;
    public int CurrentHitPoints
    {
        get { return _currentHitPoints; }
    }

    public int MaxHitPoints
    {
        get { return maxHitPoints; }
    }

    // Our navigation agent
    private UnityEngine.AI.NavMeshAgent _agent;

    // Check for collisions with everything when checking for a random location for the wander function
    private const int AgentLayerMask = -1;

    // Control how often we set a new random destination
    private const int RandomWanderUpdateInterval = 50;
    private int _tickToNextRandomUpdate = 0;

    // Keep track of game objects in our visual field
    private Dictionary<String, GameObject> ObjectsPercieved = new Dictionary<String, GameObject>();

    // The inventory
    //private Dictionary<String, GameObject> Inventory = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Start()
    {
        StartPosition = transform.position;
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _currentHitPoints = MaxHitPoints;
    }

    public void AddToPercievedObjectsList(GameObject seenObject)
    {
        //Debug.Log(this.name + " seen " + seenObject.name);
        // We can see the object, add it to the list of game objects we know about, unless it's already in the list
        if (!ObjectsPercieved.ContainsKey(seenObject.gameObject.name))
        {
            ObjectsPercieved.Add(seenObject.gameObject.name, seenObject.gameObject);
        }
    }

    public void RemoveFromPercievedObjectList(GameObject unseenObject)
    {
        // add it to the list of objects we can currently percieve
        if (ObjectsPercieved.ContainsKey(unseenObject.gameObject.name))
        {
            ObjectsPercieved.Remove(unseenObject.gameObject.name);
        }
    }

    // Move towards a target object
    public void MoveTo(GameObject target)
    {
        if(Fleeing == true)
        {
            Fleeing = false;
        }

        _agent.destination = target.transform.position;
    }

    public void MoveToPickup()
    {
        _agent.destination = powerPickUp.transform.position;
    }

    // Randomly wander around the level
    public void RandomWander()
    {
        if(Fleeing == true)
        {
            Fleeing = false;
        }

        // Change our direction every few ticks
        if (_tickToNextRandomUpdate >= RandomWanderUpdateInterval)
        {
            // Choose a new direction
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * RandomWanderDistance;
            randomDirection += transform.position;

            // Check we can move there
            UnityEngine.AI.NavMeshHit navHit;
            UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out navHit, RandomWanderDistance, AgentLayerMask);

            _agent.destination = navHit.position;

            _tickToNextRandomUpdate = 0;
        }
        else
        {
            // Keep track of our next direction change timer
            _tickToNextRandomUpdate++;
        }
    }

    // Check if we're with attacking range of the enemy
    public bool IsInAttackRange(GameObject enemy)
    {
        // Get the game object from the name
        if (enemy != null)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < AttackRange)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsInPickUpRange()
    {
        if(powerPickUp != null)
        {
            if (Vector3.Distance(transform.position, powerPickUp.transform.position) < PickUpRange)
            {
                return true;
            }
    
       }

        return false;

    }

    // Attack the enemy
    public void AttackEnemy(GameObject enemy)
    {
        // But only if it is the enemy
        if (enemy.CompareTag(Constants.EnemyTag))
        {
            // We may not always hit
            if (UnityEngine.Random.value < HitProbability)
            {
                int actualDamage = NormalAttackDamage;
                // Tell the enemy we hit them
                if (_powerUp > 0)
                {
                    actualDamage *= _powerUp;
                }
                enemy.GetComponent<AgentActions>().TakeDamage(actualDamage);
            }
        }
    }

    // We've been hit
    public void TakeDamage(int damage)
    {
        if (_currentHitPoints + damage > 0)
        {
            _currentHitPoints -= damage;
        }
        else
        {
            _currentHitPoints = 0;
            Die();
        }
    }

    // Heal up
    public void HealDamage(int amount)
    {
        if (_currentHitPoints + amount < MaxHitPoints)
        {
            _currentHitPoints += amount;
        }
        else
        {
            _currentHitPoints = MaxHitPoints;
        }
    }

    // We've died
    public void Die()
    {
        _alive = false;
    }

    // Use the power up
    public void UsePowerUp(int powerUpAmount)
    {
        if(!HasPowerUp)
        {
            _powerUp = powerUpAmount;
        }
    }

    // Run away, run away
    public void Flee(GameObject enemy)
    {

        Fleeing = true;
        //FleeTimer -= Time.deltaTime;

        // Turn away from the threat
        transform.rotation = Quaternion.LookRotation(transform.position - enemy.transform.position);
        Vector3 runTo = transform.position + transform.forward * _agent.speed;

        //So now we've got a Vector3 to run to and we can transfer that to a location on the NavMesh with samplePosition.
        // stores the output in a variable called hit
        UnityEngine.AI.NavMeshHit navHit;

        // Check for a point to flee to
        UnityEngine.AI.NavMesh.SamplePosition(runTo, out navHit, FleeDistance, 1 << UnityEngine.AI.NavMesh.GetAreaFromName("Walkable"));
        _agent.SetDestination(navHit.position);

        //if(FleeTimer <= 0)
        //{

        //}
        

    }

    // Check if something of interest is in range
    public bool IsObjectInView(String name)
    {
        // If we can percieve it retern it, otherwise return null
        GameObject objectPercieved;
        if (ObjectsPercieved.TryGetValue(name, out objectPercieved))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Get a percieved object, null if object is not in view
    public GameObject GetObjectInView(String name)
    {
        // If we can percieve it retern it, otherwise return null
        GameObject objectPercieved;
        if (ObjectsPercieved.TryGetValue(name, out objectPercieved))
        {
            return objectPercieved;
        }
        else
        {
            return null;
        }
    }
}