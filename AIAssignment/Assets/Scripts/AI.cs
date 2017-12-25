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
public delegate bool Decision(AgentActions _agent);

// This is a sublass of node responsible for making desicions
class DecisionNode : Node
{
    // The miner
    AgentActions _agent;

    // The left and right child nodes, representing yes and no decisions respectively
    Node _yesChild;
    Node _noChild;

    // The decision to make
    Decision _decision;

    // Initialise the decision node
    public DecisionNode(Decision decision, AgentActions agent)
    {
        // Decision nodes are never leaf nodes
        _isLeaf = false;

        _yesChild = null;
        _noChild = null;

        _decision = decision;
        _agent = agent;
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
        if (_decision.Invoke(_agent))
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
    // The miner
    AgentActions _agent;
    // The action to take
    IAction _action;

    public ActionNode(IAction action, AgentActions agent)
    {
        // Action nodes are always leaf nodes
        _isLeaf = true;
        _action = action;
        _agent = agent;
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
    private AgentActions agentScript;

    public void Awake()
    {
        
    }

    // Use this for initialization
    void Start ()
    {
        agentScript = this.gameObject.GetComponent<AgentActions>();
    }

	// Update is called once per frame
	void Update ()
    {
        // use this update to execute your AI algorithm

    }
}
