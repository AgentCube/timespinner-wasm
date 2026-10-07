using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal sealed class WindSpellAppendage : Appendage
{
	private const float IndexTimerOffset = 0.1f;

	private const float MaxSize = 1.5f;

	private const float TimeToExpand = 0.5f;

	private float _expansionTimer;

	internal float FadeMultiplier { get; set; }

	public WindSpellAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, int index)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		ChangeAnimation(16);
		_expansionTimer -= (float)index * 0.1f;
		DrawOrigin = new Vector2(32f, 32f);
		base.AnchorOffset = new Point(-24, -16);
		base.DoesInheritDrawColor = false;
		FadeMultiplier = 1f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_expansionTimer += delta;
			if (_expansionTimer >= 0.5f)
			{
				_expansionTimer -= 0.5f;
			}
			if (_expansionTimer <= 0f)
			{
				base.DrawColor = Color.Transparent;
			}
			else
			{
				float num = _expansionTimer / 0.5f;
				float num2 = (float)Math.Sin(num * (float)Math.PI);
				base.DrawColor = Color.White * num2;
				base.Scale = 1.5f * (float)Math.Sin(num * ((float)Math.PI / 2f));
				base.Rotation = num * ((float)Math.PI * 2f);
			}
			if (FadeMultiplier < 1f)
			{
				base.DrawColor *= FadeMultiplier;
			}
		}
		base.Update(delta);
	}
}
