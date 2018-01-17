using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************************************************
* Write your AI code in this file here. The private variable 'agentScript' contains all the agents actions which are listed
* below. Ensure your code it clear and organised and commented.
* 
* 'agentScript' properties
* -  public bool Alive                                  // Check if we are still alive
* -  public bool PowerUp                                // Check if we have used the power up
* -  public int CurrentHitPoints                        // How many current hit points do we have
*
* 'agentScript' methods    
* -  public void MoveTo(GameObject target)               // Move towards a target object        
* -  public void RandomWander()                          // Randomly wander around the level    
* -  public bool IsInAttackRange(GameObject enemy)       // Check if we're with attacking range of the enemy    
* -  public void AttackEnemy(GameObject enemy)           // Attack the enemy
* -  public void Flee(GameObject enemy)                  // Run away
* -  public bool IsObjectInView(String name)             // Check if something of interest is in range
* -  public GameObject GetObjectInView(String name)      // Get a percieved object, null if object is not in view
* 
*****************************************************************************************************************************/

public class AI : MonoBehaviour
{    
    private DecisionTree decision_tree;
    private ActionExecutor action_executor;

    private float closest_distance = 100.0f;

    // This is the script containing the AI agents actions
    // e.g.agentScript.MoveTo(enemy);
    private AgentActions agentScript;

    // List of perceived enemies
    private List<GameObject> list_enemies;

    // To contain the closest enemy
    private GameObject enemy;

    // List of perceived power pickups
    private List<GameObject> list_power_pickups;

    // To contain the closest power pickup 
    private GameObject power_pickup;

    // List of perceived health kits
    private List<GameObject> list_health_kits;

    // To contain the closest health kit
    private GameObject health_kit;


    public void Awake()
    {

        agentScript = this.gameObject.GetComponent<AgentActions>();

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        // Declaring a decision and assigning it to a decision node
        // This process is repeated for the following decisions as well
        Decision health_high = new Decision(Decisions.IsHealthHigherThan25Percent);
        DecisionNode health_high_decision = new DecisionNode(health_high);

        Decision is_agent_in_sight = new Decision(Decisions.IsOpponentInSight);

        // Two nodes are created to perform 'Agent in Sight' decisions on both 
        // situations: when the health is low and when it isn't
        DecisionNode in_sight_with_less_health_decision = new DecisionNode(is_agent_in_sight);

        DecisionNode in_sight_with_more_health_decision = new DecisionNode(is_agent_in_sight);

        // Child nodes are being added
        // This is how the tree expands
        health_high_decision.AddFalseChild(in_sight_with_less_health_decision);
        health_high_decision.AddTrueChild(in_sight_with_more_health_decision);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        Decision is_health_kit_in_sight = new Decision(Decisions.IsHealthKitInSight);
        DecisionNode is_health_kit_in_sight_decision = new DecisionNode(is_health_kit_in_sight);

        // An action is created and assigned to an action node
        // This process is repeated for the following actions as well
        SingleAction flee_from_opponent = new SingleAction(Actions.FleeFromOpponent, 0);
        ActionNode flee_from_opponent_action = new ActionNode(flee_from_opponent);

        // If our health is less and agent is in sight, flee. 
        // Else, look for health kit
        in_sight_with_less_health_decision.AddFalseChild(is_health_kit_in_sight_decision);
        in_sight_with_less_health_decision.AddTrueChild(flee_from_opponent_action);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        SingleAction random_wander = new SingleAction(Actions.RandomWander, 0);
        ActionNode random_wander_action = new ActionNode(random_wander);

        SingleAction move_towards_health_kit = new SingleAction(Actions.MoveTowardsHealthKit, 0);
        ActionNode move_towards_health_kit_action = new ActionNode(move_towards_health_kit);

        // If health kit in in sight, move towards it. Else, execute random wander
        is_health_kit_in_sight_decision.AddFalseChild(random_wander_action);
        is_health_kit_in_sight_decision.AddTrueChild(move_towards_health_kit_action);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        Decision is_power_up_picked = new Decision(Decisions.IsPowerUpPicked);
        DecisionNode is_power_up_picked_decision = new DecisionNode(is_power_up_picked);

        Decision is_opponent_fleeing = new Decision(Decisions.IsOpponentFleeing);
        DecisionNode is_opponent_fleeing_decision = new DecisionNode(is_opponent_fleeing);

        // If we have more health and enemy is in sight, check if it's fleeing. If not,
        // then, check if we already picked a power up
        in_sight_with_more_health_decision.AddFalseChild(is_power_up_picked_decision);
        in_sight_with_more_health_decision.AddTrueChild(is_opponent_fleeing_decision);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        Decision is_power_up_in_sight = new Decision(Decisions.IsPowerUpInSight);
        DecisionNode is_power_up_in_sight_decision = new DecisionNode(is_power_up_in_sight);

        // If we haven't picked it up, then look for one. Else, execute random wander
        is_power_up_picked_decision.AddFalseChild(is_power_up_in_sight_decision);
        is_power_up_picked_decision.AddTrueChild(random_wander_action);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        SingleAction move_towards_power_up = new SingleAction(Actions.MoveTowardsPowerUp, 0);
        ActionNode move_towards_power_up_action = new ActionNode(move_towards_power_up);

        // If a pick up is in sight, then move towards it. Else, execute random wander
        is_power_up_in_sight_decision.AddFalseChild(random_wander_action);
        is_power_up_in_sight_decision.AddTrueChild(move_towards_power_up_action);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        Decision is_attack_power_higher = new Decision(Decisions.IsAttackPowerHigher);
        DecisionNode is_attack_power_higher_decision = new DecisionNode(is_attack_power_higher);

        // If the opponent is not fleeing, check if our attack power is higher. Else, execute random wander
        is_opponent_fleeing_decision.AddFalseChild(is_attack_power_higher_decision);
        is_opponent_fleeing_decision.AddTrueChild(random_wander_action);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        Decision is_in_attack_distance = new Decision(Decisions.IsInAttackDistance);
        DecisionNode is_in_attack_distance_decision = new DecisionNode(is_in_attack_distance);

        // If our attack power is equal or higher than the opponent, check if we're in attack range.
        // Else, flee from sight
        is_attack_power_higher_decision.AddFalseChild(flee_from_opponent_action);
        is_attack_power_higher_decision.AddTrueChild(is_in_attack_distance_decision);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        SingleAction move_towards_agent = new SingleAction(Actions.MoveTowardsOpponent, 0);
        ActionNode move_towards_agent_action = new ActionNode(move_towards_agent);

        SingleAction attack_opponent = new SingleAction(Actions.AttackOpponent, 0.15f);
        ActionNode attack_opponent_action = new ActionNode(attack_opponent);

        // If we're not in attack distance, then move towards opponent. Else, attack the
        // opponent
        is_in_attack_distance_decision.AddFalseChild(move_towards_agent_action);
        is_in_attack_distance_decision.AddTrueChild(attack_opponent_action);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        // Assigning the root node with the health check decision
        decision_tree = new DecisionTree(health_high_decision);

        // Assigning random wander as the default action
        action_executor = new ActionExecutor(random_wander);

    }

