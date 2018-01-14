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
    public const float AttackRange = 1.0f;
    public const int NormalAttackDamage = 10;
    public const float HitProbability = 0.5f;

    // How far will the random wander go
    private const int RandomWanderDistance = 200;
    private const int FleeDistance = 100;

    // Are we still alive
    public bool _alive = true;

    public bool Alive
    {
        get { return _alive; }
        set { _alive = value; }
    }

    // Are we fleeing
    private bool _fleeing = false;
    public bool Fleeing
    {
        get { return _fleeing; }
        set { _fleeing = value; }
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
    private List<GameObject> seen_objects;


    //private Dictionary<String, GameObject> ObjectsPercieved = new Dictionary<String, GameObject>();

    // The inventory
    //private Dictionary<String, GameObject> Inventory = new Dictionary<string, GameObject>();


    // Use this for initialization
    void Start()
    {
        StartPosition = transform.position;
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _currentHitPoints = MaxHitPoints;

        seen_objects = new List<GameObject>();

    }

    // If we can see the object, add it to the list of game objects 
    // we know about, unless it's already in the list
    public void AddToSeenObjects(GameObject seen_obj)
    {
        if(!seen_objects.Contains(seen_obj))
        {
            seen_objects.Add(seen_obj);
        }

    }

    // If we can't see the object, remove it from the list of game objects
    // we know about if it contains in the list
    public void RemoveFromSeenObjects(GameObject unseen_obj)
    {
        if(seen_objects.Contains(unseen_obj))
        {
            seen_objects.Remove(unseen_obj);
        }

    }

    // Move towards a target object
    public void MoveTo(GameObject target)
    {
        if (Fleeing == true)
        {
            Fleeing = false;
        }

        _agent.destination = target.transform.position;

        
    }

    // Randomly wander around the level
    public void RandomWander()
    {

        if (Fleeing == true)
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
        if (_currentHitPoints - damage > 0)
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
        _agent.isStopped = true;
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
        
        if(Fleeing == false)
        {
            Fleeing = true;

        }            

        // Turn away from the threat
        transform.rotation = Quaternion.LookRotation(transform.position - enemy.transform.position);
        Vector3 runTo = transform.position + transform.forward * _agent.speed;

        //So now we've got a Vector3 to run to and we can transfer that to a location on the NavMesh with samplePosition.
        // stores the output in a variable called hit
        UnityEngine.AI.NavMeshHit navHit;

        // Check for a point to flee to
        UnityEngine.AI.NavMesh.SamplePosition(runTo, out navHit, FleeDistance, 1 << UnityEngine.AI.NavMesh.GetAreaFromName("Walkable"));
        _agent.SetDestination(navHit.position);


    }

    // Check if something of interest is in range
    //public bool IsObjectInView(String name)
    //{
    //    // If we can perceive it return it, otherwise return null
    //    GameObject objectPercieved;
    //    if (ObjectsPercieved.TryGetValue(name, out objectPercieved))
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}


    public List<GameObject> GetGameObjectsInViewOfTag(String seen_tag)
    {
        // Creating a temporary list
        List<GameObject> temp_list;

        temp_list = new List<GameObject>();

        foreach (GameObject obj in seen_objects)
        {
            // If the tag from the parameter matches the tag
            // in the seen objects, add it to the temporary list
            if(obj.tag == seen_tag)
            {
                temp_list.Add(obj);
            }
        }

        return temp_list;
    }

}