using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class contains all the decisions made
// by the AI GameObject
class Decisions
{

    //******************************************************************************************************************//

    public static bool IsAgentInSight(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        // If the game objects of Enemy Tag are within sight, the count of
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

    public static bool IsInAttackDistance(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return agent.IsInAttackRange(enemy);
    }

    //******************************************************************************************************************//

    public static bool IsOpponentFleeing(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return enemy.GetComponent<AgentActions>().Fleeing;
    }

    //******************************************************************************************************************//

    public static bool IsPowerUpInSight(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {

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

    public static bool IsHealthKitInSight(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
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

    public static bool IsPowerUpPicked(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return agent.HasPowerUp;
    }

    //******************************************************************************************************************//

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

}

// A reference pointer to the decision functions
// declared in the Decisions class
public delegate bool Decision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit);

// This class contains all the actions performed
// by the AI GameObject
class Actions
{

    //******************************************************************************************************************//

    public static bool MoveTowardsPickup(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.MoveTo(powerPickup);
        return true;
    }

    //******************************************************************************************************************//

    public static bool MoveTowardsHealthKit(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.MoveTo(healthKit);
        return true;
    }

    //******************************************************************************************************************//

    public static bool RandomWander(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.RandomWander();
        return true;
    }

    //******************************************************************************************************************//

    public static bool MoveTowardsAgent(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.MoveTo(enemy);
        return true;
    }

    //******************************************************************************************************************//

    public static bool AttackOpponent(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.AttackEnemy(enemy);
        return true;
    }

    //******************************************************************************************************************//

    public static bool FleeFromBattle(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        agent.Flee(enemy);
        return true;
    }

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
    void Reset();
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

    public void Reset()
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

    public void AddAction(SingleAction s_act)
    {
        sequence.Add(s_act);
    }

    public void Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {

       
        sequence[slot].Execute(agent, enemy, powerPickup, healthKit);


        if (sequence[slot].IsComplete)
        {

            if(slot + 1 < sequence.Count)
            {
                slot++;
            }

        }


    }

    public void Reset()
    {
        foreach (SingleAction act in sequence)
        {
            act.Reset();
        }

        slot = 0;
    }

    public bool IsComplete
    {

        get { return sequence[slot].IsComplete; }
    }



}

abstract class Node
{
    private bool is_Leaf;

    public bool IsLeaf
    {
        get { return is_Leaf; }
        set { is_Leaf = value; }
    }

    public abstract Node MakeDecision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit);
    public abstract IAction GetAction();

}



class DecisionNode : Node
{
    Node true_child;
    Node false_child;

    Decision current_decision;

    public DecisionNode(Decision cur_dec)
    {
        IsLeaf = false;

        true_child = null;
        false_child = null;

        current_decision = cur_dec;
    }

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
        if (current_decision.Invoke(agent, enemy, powerPickup, healthKit) == true)
        {
            return true_child;
        }
        else
        {
            return false_child;
        }
    }

    public override IAction GetAction()
    {
        return null;
    }

}

class ActionNode : Node
{
    IAction current_action;

    public ActionNode(IAction cur_action)
    {
        IsLeaf = true;

        current_action = cur_action;

    }

    public override Node MakeDecision(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        return null;
    }

    public override IAction GetAction()
    {
        return current_action;
    }
}

public class ActionExecutor : MonoBehaviour
{
    IAction executing_action;

    public ActionExecutor(IAction default_action)
    {
        executing_action = default_action;
    }


    public void SetNewAction(IAction new_action)
    {
        if (executing_action.IsComplete)
        {
            if (new_action != executing_action)
            {
                executing_action = new_action;
            }

            executing_action.Reset();
        }

    }

    public void Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {

        executing_action.Execute(agent, enemy, powerPickup, healthKit);

    }
}

class DecisionTree : MonoBehaviour
{

    Node root_node;
    Node current_node;

    IAction leaf_action;

    public DecisionTree(Node root)
    {
        root_node = root;
        current_node = root_node;

        leaf_action = null;

    }

    public IAction Execute(AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        Traverse(root_node, agent, enemy, powerPickup, healthKit);
        return leaf_action;
    }

    public void Traverse(Node cur_node, AgentActions agent, GameObject enemy, GameObject powerPickup, GameObject healthKit)
    {
        if (cur_node.IsLeaf)
        {
            leaf_action = cur_node.GetAction();
        }
        else
        {
            current_node = cur_node.MakeDecision(agent, enemy, powerPickup, healthKit);
            Traverse(current_node, agent, enemy, powerPickup, healthKit);
        }
    }


}
