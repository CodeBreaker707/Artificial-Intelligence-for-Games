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
    private DTAlgorithm decision_tree;
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


        Action FleeBattle = new Action(Actions.FleeFromBattle, 0);
        ActionNode FleeBattleAction = new ActionNode(FleeBattle);

        Action moveTowardsHealthKit = new Action(Actions.MoveTowardsHealthKit, 0);
        SequentialActions FleeAndMoveToHealth = new SequentialActions();
        FleeAndMoveToHealth.AddAction(FleeBattle);
        FleeAndMoveToHealth.AddAction(moveTowardsHealthKit);
        ActionNode FleeAndMoveToHealthAction = new ActionNode(FleeAndMoveToHealth);


       // Decision OpponentAlive = new Decision(Decisions.IsOpponentAlive);
       // DecisionNode isOpponentAliveDecision = new DecisionNode(OpponentAlive);

        Decision sightDecision = new Decision(Decisions.IsAgentInSight);
        DecisionNode inSightDecision = new DecisionNode(sightDecision);

        HealthHighDecision.AddFalseChild(FleeAndMoveToHealthAction);
        HealthHighDecision.AddTrueChild(inSightDecision);

        Action randomWander = new Action(Actions.RandomWander, 0);
        ActionNode randomWanderAction = new ActionNode(randomWander);
  

        //isOpponentAliveDecision.AddFalseChild(randomWanderAction);
        //isOpponentAliveDecision.AddTrueChild(inSightDecision);

        Decision pickUpDecision = new Decision(Decisions.IsPowerUpClose);
        DecisionNode isPickUpCloseDecision = new DecisionNode(pickUpDecision);

        Decision OpponentFleeing = new Decision(Decisions.IsOpponentFleeing);
        DecisionNode isOpponentFleeingDecision = new DecisionNode(OpponentFleeing);

        inSightDecision.AddFalseChild(isPickUpCloseDecision);
        inSightDecision.AddTrueChild(isOpponentFleeingDecision);

        //Decision PowerUpPickedDecision = new Decision(Decisions.IsPowerUpPicked);
        //DecisionNode isPowerUpPickedDecision = new DecisionNode(PowerUpPickedDecision);

        Action moveToPPU = new Action(Actions.MoveTowardsPickup, 0);
        ActionNode moveToPPUAction = new ActionNode(moveToPPU);

        isPickUpCloseDecision.AddFalseChild(randomWanderAction);
        isPickUpCloseDecision.AddTrueChild(moveToPPUAction);
        ////isPickUpCloseDecision.AddTrueChild(isPowerUpPickedDecision);

        ////isPowerUpPickedDecision.AddFalseChild(moveToPPUAction);
        ////isPowerUpPickedDecision.AddTrueChild(randomWanderAction);


        Decision attackHigher = new Decision(Decisions.IsAttackPowerHigher);
        DecisionNode attackHigherDecision = new DecisionNode(attackHigher);

        isOpponentFleeingDecision.AddFalseChild(attackHigherDecision);
        isOpponentFleeingDecision.AddTrueChild(randomWanderAction);


        Action moveTo = new Action(Actions.MoveTowardsAgent, 0);
        Action attackEnemy = new Action(Actions.AttackOpponent, 0.15f);
        SequentialActions MoveAndAttack = new SequentialActions();
        MoveAndAttack.AddAction(moveTo);
        MoveAndAttack.AddAction(attackEnemy);
        ActionNode attackEnemyAction = new ActionNode(MoveAndAttack);

        attackHigherDecision.AddFalseChild(FleeBattleAction);
        attackHigherDecision.AddTrueChild(attackEnemyAction);

        decision_tree = new DTAlgorithm(HealthHighDecision);
        
        action_executor = new ActionExecutor(randomWander);

    }

    //Use this for initialization

   void Start ()
    {        

        list_enemies = new List<GameObject>();
        list_power_pickups = new List<GameObject>();
        list_health_kits = new List<GameObject>();

        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if(this.gameObject != obj)
            {
                list_enemies.Add(obj);
            }
        }

        enemy = list_enemies[0];

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Powerup"))
        {
            list_power_pickups.Add(obj);
        }

        power_pickup = list_power_pickups[0];

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("HealthKit"))
        {
            list_health_kits.Add(obj);
        }

        health_kit = list_health_kits[0];


    }

    //Update is called once per frame
    void Update()
    {
        closestDistance = Mathf.Infinity;

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
