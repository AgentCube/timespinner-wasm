using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabComputer : EnvironmentPrefabBase
{
	private readonly GlowTexture _glowTexture;

	public EnvPrefabLabComputer(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 16, 5);
		Position = Position.Add(0, -2);
		_glowTexture = new GlowTexture(_level)
		{
			GlowSpriteSheet = _level.GCM.SpMiscLab,
			FrameIndex = 6,
			GlowCircleCount = 6,
			Center = Position,
			GlowCircleWidth = 32,
			GlowCircleHeight = 16
		};
		_sprite = _level.GCM.SpMiscLab;
		ChangeAnimation(6);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_glowTexture.Update(delta);
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		_glowTexture.Draw(spriteBatch);
	}
}
