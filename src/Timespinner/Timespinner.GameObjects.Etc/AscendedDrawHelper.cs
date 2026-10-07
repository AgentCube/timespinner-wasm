using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Ascended;

namespace Timespinner.GameObjects.Etc;

internal class AscendedDrawHelper
{
	private const int ShockwaveOffsetX = 4;

	private const int ShockwaveOffsetY = 4;

	private readonly bool _hasSparkles;

	private readonly int _sparkleTrailCount;

	private readonly GlowTexture _glowTexture;

	private readonly AscendedHalo _halo;

	private readonly AscendedSparkleTrail[] _sparkleTrails;

	private readonly Animate _parent;

	private readonly Level _level;

	private bool _isShowingShockwave;

	private Color _ascendedColor;

	private ShockwaveAnimation _shockwave;

	internal Point ParentCenter => _parent.Bbox.Center;

	internal AscendedDrawHelper(Animate parent, int sparkleTrailCount)
	{
		_parent = parent;
		_level = parent.Level;
		_sparkleTrailCount = sparkleTrailCount;
		Point parentCenter = ParentCenter;
		_halo = new AscendedHalo(parentCenter, _level, _level.GCM.SpOrbMeleeBarrier);
		if (_sparkleTrailCount > 0)
		{
			_hasSparkles = true;
			_sparkleTrails = new AscendedSparkleTrail[_sparkleTrailCount];
			float percentage = 1f / (float)_sparkleTrailCount;
			SpriteSheet spOrbMeleeBarrier = _level.GCM.SpOrbMeleeBarrier;
			for (int i = 0; i < _sparkleTrailCount; i++)
			{
				AscendedSparkleTrail ascendedSparkleTrail = new AscendedSparkleTrail(Point.Zero, _level, spOrbMeleeBarrier);
				_sparkleTrails[i] = ascendedSparkleTrail;
				ascendedSparkleTrail.Activate(parentCenter, i, percentage);
			}
		}
		_glowTexture = new GlowTexture(_level)
		{
			GlowCircleCount = 6,
			GlowCircleRadius = 64,
			GlowCircleConsecutiveSizeReduction = 0.95f,
			GlowColorMultiplier = 0.1f,
			GlowFrequency = 3f,
			GlowOffset = 1f,
			GlowAmplitude = 0.15f
		};
	}

	internal void SetGlowColor(Color color)
	{
		_ascendedColor = color;
		_glowTexture.BaseColor = color;
		_halo.SetDrawColor(color);
		if (_hasSparkles)
		{
			AscendedSparkleTrail[] sparkleTrails = _sparkleTrails;
			foreach (AscendedSparkleTrail ascendedSparkleTrail in sparkleTrails)
			{
				ascendedSparkleTrail.BaseDrawColor = color;
			}
		}
	}

	internal void Update(float delta)
	{
		Point parentCenter = ParentCenter;
		_halo.Position = parentCenter;
		_halo.Update(delta);
		if (_hasSparkles)
		{
			AscendedSparkleTrail[] sparkleTrails = _sparkleTrails;
			foreach (AscendedSparkleTrail ascendedSparkleTrail in sparkleTrails)
			{
				if (ascendedSparkleTrail.IsActive)
				{
					ascendedSparkleTrail.Origin = parentCenter;
					ascendedSparkleTrail.Update(delta);
				}
			}
		}
		_glowTexture.Center = parentCenter;
		_glowTexture.Update(delta);
		if (_isShowingShockwave && _shockwave != null)
		{
			_shockwave.Update(delta);
		}
	}

	internal void Draw(SpriteBatch spritebatch, bool isAbove)
	{
		if (_parent.DoesDrawWhenOutsideOfObjectVisibleArea || _parent.IsWithinObjectVisibleArea)
		{
			if (!isAbove)
			{
				_halo.Draw(spritebatch);
			}
			if (_hasSparkles)
			{
				AscendedSparkleTrail[] sparkleTrails = _sparkleTrails;
				foreach (AscendedSparkleTrail ascendedSparkleTrail in sparkleTrails)
				{
					if (ascendedSparkleTrail.IsActive)
					{
						ascendedSparkleTrail.Draw(spritebatch, isAbove);
					}
				}
			}
			_glowTexture.Draw(spritebatch);
		}
		if (isAbove && _isShowingShockwave && _shockwave != null && !_shockwave.IsDead)
		{
			_shockwave.Draw(spritebatch);
		}
	}

	internal void AddShockwave()
	{
		Color tintColor = _ascendedColor.Lerp(new Color(1f, 1f, 1f, 0.1f), 0.5f);
		_isShowingShockwave = true;
		Point inPosition = ParentCenter.Add(4, 4);
		_shockwave = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, inPosition, _level, tintColor);
	}

	internal void ScatterSparkles()
	{
		AscendedSparkleTrail[] sparkleTrails = _sparkleTrails;
		for (int i = 0; i < sparkleTrails.Length; i++)
		{
			sparkleTrails[i]?.StartScatter();
		}
	}
}
