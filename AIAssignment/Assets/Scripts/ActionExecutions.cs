//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//// This class stores all the decisions that the AI can make
//class Decisions
//{

//    public static bool IsOpponentAlive(AgentActions agent, GameObject enemy)
//    {
//        return enemy.GetComponent<AgentActions>().Alive;
//    }

//    public static bool IsAgentInSight(AgentActions agent, GameObject enemy)
//    {
//        return agent.IsInAttackRange(enemy);
//    }

//    public static bool IsOpponentFleeing(AgentActions agent, GameObject enemy)
//    {
//        return enemy.GetComponent<AgentActions>().Fleeing;
//    }

//    public static bool IsPowerUpClose(AgentActions agent, GameObject enemy)
//    {
//        return agent.IsInPickUpRange();
//    }

//    public static bool IsPowerUpPicked(AgentActions agent, GameObject enemy)
//    {
//        return agent.HasPowerUp;
//    }

//    public static bool IsAttackPowerHigher(AgentActions agent, GameObject enemy)
//    {
//        if (agent.PowerUp >= enemy.GetComponent<AgentActions>().PowerUp)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }

//    }

//    public static bool IsHealthHigherThan25Percent(AgentActions agent, GameObject enemy)
//    {
//        if(agent.CurrentHitPoints > 0.25 * agent.MaxHitPoints)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }



//}

//// Here are all the actions we can take
//// All actions return true once they have completed, false otherwise
//class Actions
//{


//    public static bool MoveTowardsPickup(AgentActions agent, GameObject enemy)
//    {
//        agent.MoveToPickup();
//        return true;
//    }

//    public static bool RandomWander(AgentActions agent, GameObject enemy)
//    {
//        agent.RandomWander();
//        return true;
//    }

//    public static bool MoveTowardsAgent(AgentActions agent, GameObject enemy)
//    {
//        agent.MoveTo(enemy);
//        return true;
//    }

//    public static bool AttackOpponent(AgentActions agent, GameObject enemy)
//    {
//        agent.AttackEnemy(enemy);
//        return true;
//    }

//    public static bool FleeFromBattle(AgentActions agent, GameObject enemy)
//    {
//        agent.Flee(enemy);
//        return true;
//    }

//}

//// Action interface
//public interface IAction : IComparable
//{
//    bool IsInterruptable { get; }
//    bool IsCombinable { get; }
//    bool IsCombinableWith(IAction otherAction);
//    bool IsComplete { get; }
//    int Priority { get; }
//    void Reset();
//    void Execute(AgentActions agent, GameObject enemy, float deltaTime);
//    string ToString();
//}

//// Declares the delegate for the action function
//public delegate bool ActionDelegate(AgentActions agent, GameObject enemy);

//// Base action, stoers information about the action and executes it with a delegate
//public class Action : IAction
//{
//    // the priority of this action
//    private int _priority = 0;
//    // When the action expires
//    private float _expiryTime = 0.0f;
//    // Keep track of the time
//    private float _timer = 0.0f;

//    // is this interruptable
//    private bool _interruptable = false;
//    // is this combinable
//    private bool _combinable = false;
//    // is it complete
//    private bool _complete;

//    // The action to perform
//    ActionDelegate _action;

//    public Action(ActionDelegate action, bool interruptable, bool combinable, float expireyTime, int priority)
//    {
//        _action = action;
//        _interruptable = interruptable;
//        _combinable = combinable;
//        _expiryTime = expireyTime;
//    }

//    public void Reset()
//    {
//        _timer = 0.0f;
//        _complete = false;
//    }

//    // Can this action be interrupted?
//    public bool IsInterruptable
//    {
//        get { return _interruptable; }
//    }

//    public bool IsCombinable
//    {
//        get { return _combinable; }
//    }
//    // Can this action be combined with another action?
//    public bool IsCombinableWith(IAction otherAction)
//    {
//        // Both actions must be combinable
//        if (this.IsCombinable && otherAction.IsCombinable)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }

//    }
//    // Is this action complete?
//    public bool IsComplete
//    {
//        get { return _complete; }
//        set { _complete = value; }
//    }

//    public int Priority
//    {
//        get { return _priority; }
//    }

//    // Implement the compare to interface for the priority queue
//    public int CompareTo(object toCompare)
//    {
//        // Cast from object to IAction interface to access priority value
//        IAction actionToCompare = toCompare as IAction;
//        return _priority.CompareTo(actionToCompare.Priority);
//    }