    // Use this for initialization
    void Start()
    {
        // Initialising the list and individual game objects
        list_enemies = new List<GameObject>();
        list_power_pickups = new List<GameObject>();
        list_health_kits = new List<GameObject>();

        enemy = null;
        power_pickup = null;
        health_kit = null;


    }

    // Update is called once per frame
    void Update()
    {
        //******************************************************************************************************************//
        //******************************************************************************************************************//

        // Setting the distance to infinity for
        // a default value
        closest_distance = Mathf.Infinity;

        // These will stay null if nothing of that type
        // is in sight
        enemy = null;
        power_pickup = null;
        health_kit = null;

        // We now get the perceived game objects of a particular tag
        // and store it to the list
        list_enemies = agentScript.GetGameObjectsInViewOfTag(Constants.EnemyTag);
        list_power_pickups = agentScript.GetGameObjectsInViewOfTag(Constants.PowerUpTag);
        list_health_kits = agentScript.GetGameObjectsInViewOfTag(Constants.HealthKitTag);

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        for (int i = 0; i < list_enemies.Count; i++)
        {
            // If the enemy is alive and active, perform the distance check
            if (list_enemies[i].GetComponent<AgentActions>().Alive && list_enemies[i] != null)
            {
                // If it's less than the closest distance, assign the individual object and
                // update the closest distance
                if (Vector3.Distance(list_enemies[i].transform.position, this.gameObject.transform.position) < closest_distance)
                {
                    enemy = list_enemies[i];
                    closest_distance = Vector3.Distance(list_enemies[i].transform.position, this.gameObject.transform.position);
                }

            }
            // Else, remove it from the list
            else
            {
                list_enemies.Remove(list_enemies[i]);
            }


        }

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        closest_distance = Mathf.Infinity;

        for (int i = 0; i < list_power_pickups.Count; i++)
        {
            if (list_power_pickups[i] != null)
            {
                if (Vector3.Distance(list_power_pickups[i].transform.position, this.gameObject.transform.position) < closest_distance)
                {
                    power_pickup = list_power_pickups[i];
                    closest_distance = Vector3.Distance(list_power_pickups[i].transform.position, this.gameObject.transform.position);
                }

            }
            else
            {
                list_power_pickups.Remove(list_power_pickups[i]);
            }

        }

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        closest_distance = Mathf.Infinity;

        for (int i = 0; i < list_health_kits.Count; i++)
        {
            if (list_health_kits[i] != null)
            {
                if (Vector3.Distance(list_health_kits[i].transform.position, this.gameObject.transform.position) < closest_distance)
                {
                    health_kit = list_health_kits[i];
                    closest_distance = Vector3.Distance(list_health_kits[i].transform.position, this.gameObject.transform.position);
                }
            }
            else
            {
                list_health_kits.Remove(list_health_kits[i]);
            }

        }

        //******************************************************************************************************************//
        //******************************************************************************************************************//

        // If we're alive, continue
        if (agentScript.Alive)
        {
            // Gets the leaf action from the tree
            IAction action = decision_tree.Execute(agentScript, enemy, power_pickup, health_kit);

            action_executor.SetNewAction(action);
            action_executor.Execute(agentScript, enemy, power_pickup, health_kit);
        }

        //******************************************************************************************************************//
        //******************************************************************************************************************//


    }

}