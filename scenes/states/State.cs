using Godot;

namespace Game.States;

public abstract partial class State : Node
{	
	public CharacterBody2D owner;

	public virtual void Enter() {}
	public virtual void Exit()  {}
	public virtual void Update(float delta) {}
	public virtual void PhysicsUpdate(float delta) {}
}
