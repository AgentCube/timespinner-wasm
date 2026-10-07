using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal class EnvPrefabTemplePlatform : EnvironmentPrefabBase
{
	private const int Anim_Start = 19;

	private const float TrailOffsetTime = 1f / 60f;

	private const float ColorChangeFrequency = 2f;

	private const float AlphaChangeA = 0.1f;

	private const float AlphaChangeB = 0.25f;

	private static readonly Color ColorChangeColorA = new Color(0.3f, 0.25f, 0.3f, 1f);

	private static readonly Color ColorChangeColorB = new Color(0.25f, 0.3f, 0.25f, 1f);

	private readonly Point _trailOffset;

	private float _colorChangeTimer;

	private float _shiftTrailTimer;

	public EnvPrefabTemplePlatform(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpNightmareBoss;
		ChangeAnimation(19);
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
		_doesCollideWithTiles = false;
		_isSolid = false;
		_doesDrawTrail = true;
		_trailLength = 6;
		_trailFadeRate = 1f;
		_trailOffset = new Point(-3, 0);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_colorChangeTimer += delta * 2f;
			if (_colorChangeTimer >= (float)Math.PI * 2f)
			{
				_colorChangeTimer -= (float)Math.PI * 2f;
			}
			float amount = (float)(Math.Sin(_colorChangeTimer) + 1.0) * 0.5f;
			float amount2 = (float)(Math.Sin(_colorChangeTimer * 2f) + 1.0) * 0.5f;
			float num = MathHelper.Lerp(0.1f, 0.25f, amount2);
			Color drawColor = ColorChangeColorA.Lerp(ColorChangeColorB, amount);
			drawColor.A = (byte)(255f * num);
			base.DrawColor = drawColor;
			_shiftTrailTimer += delta;
			if (_shiftTrailTimer >= 1f / 60f)
			{
				_shiftTrailTimer -= 1f / 60f;
				ShiftTrailHistory(_trailOffset);
			}
		}
		base.Update(delta);
	}
}
