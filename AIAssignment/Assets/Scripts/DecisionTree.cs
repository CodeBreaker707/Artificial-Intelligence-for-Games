using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class contains all the decisions made
// by the AI GameObject
class Decisions
{

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if an opponent is in agent's sight
    public static bool IsOpponentInSight(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // If the game objects of Enemy tag are within sight, the count of
        // the returned list will be greater than zero
        if (agent.GetGameObjectsInViewOfTag(Constants.EnemyTag).Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if the agents are
    // close enough to each other
    public static bool IsInAttackDistance(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return agent.IsInAttackRange(enemy);
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if the agent's opponent is fleeing
    public static bool IsOpponentFleeing(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return enemy.GetComponent<AgentActions>().Fleeing;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if a power pickup is in agent's sight
    public static bool IsPowerUpInSight(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // If the game objects of PowerUp tag are in sight, the count
        // of the returned list will be greater than zero
        if (agent.GetGameObjectsInViewOfTag(Constants.PowerUpTag).Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if a health kit is in agent's sight
    public static bool IsHealthKitInSight(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // If the game objects of HealthKit tag are in sight, the count
        // of the returned list will be greater than zero
        if (agent.GetGameObjectsInViewOfTag(Constants.HealthKitTag).Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if agent has already
    // picked up a power pickup
    public static bool IsPowerUpPicked(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return agent.HasPowerUp;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // A decision to check if the agent's attack power is equal to
    // or greater than the opponent's attack power
    public static bool IsAttackPowerHigher(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        if (agent.PowerUp >= enemy.GetComponent<AgentActions>().PowerUp)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    public static bool IsHealthHigherThan25Percent(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // If the agent's health is greater than 25% of its maximum
        //health, this function will return true
        if (agent.CurrentHitPoints > 0.25 * agent.MaxHitPoints)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

}

// A reference pointer to the decision functions
// declared in the Decisions class
public delegate bool Decision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit);


// This class contains all the actions performed
// by the AI GameObject
class Actions
{

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // An action to make the agent move towards a
    // power pickup if it's in sight
    public static bool MoveTowardsPowerUp(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.MoveTo(powerPickup);
        return true;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // An action to make the agent move towards a
    // health kit if it's in sight
    public static bool MoveTowardsHealthKit(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.MoveTo(healthKit);
        return true;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // An action to make the agent move towards its
    // opponent if it's in sight
    public static bool MoveTowardsOpponent(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.MoveTo(enemy);
        return true;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // An action to make the agent randomly wander the level
    public static bool RandomWander(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.RandomWander();
        return true;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // An action to make the agent attack its opponent
    public static bool AttackOpponent(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.AttackEnemy(enemy);
        return true;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

    // An action to make the agent flee from its
    // opponent
    public static bool FleeFromOpponent(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.Flee(enemy);
        return true;
    }

    //******************************************************************************************************************//
    //******************************************************************************************************************//

}

// A reference pointer to the action functions
// declared in the Actions class
public delegate bool Action(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit);

// An interface to contain all
// the details of the action
public interface IAction
{
    bool IsComplete { get; }

    // Function to execute the action
    void Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit);

    // Function to reset values
    void ResetValues();
}


public class SingleAction : IAction
{
    // Declaring the reference pointer
    Action main_action;

    private bool is_complete;

    // Stores the delayed time
    // after which the action will
    // be executed
    private float delay;

    // Elapsed time
    private float timer;

    // Constructor to intialise default values
    // and which action to execute
    public SingleAction(Action m_action, float time)
    {
        main_action = m_action;
        delay = time;
        is_complete = true;

    }


    public void Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        if (!is_complete)
        {
            // Timer increases with delta time
            timer += Time.deltaTime;

            // If timer execeeds the delay in execution,
            // then execute the action
            if (timer >= delay)
            {

                is_complete = main_action.Invoke(agent, enemy, powerPickup, healthKit);
            }

        }
    }

    public void ResetValues()
    {
        is_complete = false;
        timer = 0.0f;
    }

    public bool IsComplete
    {
        get { return is_complete; }
    }

}

public class SequentialActions : IAction
{
    // To store a list of actions
    List<SingleAction> sequence = new List<SingleAction>();

    // To access the actions in
    // the sequence list
    int slot = 0;

    // A function to add actions into the sequence
    public void AddAction(SingleAction s_act)
    {
        sequence.Add(s_act);
    }

    public void Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {


        sequence[slot].Execute(agent, enemy, powerPickup, healthKit);


        if (sequence[slot].IsComplete)
        {
            // This check is to make sure we
            // don't go out of array bounds
            if (slot + 1 < sequence.Count)
            {
                slot++;
            }

        }


    }

    public void ResetValues()
    {
        // Resets for every action in the sequence
        foreach (SingleAction s_act in sequence)
        {
            s_act.ResetValues();
        }

        slot = 0;
    }


    public bool IsComplete
    {
        // Returns the boolean for the action in that
        // slot
        get { return sequence[slot].IsComplete; }
    }



}

// This class represents a point/junction
// used in the decision tree 
abstract class Node
{
    // It's called leaf because no other node can branch
    // from it
    // i.e. Leaf Node = Action Node
    private bool is_Leaf;

    public bool IsLeaf
    {
        get { return is_Leaf; }
        set { is_Leaf = value; }
    }

    // A node can either make a decision or arrive
    // at an action. Thus, these methods are declared
    public abstract Node MakeDecision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit);
    public abstract IAction GetAction();

}


// A class responsible for making decisions
// in the decision tree
class DecisionNode : Node
{
    // If the decision is postive, we use this child
    Node true_child;
    // Otherwise,
    Node false_child;

    // Declaring the reference pointer
    Decision m_decision;

    public DecisionNode(Decision decision)
    {
        IsLeaf = false;

        true_child = null;
        false_child = null;

        m_decision = decision;
    }

    // Child Nodes are added
    public void AddTrueChild(Node child)
    {
        true_child = child;
    }

    public void AddFalseChild(Node child)
    {
        false_child = child;
    }

    public override Node MakeDecision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // The reference pointer will now call the function it was assigned to
        // and return a boolean of either true or false
        if (m_decision.Invoke(agent, enemy, powerPickup, healthKit) == true)
        {
            return true_child;
        }
        else
        {
            return false_child;
        }
    }

    // This node doesn't bother about actions, so
    // we do nothing with it
    public override IAction GetAction()
    {
        return null;
    }

}

// A class responsible for returning the leaf action
// from the decision tree
class ActionNode : Node
{
    // Declaring the action assigned to
    // this node
    IAction m_action;

    public ActionNode(IAction action)
    {
        IsLeaf = true;

        m_action = action;

    }

    // This class doesn't bother about decisions, so
    // we do nothing with it
    public override Node MakeDecision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return null;
    }


    public override IAction GetAction()
    {
        return m_action;
    }

}

// A class to execute the leaf action
public class ActionExecutor
{
    // The action that will get executed
    IAction current_action;

    public ActionExecutor(IAction default_action)
    {
        current_action = default_action;
    }


    public void SetNewAction(IAction new_action)
    {
        // If the current action is complete and it's not
        // the same as the new action, then
        // the new action becomes the current action
        if (current_action.IsComplete)
        {
            if (new_action != current_action)
            {
                current_action = new_action;
            }

            current_action.ResetValues();
        }

    }

    // Current action gets executed
    public void Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        current_action.Execute(agent, enemy, powerPickup, healthKit);

    }
}

// The class comprising the decision tree algorithm
class DecisionTree
{
    // A node from where the tree will begin
    // its traversal
    Node root_node;

    Node current_node;

    IAction leaf_action;

    public DecisionTree(Node root)
    {
        root_node = root;
        current_node = root_node;

        leaf_action = null;

    }

    // The tree will begin its traversal from here
    public IAction Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        Traverse(root_node, agent, enemy, powerPickup, healthKit);
        return leaf_action;
    }

    public void Traverse(Node cur_node, AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // If the current node is not an action node, then make a
        // decision
        if (cur_node.IsLeaf)
        {
            leaf_action = cur_node.GetAction();
        }
        else
        {
            current_node = cur_node.MakeDecision(agent, enemy, powerPickup, healthKit);

            // The function is called from within the function itself almost
            // like a loop until it arrives at an action node
            Traverse(current_node, agent, enemy, powerPickup, healthKit);
        }

    }


}