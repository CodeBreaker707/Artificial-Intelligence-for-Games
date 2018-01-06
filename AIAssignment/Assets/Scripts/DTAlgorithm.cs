using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class stores all the decisions that the AI can make
class Decisions
{

    public static bool IsOpponentAlive(AgentActions agent, GameObject enemy)
    {
        return enemy.GetComponent<AgentActions>().Alive;
    }

    public static bool IsAgentInSight(AgentActions agent, GameObject enemy)
    {
        return agent.IsInAttackRange(enemy);
    }

    public static bool IsOpponentFleeing(AgentActions agent, GameObject enemy)
    {
        return enemy.GetComponent<AgentActions>().Fleeing;
    }

    public static bool IsPowerUpClose(AgentActions agent, GameObject enemy)
    {
        return agent.IsInPickUpRange();
    }

    public static bool IsPowerUpPicked(AgentActions agent, GameObject enemy)
    {
        return agent.HasPowerUp;
    }

    public static bool IsAttackPowerHigher(AgentActions agent, GameObject enemy)
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

    public static bool IsHealthHigherThan25Percent(AgentActions agent, GameObject enemy)
    {
        if (agent.CurrentHitPoints > 0.25 * agent.MaxHitPoints)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



}

// Here are all the actions we can take
// All actions return true once they have completed, false otherwise
class Actions
{

    public static bool MoveTowardsPickup(AgentActions agent, GameObject enemy)
    {
        agent.MoveToPickup();
        return true;
    }

    public static bool RandomWander(AgentActions agent, GameObject enemy)
    {
        agent.RandomWander();
        return true;
    }

    public static bool MoveTowardsAgent(AgentActions agent, GameObject enemy)
    {
        agent.MoveTo(enemy);
        return true;
    }

    public static bool AttackOpponent(AgentActions agent, GameObject enemy)
    {
        agent.AttackEnemy(enemy);
        return true;
    }

    public static bool FleeFromBattle(AgentActions agent, GameObject enemy)
    {
        agent.Flee(enemy);
        return true;
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

    public abstract Node MakeDecision(AgentActions agent, GameObject enemy);
    public abstract IAction GetAction();

}

public delegate bool Decision(AgentActions agent, GameObject enemy);

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

    public override Node MakeDecision(AgentActions agent, GameObject enemy)
    {
        if (current_decision.Invoke(agent, enemy))
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

    public override Node MakeDecision(AgentActions agent, GameObject enemy)
    {
        return null;
    }

    public override IAction GetAction()
    {
        return current_action;
    }
}

public delegate bool MainAction(AgentActions agent, GameObject enemy);


public interface IAction
{
    void Execute(AgentActions agent, GameObject enemy);
    void Reset();
}


public class Action : IAction
{
    MainAction main_action;
    public bool is_complete;

    public Action(MainAction m_action)
    {
        main_action = m_action;
    }

    

    public void Execute(AgentActions agent, GameObject enemy)
    {
        if(!is_complete)
        {
            is_complete = main_action.Invoke(agent, enemy);
        }
    }

    public void Reset()
    {
        is_complete = false;
    }

}

public class SequentialActions : IAction
{
    List<Action> sequence = new List<Action>();

    public void AddAction(Action act)
    {
        sequence.Add(act);
    }

    public void Execute(AgentActions agent, GameObject enemy)
    {

        foreach (Action act in sequence)
        {
            act.Execute(agent, enemy);
        }

    }

    public void Reset()
    {
        foreach (Action act in sequence)
        {
            act.Reset();
        }
    }

    

}

public class ActionExecutor : MonoBehaviour
{
    IAction executing_action;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    public void SetNewAction(IAction new_action)
    {
        if (new_action != executing_action)
        {
            executing_action = new_action;
        }

        executing_action.Reset();

    }

    public void Execute(AgentActions agent, GameObject enemy)
    { 

        executing_action.Execute(agent, enemy);
    }
}

class DTAlgorithm : MonoBehaviour
{

    Node root_node;
    Node current_node;

    IAction leaf_action;

    public DTAlgorithm(Node root)
    {
        root_node = root;
        current_node = root_node;

        leaf_action = null;

    }

    public IAction Execute(AgentActions agent, GameObject enemy)
    {
        Traverse(root_node, agent, enemy);
        return leaf_action;
    }

    public void Traverse(Node cur_node, AgentActions agent, GameObject enemy)
    {
        if(cur_node.IsLeaf)
        {
            leaf_action = cur_node.GetAction();
        }
        else
        {
            current_node = cur_node.MakeDecision(agent, enemy);
            Traverse(current_node, agent, enemy);
        }
    }


}
