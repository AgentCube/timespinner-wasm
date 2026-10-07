using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Cantoran;

internal class CantoranBarrierDamageArea : DamageArea
{
	private readonly float _maxLife;

	public CantoranBarrierDamageArea(Level inLevel, Point inPosition, int baseDamage, float maxLife, SpriteSheet sprite, Mobile anchor)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, anchor)
	{
		_sprite = sprite;
		_doesDrawSpriteAndAppendages = false;
		base.AnchorOffset = new Point(0, -32);
		_power = baseDamage;
		_force = 4;
		_maxLife = maxLife;
		_life = maxLife;
		base.DamageTimeoutTime = 0.5f;
		_damageElement = EDamageElement.Light;
	}

	internal void SetDimensions(int width, int height)
	{
		Bbox = new Rectangle(Position.X, Position.Y, width + 2, height + 2);
	}

	internal void Reset(bool shouldAdd)
	{
		if (shouldAdd)
		{
			base.ID = -1;
		}
		_life = _maxLife;
		_isFading = false;
		_fadeTimer = 0f;
	}
}
