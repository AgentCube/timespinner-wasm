using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabTube : EnvironmentPrefabBase
{
	private const int BboxWidth = 48;

	private const int BboxHeight = 88;

	private const int BubbleTopOffsetY = -7;

	private const int BubbleEmissionOffsetX = -5;

	private const int BubbleEmissionOffsetY = -9;

	private const int MaxBubblesNormal = 3;

	private const int MaxBubblesEnding = 32;

	private const int DeathBubblesOffsetX = 12;

	private const float TimeToDie = 5f;

	private const float TimeBetweenDeathBubbles = 0.1f;

	private const float MinBubbleEmissionTime = 2f;

	private const float MaxBubbleEmissionTime = 10f;

	private const float BubbleEmissionTimeRange = 8f;

	private static readonly Color BaseScreenColor = new Color(200, 150, 60, 255);

	private static readonly Color BaseScreenDeathColor = new Color(200, 32, 64, 255);

	private readonly bool _doesTubeGlow;

	private readonly bool _doesEmitBubbles;

	private readonly GlowTexture _glowTexture;

	private readonly GlowTexture _screenGlowTexture;

	private readonly UnderwaterBubbleParticleSystem _bubbleParticles;

	private bool _isDying;

	private float _deathTimer;

	private float _deathBubbleTimer;

	private float _bubbleEmissionTimer;

	private SFXCueInstance _testLoopInstance;

	public EnvPrefabLabTube(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		base.DrawPlane = EDrawPlane.Back;
		Bbox = new Rectangle(0, 0, 48, 88);
		_doesDrawBaseSprite = false;
		SnapBboxToPosition();
		_sprite = _level.GCM.SpMiscLab;
		_doesTubeGlow = prefabType == EEnvironmentPrefabType.L11_TubeLargeEmpty || prefabType == EEnvironmentPrefabType.L11_TubeLargeChild || prefabType == EEnvironmentPrefabType.L11_TubeLargeAdult || prefabType == EEnvironmentPrefabType.L11_TubeLargeAdolescent || prefabType == EEnvironmentPrefabType.L17_TubeLargeChild;
		_doAppendagesInheritDrawColor = false;
		_doesEmitBubbles = prefabType == EEnvironmentPrefabType.L11_TubeLargeChild || prefabType == EEnvironmentPrefabType.L11_TubeLargeAdolescent || prefabType == EEnvironmentPrefabType.L17_TubeLargeChild;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 0)
		{
			SetCharacterSequence(base.CharacterSpecification.Sequences[0]);
		}
		if (_doesTubeGlow)
		{
			_glowTexture = new GlowTexture(_level)
			{
				GlowSpriteSheet = _sprite,
				FrameIndex = 7,
				GlowCircleWidth = 64,
				GlowCircleHeight = 128,
				Center = Bbox.Center,
				BaseColor = Color.Teal
			};
			_screenGlowTexture = new GlowTexture(_level)
			{
				GlowSpriteSheet = _level.GCM.SpMiscLab,
				FrameIndex = 37,
				GlowCircleCount = 1,
				Center = new Point(Position.X, Position.Y - 1),
				GlowCircleWidth = 18,
				GlowCircleHeight = 8,
				GlowColorMultiplier = 0.5f,
				BaseColor = BaseScreenColor
			};
		}
		if (_doesEmitBubbles)
		{
			_doesDrawParticleSystemsUnder = true;
			_bubbleParticles = new UnderwaterBubbleParticleSystem(_level.GCM.TxParticleEnergy, (prefabType == EEnvironmentPrefabType.L17_TubeLargeChild) ? 32 : 3);
			if (base.Appendages.Count > 1)
			{
				base.Appendages[1].AddParticleSystem(_bubbleParticles);
			}
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_doesTubeGlow)
			{
				_glowTexture.Update(delta);
				_screenGlowTexture.Update(delta);
				if (_testLoopInstance == null)
				{
					_testLoopInstance = CreateCue(ESFX.AmbientTestTubeLoop, Position, isLooped: true);
					if (_testLoopInstance != null)
					{
						_testLoopInstance.PlayWhenInRange();
					}
				}
			}
			if (_doesEmitBubbles)
			{
				_bubbleEmissionTimer -= delta;
				if (_bubbleEmissionTimer <= 0f)
				{
					_bubbleParticles.AddParticles(Bbox.Center.Add(-5, -9).ToVector2());
					_bubbleEmissionTimer = (float)(2.0 + _level.NextRandomDouble() * 8.0);
					_bubbleParticles.WaterTopY = Bbox.Top + -7;
				}
			}
			if (_isDying && _deathTimer < 5f)
			{
				_deathTimer += delta;
				_deathBubbleTimer -= delta;
				if (_deathBubbleTimer <= 0f)
				{
					_deathBubbleTimer += 0.1f;
					int num = _level.NextRandomInt(-12, 12);
					_bubbleParticles.AddParticles(new Vector2(Position.X + num, Position.Y));
				}
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_doesTubeGlow)
		{
			_glowTexture.Draw(spriteBatch);
			_screenGlowTexture.Draw(spriteBatch);
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification.IntArgument == 1)
		{
			_isDying = true;
			_deathTimer = 0f;
			_screenGlowTexture.BaseColor = BaseScreenDeathColor;
			if (_testLoopInstance != null)
			{
				_testLoopInstance.Stop(5f);
			}
		}
	}
}
