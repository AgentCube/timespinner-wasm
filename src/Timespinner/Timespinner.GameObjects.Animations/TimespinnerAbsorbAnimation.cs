using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Animations;

internal class TimespinnerAbsorbAnimation : BattleAnimation
{
	private const int MaxRadius = 24;

	private const float AuraBrushEmissionRate = 0.1f;

	private const float MaxEmissionTime = 3f;

	private const float AnimationStartTime = 3.95f;

	private readonly float _animationSequenceLength;

	private readonly Point _originPoint;

	private readonly Mobile _lunais;

	private readonly Vector2 _origin;

	private readonly SFXCueInstance _chargeCueInstance;

	private readonly List<AuraPaintBrush> _auraBrushes = new List<AuraPaintBrush>();

	private bool _hasUnleashedAnimation;

	private bool _hasPlayedStartingEffect;

	private float _life;

	private float _auraBrushEmissionCounter;

	public TimespinnerAbsorbAnimation(Level inLevel, Point origin, Mobile lunais, float length)
		: base(inLevel.GCM.SpEffectsLarge, origin, inLevel)
	{
		_originPoint = origin;
		_origin = origin.ToVector2();
		_lunais = lunais;
		_animationSequenceLength = length;
		base.AnimationStart = 0;
		base.AnimationLength = 4;
		base.AnimationSpeed = 0.066f;
		base.DrawColor = Color.White * 0.8f;
		base.ParticleSystem = new TimespinnerAbsorbParticleSystem(inLevel.GCM.TxParticleEnergy, 5, _lunais);
		_chargeCueInstance = base.Level.CreateCue(ESFX.LunaisChargeLoop, origin, isLooped: true);
	}

	internal void End()
	{
		_life = _animationSequenceLength;
		if (_chargeCueInstance != null && _hasPlayedStartingEffect && !_chargeCueInstance.IsFinished)
		{
			_chargeCueInstance.Stop(0.15f);
		}
	}

	public override void Update(float delta)
	{
		if (!_hasPlayedStartingEffect && _life < _animationSequenceLength)
		{
			_hasPlayedStartingEffect = true;
			base.Level.PlayCue(ESFX.LunaisChargeFlashMedium, _originPoint, isLooped: false);
			if (_chargeCueInstance != null)
			{
				_chargeCueInstance.Play();
			}
		}
		_life += delta;
		if (_life < _animationSequenceLength)
		{
			base.ParticleSystem.AddParticles(_origin);
			_auraBrushEmissionCounter += delta;
			if (_auraBrushEmissionCounter > 0.1f && _life < 3f)
			{
				_auraBrushEmissionCounter = 0f;
				EmitAuraBrush();
			}
			else if (_life > 3.95f && !_hasUnleashedAnimation)
			{
				base.AnimationIndex = 0;
				_isDoneAnimating = false;
				_hasUnleashedAnimation = true;
				if (_chargeCueInstance != null)
				{
					_chargeCueInstance.Stop();
				}
				base.Level.PlayCue(ESFX.LunaisChargeFlashLarge, _originPoint, isLooped: false);
			}
		}
		base.Update(delta);
		for (int num = _auraBrushes.Count - 1; num >= 0; num--)
		{
			AuraPaintBrush auraPaintBrush = _auraBrushes[num];
			auraPaintBrush.Update(delta);
			if (auraPaintBrush.IsDead)
			{
				_auraBrushes.RemoveAt(num);
			}
		}
	}

	private void EmitAuraBrush()
	{
		Vector2 vector = new Vector2(24f * (float)Math.Sin(6.2831854820251465 * base.Level.NextRandomDouble()), 24f * (float)Math.Sin(6.2831854820251465 * base.Level.NextRandomDouble()));
		Point center = _lunais.Bbox.Center;
		AuraPaintBrush item = new AuraPaintBrush(_originPoint.Add(vector), vector, center, base.Level, 0);
		_auraBrushes.Add(item);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		foreach (AuraPaintBrush auraBrush in _auraBrushes)
		{
			auraBrush.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}
}
