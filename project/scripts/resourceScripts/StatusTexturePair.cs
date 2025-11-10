using Godot;
using System;

[GlobalClass]
public partial class StatusTexturePair : Resource
{
	[Export] private Texture2D texture;

	[Export] private StatusComponent.STATUS status;

	public Texture2D GetTexture() { return texture; }
	public StatusComponent.STATUS GetStatus() { return this.status; }
}
