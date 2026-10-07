using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.StatusEffects;

internal class PoisonStatusEffect : BaseStatusEffect
{
	private const float TimeBetweenParticleEmission = 0.1f;

	private const float TimeBetweenDamageTics = 1f;

	private const float MaxPoisonTime = 30f;

	private const float DamagePercentagePerTic = 0.01f;

	private readonly PoisonBubblesParticleSystem _poisonBubbles;

	private readonly StatusEffectSparkleParticleSystem _poisonSparklesSystem;

	private float _particleTimer;

	private float _damageTicTimer;

	public PoisonStatusEffect(Level inLevel, Alive host, bool isPlayer)
		: base(inLevel, host, EStatusEffectType.Poison, isPlayer)
	{
		_poisonBubbles = new PoisonBubblesParticleSystem(base.Sprite, 5, 1, 0);
		_poisonSparklesSystem = new StatusEffectSparkleParticleSystem(base.Sprite, 5, 0, 0)
		{
			BaseColor = new Vector4(0.6f, 0.3f, 0.6f, 1f)
		};
		base.DoesShowStatusText = true;
		base.TextColor1 = new Color(200, 180, 200);
		base.TextColor2 = new Color(112, 56, 112);
	}

	internal override float GetMaxTime()
	{
		return 30f;
	}

	internal override void Update(float delta)
	{
		if (!base.IsFadingOut)
		{
			_particleTimer += delta;
			if (_particleTimer >= 0.1f)
			{
				_particleTimer -= 0.1f;
				_poisonBubbles.AddParticles(base.Host.Bbox);
				_poisonSparklesSystem.AddParticles(base.Host.Bbox);
			}
			_damageTicTimer += delta;
			if (_damageTicTimer >= 1f)
			{
				_damageTicTimer -= 1f;
				int damage = (int)Math.Ceiling((float)base.Host.MaxHP * 0.01f);
				base.Host.ManageSubtleDamage(damage, isLethal: false, EElementalWeaknessState.None);
			}
		}
		_poisonBubbles.Update(delta);
		_poisonSparklesSystem.Update(delta);
		base.Update(delta);
	}

	internal override void Draw(SpriteBatch spriteBatch)
	{
		_poisonBubbles.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
		_poisonSparklesSystem.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
		base.Draw(spriteBatch);
	}

	internal override void ChangeRoom()
	{
		_poisonBubbles.KillOffParticles(0f);
		_poisonSparklesSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}
}
