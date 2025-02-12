using Godot;

namespace Game.Scripts;

public partial class Global : Node
{
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