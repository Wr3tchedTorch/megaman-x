using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game.States;

public partial class StateMachine : Node
{
    private CharacterBody2D owner;

    private readonly Dictionary<string, State> states = new();
    private State currentState;

    public override void _Ready() 
    {        
        owner = GetParent<CharacterBody2D>();

        foreach (var item in GetChildren())
        {
            if (item is State state) 
            {
                states.Add(state.Name, state);
                state.owner = owner;                
                state.Exit();
                
                RemoveChild(state);
            }
        }
        currentState = states.First().Value;
        
        AddChild(currentState);
        currentState.Enter();
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(currentState)) return;
        
        currentState.Update((float) delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsInstanceValid(currentState)) return;

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

        AddChild(currentState);
        currentState.Enter();
    }    
}