//    // Execute this action
//    public void Execute(AgentActions agent, GameObject enemy, float deltaTime)
//    {
//        // If the action has not finished
//        if (!_complete)
//        {
//            // Check if the action has had enough time to complete
//            if (_expiryTime > 0 && _timer < _expiryTime)
//            {
//                _timer += deltaTime;
//                _complete = false;
//                Debug.Log("action " + ToString() + " waiting");
//            }
//            else
//            {
//                // Execute the action and use the action return to set complete
//                _complete = _action.Invoke(agent, enemy);
//                Debug.Log("Executing " + ToString() + ", iscomplete = " + _complete.ToString());
//            }
//        }
//    }

//    // Get the string name of the action
//    public override string ToString()
//    {
//        return _action.Method.Name;
//    }
//}

//public class CompoundAction : IAction
//{
//    // The actions to perform
//    private List<IAction> _actions = new List<IAction>();

//    public CompoundAction()
//    {
//    }

//    public void Reset()
//    {
//        foreach (IAction action in _actions)
//        {
//            action.Reset();
//        }
//    }

//    public void Combine(IAction action)
//    {
//        _actions.Add(action);
//    }
//    // Can this action be interrupted?
//    public bool IsInterruptable
//    {
//        get
//        {
//            // A compound action can be interrupted if any of its actions can be interrupted
//            foreach (IAction action in _actions)
//            {
//                if (action.IsInterruptable)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }
//    }

//    // Can this action be combined with another action?
//    public bool IsCombinable
//    {
//        get
//        {
//            // A compound action can be combined if all of its actions can be combined
//            foreach (IAction action in _actions)
//            {
//                if (!action.IsCombinable)
//                {
//                    return false;
//                }
//            }
//            return true;
//        }
//    }

//    // Can this action be combined with another action?
//    public bool IsCombinableWith(IAction otherAction)
//    {
//        // A compound action can be combined if all of its actions can be combined
//        foreach (IAction action in _actions)
//        {
//            if (!action.IsCombinableWith(otherAction))
//            {
//                return false;
//            }
//        }
//        return true;
//    }

//    // Is this action complete?
//    public bool IsComplete
//    {
//        get
//        {
//            // A compound action is complete if all of its actions are complete
//            foreach (IAction action in _actions)
//            {
//                if (!action.IsComplete)
//                {
//                    return false;
//                }
//            }
//            return true;
//        }
//    }

//    // The priority of a compound action is the maximum priority of its subactions
//    public int Priority
//    {
//        get
//        {
//            int maxPriority = 0;
//            foreach (IAction action in _actions)
//            {
//                if (action.Priority > maxPriority)
//                {
//                    maxPriority = action.Priority;
//                }
//            }
//            return maxPriority;
//        }
//    }

//    // Implement the compare to interface for the priority queue
//    public int CompareTo(object toCompare)
//    {
//        // Cast from object to IAction interface to access priority value
//        IAction actionToCompare = toCompare as IAction;
//        return Priority.CompareTo(actionToCompare.Priority);
//    }

//    // Execute this compound action
//    public void Execute(AgentActions agent, GameObject enemy, float deltaTime)
//    {
//        // Execute all the actions in the action list
//        foreach (IAction action in _actions)
//        {
//            action.Execute(agent, enemy, deltaTime);
//        }
//    }
//    public override string ToString()
//    {
//        string actionsString = "";
//        foreach (IAction action in _actions)
//        {
//            actionsString += action.ToString() + ", ";
//        }
//        return actionsString;
//    }
//}

//public class ActionSequence : IAction
//{
//    // The actions to perform
//    private List<Action> _actions = new List<Action>();

//    // Currently active action index
//    private int _currentAction = 0;

//    public ActionSequence()
//    {
//    }

//    public void Reset()
//    {
//        _currentAction = 0;

//        foreach (IAction action in _actions)
//        {
//            action.Reset();
//        }
//    }

//    public void AddAction(Action action)
//    {
//        _actions.Add(action);
//    }
//    // Can this action be interrupted?
//    public bool IsInterruptable
//    {
//        get
//        {
//            // An action sequence can be interrupted if its currently active action can be interrupted
//            return _actions[_currentAction].IsInterruptable;
//        }
//    }

//    // Can this action be combined with another action?
//    public bool IsCombinable
//    {
//        get
//        {
//            // An action sequence can be combined if all of its actions can be combined
//            foreach (IAction action in _actions)
//            {
//                if (!action.IsCombinable)
//                {
//                    return false;
//                }
//            }
//            return true;
//        }
//    }

