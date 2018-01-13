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
    //This is the script containing the AI agents actions
    //e.g.agentScript.MoveTo(enemy);
    private DecisionTree decision_tree;
    private ActionExecutor action_executor;

    private float closestDistance = 100.0f;

    private AgentActions agentScript;

    private List<GameObject> list_enemies;
    private GameObject enemy;

    private List<GameObject> list_power_pickups;
    private GameObject power_pickup;

    private List<GameObject> list_health_kits;
    private GameObject health_kit;


    public void Awake()
    {
        agentScript = this.gameObject.GetComponent<AgentActions>();

        Decision HealthHigh = new Decision(Decisions.IsHealthHigherThan25Percent);
        DecisionNode HealthHighDecision = new DecisionNode(HealthHigh);

        Decision sightDecision = new Decision(Decisions.IsAgentInSight);

        DecisionNode inSightWithLessHealthDecision = new DecisionNode(sightDecision);

        
        DecisionNode inSightWithMoreHealthDecision = new DecisionNode(sightDecision);

        HealthHighDecision.AddFalseChild(inSightWithLessHealthDecision);
        HealthHighDecision.AddTrueChild(inSightWithMoreHealthDecision);

        Decision isHealthKitInSight = new Decision(Decisions.IsHealthKitInSight);
        DecisionNode isHealthKitInSightDecision = new DecisionNode(isHealthKitInSight);

        SingleAction FleeBattle = new SingleAction(Actions.FleeFromBattle, 0);
        ActionNode FleeBattleAction = new ActionNode(FleeBattle);
        

        inSightWithLessHealthDecision.AddFalseChild(isHealthKitInSightDecision);
        inSightWithLessHealthDecision.AddTrueChild(FleeBattleAction);

        SingleAction randomWander = new SingleAction(Actions.RandomWander, 0);
        ActionNode randomWanderAction = new ActionNode(randomWander);

        SingleAction moveTowardsHealthKit = new SingleAction(Actions.MoveTowardsHealthKit, 0);
        ActionNode MoveToHealthAction = new ActionNode(moveTowardsHealthKit);

        isHealthKitInSightDecision.AddFalseChild(randomWanderAction);
        isHealthKitInSightDecision.AddTrueChild(MoveToHealthAction);


        Decision PowerUpPickedDecision = new Decision(Decisions.IsPowerUpPicked);
        DecisionNode isPowerUpPickedDecision = new DecisionNode(PowerUpPickedDecision);

        Decision OpponentFleeing = new Decision(Decisions.IsOpponentFleeing);
        DecisionNode isOpponentFleeingDecision = new DecisionNode(OpponentFleeing);

        inSightWithMoreHealthDecision.AddFalseChild(isPowerUpPickedDecision);
        inSightWithMoreHealthDecision.AddTrueChild(isOpponentFleeingDecision);


        Decision isPowerUpPickupInSight = new Decision(Decisions.IsPowerUpInSight);
        DecisionNode isPickUpInSightDecision = new DecisionNode(isPowerUpPickupInSight);

        isPowerUpPickedDecision.AddFalseChild(isPickUpInSightDecision);
        isPowerUpPickedDecision.AddTrueChild(randomWanderAction);

        SingleAction moveToPPU = new SingleAction(Actions.MoveTowardsPickup, 0);
        ActionNode moveToPPUAction = new ActionNode(moveToPPU);

        isPickUpInSightDecision.AddFalseChild(randomWanderAction);
        isPickUpInSightDecision.AddTrueChild(moveToPPUAction);


        Decision attackHigher = new Decision(Decisions.IsAttackPowerHigher);
        DecisionNode attackHigherDecision = new DecisionNode(attackHigher);

        isOpponentFleeingDecision.AddFalseChild(attackHigherDecision);
        isOpponentFleeingDecision.AddTrueChild(randomWanderAction);

        Decision inAttackRange = new Decision(Decisions.IsInAttackDistance);
        DecisionNode inAttackRangeDecision = new DecisionNode(inAttackRange);

        attackHigherDecision.AddFalseChild(FleeBattleAction);
        attackHigherDecision.AddTrueChild(inAttackRangeDecision);

        SingleAction moveToAgent = new SingleAction(Actions.MoveTowardsAgent, 0);
        ActionNode moveToAgentAction = new ActionNode(moveToAgent);

        
        SingleAction attackEnemy = new SingleAction(Actions.AttackOpponent, 0.15f);
        ActionNode attackEnemyAction = new ActionNode(attackEnemy);

        inAttackRangeDecision.AddFalseChild(moveToAgentAction);
        inAttackRangeDecision.AddTrueChild(attackEnemyAction);
        

        decision_tree = new DecisionTree(HealthHighDecision);
        
        action_executor = new ActionExecutor(randomWander);

    }

    //Use this for initialization

   void Start ()
    {        

        list_enemies = new List<GameObject>();
        list_power_pickups = new List<GameObject>();
        list_health_kits = new List<GameObject>();

        enemy = null;
        power_pickup = null;
        health_kit = null;       


    }

    //Update is called once per frame
    void Update()
    {
        closestDistance = Mathf.Infinity;

        enemy = null;
        power_pickup = null;
        health_kit = null;

        list_enemies = agentScript.GetGameObjectsInViewOfTag(Constants.EnemyTag);
        list_power_pickups = agentScript.GetGameObjectsInViewOfTag(Constants.PowerUpTag);
        list_health_kits = agentScript.GetGameObjectsInViewOfTag(Constants.HealthKitTag);

        for (int i = 0; i < list_enemies.Count; i++)
        {

            if (list_enemies[i].GetComponent<AgentActions>().Alive && list_enemies[i] != null)
            {
                if (Vector3.Distance(list_enemies[i].transform.position, this.gameObject.transform.position) < closestDistance)
                {
                    enemy = list_enemies[i];
                    closestDistance = Vector3.Distance(list_enemies[i].transform.position, this.gameObject.transform.position);
                }

            }
            else
            {
                list_enemies.Remove(list_enemies[i]);
            }


        }

        closestDistance = Mathf.Infinity;

        for (int i = 0; i < list_power_pickups.Count; i++)
        {
            if (list_power_pickups[i] != null)
            {
                if (Vector3.Distance(list_power_pickups[i].transform.position, this.gameObject.transform.position) < closestDistance)
                {
                    power_pickup = list_power_pickups[i];
                    closestDistance = Vector3.Distance(list_power_pickups[i].transform.position, this.gameObject.transform.position);
                }

            }
            else
            {
                list_power_pickups.Remove(list_power_pickups[i]);
            }

        }

        closestDistance = Mathf.Infinity;

        for (int i = 0; i < list_health_kits.Count; i++)
        {
            if (list_health_kits[i] != null)
            {
                if (Vector3.Distance(list_health_kits[i].transform.position, this.gameObject.transform.position) < closestDistance)
                {
                    health_kit = list_health_kits[i];
                    closestDistance = Vector3.Distance(list_health_kits[i].transform.position, this.gameObject.transform.position);
                }
            }
            else
            {
                list_health_kits.Remove(list_health_kits[i]);
            }

        }


        //use this update to execute your AI algorithm
        if (agentScript.Alive)
        {
            IAction action = decision_tree.Execute(agentScript, enemy, power_pickup, health_kit); 

            action_executor.SetNewAction(action);
            action_executor.Execute(agentScript, enemy, power_pickup, health_kit);
        }




    }

}
