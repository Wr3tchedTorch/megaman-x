using Godot;
using System;

namespace Game.States;

public abstract partial class State : Node
{	
	public abstract void Enter();
	public abstract void Exit();
	public abstract void Update(float delta);
	public abstract void PhysicsUpdate(float delta);
}
