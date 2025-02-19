using Godot;

namespace Game.Scripts;

public partial class Global : Node
{
    private readonly StringName groupParticles = "Particles";
    private readonly StringName groupTimers    = "Timers";

    private static Global instance;

    public override void _EnterTree() 
    {
        instance = this;
    }

    public static void SpawnParticle(Vector2 position, PackedScene packedScene, bool flip)
    {
        AnimatedSprite2D particle = packedScene.Instantiate<AnimatedSprite2D>();
        particle.GlobalPosition = position;
        particle.FlipH = flip;

        instance.GetTree().GetFirstNodeInGroup("Particles").AddChild(particle);        
    }
}