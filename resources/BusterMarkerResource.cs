using Godot;

namespace Game.Resources;

[GlobalClass]
public partial class BusterMarkerResource : Resource
{
	[Export] public Vector2 Idle = new Vector2(15, -18);
	[Export] public Vector2 Land = new Vector2(15, -18);
	[Export] public Vector2 Run  = new Vector2(18, -21);
	[Export] public Vector2 Jump = new Vector2(13, -30);
	[Export] public Vector2 Fall = new Vector2(15, -28);
	[Export] public Vector2 Dash = new Vector2(24, -14);
	[Export] public Vector2 None = new Vector2(24, -14);
}
