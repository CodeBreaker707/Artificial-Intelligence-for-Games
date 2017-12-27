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

// We use this abstract base class so the tree traversal algorithm only has to worry about nodes
// and not whether they are decision or action nodes
abstract class Node
{
    protected bool _isLeaf;

    // Keep track of whether we're a leaf node or not for the travesal algorithm to
    // decide whether to recurse or not
    public bool IsLeaf
    {
        get { return _isLeaf; }
    }

    // Nodes can either make decisions or take actions
    // These will be implemented in the appropriate subclasses
    public abstract IAction GetAction();
    public abstract Node MakeDecision();
}

//This declares a delagate which will actually make the decision
// A delegate is a reference to a function
public delegate bool Decision(AgentActions _agent, GameObject enemy);

// This is a sublass of node responsible for making desicions
class DecisionNode : Node
{
    // An agent and the opponent
    AgentActions _agent;
    GameObject _enemy;

    // The left and right child nodes, representing yes and no decisions respectively
    Node _yesChild;
    Node _noChild;

    // The decision to make
    Decision _decision;

    // Initialise the decision node
    public DecisionNode(Decision decision, AgentActions agent, GameObject enemy)
    {
        // Decision nodes are never leaf nodes
        _isLeaf = false;

        _yesChild = null;
        _noChild = null;

        _decision = decision;
        _agent = agent;
        _enemy = enemy;

    }

    // Add a 'yes' child node
    public void AddYesChild(Node child)
    {
        _yesChild = child;
    }

    // Add a node child node
    public void AddNoChild(Node child)
    {
        _noChild = child;
    }

    // Execute the desicion delegate and return the appropriate child node
    public override Node MakeDecision()
    {
        if (_decision.Invoke(_agent, _enemy))
        {
            // The decision is yes, return the 'yes' child
            return _yesChild;
        }
        else
        {
            // The desicion is no, return the 'no' node
            return _noChild;
        }
    }

    // This is just a place holder to stop the compiler complaing that we haven't implemented the abstract function
    public override IAction GetAction()
    {
        // Do nothing
        return null;
    }
}

class ActionNode : Node
{
    // The agents
    AgentActions _agent;
    GameObject _enemy;

    // The action to take
    IAction _action;

    public ActionNode(IAction action, AgentActions agent, GameObject enemy)
    {
        // Action nodes are always leaf nodes
        _isLeaf = true;
        _action = action;
        _agent = agent;
        _enemy = enemy;
    }

    // This is just a place holder to stop the compiler complaing that we haven't implemented the abstract function 
    public override Node MakeDecision()
    {
        // Do nothing, returning null ensures no meaningful decision is attempted
        return null;
    }

    // This function will return its stored action
    public override IAction GetAction()
    {
        return _action;
    }
}

// This is the decision tree itself, it stores the root node and the
// node currently being visited during traversal
class DecisionTree
{
    // The root node of the tree
    Node _root;
    // The node we are currently visiting
    Node _currentNode;

    IAction _selectedAction = null;

    // Initialise the tree
    public DecisionTree(Node root)
    {
        _root = root;

        // Start at the root
        _currentNode = _root;
    }

    // Start running the traversal
    public IAction Execute()
    {
        // Start traversal at the root
        Traverse(_root);
        return _selectedAction;
    }

    // Recursively traverse the tree untill we reach a leaf node
    private void Traverse(Node currentNode)
    {
        // Have we arrived at a leaf node?
        if (currentNode.IsLeaf)
        {
            //_currentActionName = currentNode.GetDelegateName();
            // Exceute the appropriate action
            _selectedAction = currentNode.GetAction();
        }
        // Otherwise continue down the tree
        else
        {
            // Decide whether to go down the 'yes' branch or the 'no' branch
            // _currentNode is set to the appropriate child node depending on the decision
            _currentNode = currentNode.MakeDecision();

            // Recurse on the child node
            Traverse(_currentNode);
        }
    }
}

public class AI : MonoBehaviour
{
    // This is the script containing the AI agents actions
    // e.g. agentScript.MoveTo(enemy);
    private DecisionTree _decisionTree;
    private ActionExecutions _actionExecutor;

    private AgentActions agentScript;
    public GameObject enemy;

    public void Awake()
    {
        agentScript = this.gameObject.GetComponent<AgentActions>();

        Decision sightDecision = new Decision(Decisions.IsAgentInSight);
        DecisionNode inSightDecision = new DecisionNode(sightDecision, agentScript, enemy);

        Decision pickUpDecision = new Decision(Decisions.IsPowerUpClose);
        DecisionNode isPickUpCloseDecision = new DecisionNode(pickUpDecision, agentScript, enemy);

        inSightDecision.AddNoChild(isPickUpCloseDecision);

        Decision PowerUpPickedDecision = new Decision(Decisions.IsPowerUpPicked);
        DecisionNode isPowerUpPickedDecision = new DecisionNode(PowerUpPickedDecision, agentScript, enemy);

        Action moveToPPU = new Action(Actions.MoveTowardsPickup, true, false, 0.5f, 1);
        ActionNode moveToPPUAction = new ActionNode(moveToPPU, agentScript, enemy);

        Action randomWander = new Action(Actions.RandomWander, true, false, 0.0f, 1);
        ActionNode randomWanderAction = new ActionNode(randomWander, agentScript, enemy);

        isPickUpCloseDecision.AddNoChild(randomWanderAction);
        isPickUpCloseDecision.AddYesChild(isPowerUpPickedDecision);

        isPowerUpPickedDecision.AddNoChild(moveToPPUAction);
        isPowerUpPickedDecision.AddYesChild(randomWanderAction);

        Decision attackHigher = new Decision(Decisions.IsAttackPowerHigher);
        DecisionNode attackHigherDecision = new DecisionNode(attackHigher, agentScript, enemy);

        inSightDecision.AddYesChild(attackHigherDecision);

        Action moveTo = new Action(Actions.MoveTowardsAgent, true, true, 0.5f, 2);
        Action attackEnemy = new Action(Actions.AttackOpponent, true, false, 0.5f, 3);
        ActionSequence MoveAndAttack = new ActionSequence();
        MoveAndAttack.AddAction(moveTo);
        MoveAndAttack.AddAction(attackEnemy);
        ActionNode attackEnemyAction = new ActionNode(MoveAndAttack, agentScript, enemy);


        attackHigherDecision.AddYesChild(attackEnemyAction);

        _decisionTree = new DecisionTree(inSightDecision);

        _actionExecutor = new ActionExecutions();

    }

    // Use this for initialization
    void Start ()
    {
        

    }

    // Update is called once per frame
    void Update ()
    {
        // use this update to execute your AI algorithm
        IAction action = _decisionTree.Execute();

        _actionExecutor.ScheduleAction(action);
        _actionExecutor.Execute(agentScript, enemy, Time.deltaTime);

    }
}