//    // Can this action be combined with another action?
//    public bool IsCombinableWith(IAction otherAction)
//    {
//        // An action sequence can be combined if all of its actions can be combined
//        foreach (IAction action in _actions)
//        {
//            if (!action.IsCombinableWith(otherAction))
//            {
//                return false;
//            }
//        }
//        return true;
//    }

//    // Is this action complete?
//    public bool IsComplete
//    {
//        get
//        {
//            // An action sequence is complete when all its sub actions are complete
//            if (_actions.Count > 0)
//            {
//                Debug.Log(ToString() + " : " + _actions[_actions.Count - 1].IsComplete.ToString());
//                return _actions[_actions.Count - 1].IsComplete;
//            }
//            Debug.Log("No actions in sequence");
//            return true;
//        }
//    }

//    // The priority of an action sequence is the priority of the currently executing action
//    public int Priority
//    {
//        get { return _actions[_currentAction].Priority; }
//    }

//    // Implement the compare to interface for the priority queue
//    public int CompareTo(object toCompare)
//    {
//        // Cast fomr object to IAction interface to access priority value
//        IAction actionToCompare = toCompare as IAction;
//        return Priority.CompareTo(actionToCompare.Priority);
//    }

//    // Execute the action sequence from the first to the last
//    public void Execute(AgentActions agent, GameObject enemy, float deltaTime)
//    {
//        // If the current action has completed go to the next action
//        if (_actions[_currentAction].IsComplete)
//        {
//            _currentAction++;

//            // If there are no more action empty the action list
//            if (_currentAction == _actions.Count)
//            {
//                _actions.Clear();
//            }
//        }

//        // make sure theres an action to execute
//        if (_currentAction < _actions.Count)
//        {
//            // Execute the current action
//            _actions[_currentAction].Execute(agent, enemy, deltaTime);
//        }
//    }

//    public override string ToString()
//    {
//        string actionsString = "";
//        foreach (IAction action in _actions)
//        {
//            actionsString += action.ToString() + ", ";
//        }
//        return actionsString;
//    }
//}

//public class ActionExecutions : MonoBehaviour
//{

//    // The list of action we are managing, sorted by priority
//    List<IAction> _actionList = new List<IAction>();
//    //The currently executing action
//    IAction _active;
//    // Keep track of time for task duration
//    Time currentTime = new Time();

//    private string _currentActionName = "";

//    public string GetCurrentActionName()
//    {
//        return _currentActionName;
//    }

//    // Make the highest priority action active
//    private void ActivateHighestPriorityAction()
//    {
//        // initialise with the first item on the list
//        IAction highestPriority = _actionList[0];

//        foreach (IAction action in _actionList)
//        {
//            if (action.Priority > highestPriority.Priority)
//            {
//                highestPriority = action;
//            }
//        }
//        _active = highestPriority;
//    }
//    // Check the action list to see if any action can interrupt the active action
//    private void CheckForInterruption()
//    {
//        foreach (IAction action in _actionList)
//        {
//            // If the active action has higher priority, don't interrupt it
//            if (action.Priority < _active.Priority)
//            {
//                continue;
//            }
//            else if (_active.IsInterruptable)
//            {
//                // replace the currently active action with the interrupting action
//                _active = action;
//            }
//        }
//    }

//    // Whenever we can combine any actions do so
//    private void Combine()
//    {
//        foreach (IAction action in _actionList)
//        {
//            // If we can combine
//            if (_active.IsCombinableWith(action))
//            {
//                // Combine the queued action with the active action
//                ((CompoundAction)_active).Combine(action);
//                _actionList.Remove(action);
//            }
//        }
//    }

//    // Adds a new action to the action queue
//    public void ScheduleAction(IAction action)
//    {
//        if (!_actionList.Contains(action))
//        {
//            // We have to do this because the action retains its old completed status
//            action.Reset();
//            _actionList.Add(action);
//        }
//    }

//    // Execute the current action
//    public void Execute(AgentActions agent, GameObject enemy, float deltaTime)
//    {
//        // Get the highest priority action
//        ActivateHighestPriorityAction();

//        // First check for interruptions
//        CheckForInterruption();

//        // Combine any actions that can be combined
//        Combine();

//        // Finally execute the current action
//        Debug.Log(_active.ToString());

//        // If the action has completed remove it from the list
//        if (_active.IsComplete)
//        {
//            if (_actionList.Count > 0)
//            {
//                _actionList.Remove(_active);
//            }
//        }
//        else
//        {
//            // If we have an action, execute it
//            if (_active != null)
//            {
//                _active.Execute(agent, enemy, deltaTime);
//            }
//        }
//    }


//}
