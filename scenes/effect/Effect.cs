using Godot;

public partial class Effect : AnimatedSprite2D
{
	public override void _Ready()
	{
		Play();
		AnimationFinished += () => { QueueFree(); };
	}
}
