using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game.States;

public partial class StateMachine : Node
{
    private Dictionary<string, State> states;

    private State currentState;

    public override void _Ready() 
    {
        foreach (var item in GetChildren())
        {
            if (item is State state) 
            {
                states.Add(state.Name, state);
                RemoveChild(item);
            }
        }
        currentState = states.First().Value;
        AddChild(currentState);
        currentState.Enter();
    }

    public override void _Process(double delta)
    {
        currentState.Update((float) delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        currentState.PhysicsUpdate((float) delta);
    }

    public void SwitchTo(string stateName) 
    {
        if (!states.ContainsKey(stateName)) 
        {
            return;
        }
    
        currentState.Exit();
        RemoveChild(currentState);
        
        currentState = states[stateName];
        currentState.Enter();
    }    
}