using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleJustice : Monster
{
	private enum ETempleJusticeState
	{
		Idle,
		Attacking,
		Returning
	}

	private const int WallParticleEmissionOffsetX = 48;

	private const int AttackThresholdX = 32;

	private const int AttackThresholdY = 80;

	private const int AttackWidthX = 136;

	private const float IdleTimeBetweenCheckingPlayerPosition = 0.1f;

	private const float TimeToRam = 0.5f;

	private const float TimeToReturn = 1.5f;

	private const float TimeForDeath = 0.35f;

	private const float DeathTimeMultiplier = 0.5f;

	private static readonly Color BaseAuraColor = new Color(0.3f, 0.7f, 0.45f, 0.1f);

	private static readonly Color AscendedColor = new Color(0.3f, 0.7f, 0.45f, 0.25f);

	private readonly Point _spawnPoint;

	private readonly SFXCueInstance _ascendedLoopCueInstance;

	private readonly DustCrackingParticleSystem _dustParticles;

	private readonly PebblesParticleSystem _pebbleParticles;

	private readonly CharacterSequenceSpecification _rotateSequence;

	private readonly AscendedDrawHelper _ascendedDrawHelper;

	private bool _isPlayerToLeft;

	private ETempleJusticeState _justiceState;

	private float _justiceTimer;

	public TempleJustice(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_spawnPoint = inPosition;
		Bbox = new Rectangle(0, 0, 16, 16);
		ChangeAnimation(-1);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		_justiceState = ETempleJusticeState.Idle;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_isAlwaysAggroed = true;
		_ascendedDrawHelper = new AscendedDrawHelper(this, 5);
		_ascendedDrawHelper.SetGlowColor(AscendedColor);
		SetDoesDrawAppendageTrails(value: true, isHost: true, 6, 3f);
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(1f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		_rotateSequence = GetCharacterSequenceByName("Rotate");
		SetCharacterSequence(_rotateSequence);
		_dustParticles = new DustCrackingParticleSystem(_level.GCM.TxParticleDust, 3, _level.ID);
		_particleSystems.Add(_dustParticles);
		_pebbleParticles = new PebblesParticleSystem(_level.GCM.TxBlankSquare, 3, _level.ID);
		_particleSystems.Add(_pebbleParticles);
		_ascendedLoopCueInstance = CreateCue(ESFX.EnemyTempleAscendedLoop, Position, isLooped: true);
		if (_ascendedLoopCueInstance != null)
		{
			_ascendedLoopCueInstance.PlayWhenInRange();
		}
	}

	public override void Update(float delta)
	{
		if (_isFrozen)
		{
			_isFrozen = false;
		}
		if (!base.IsFrozen)
		{
			_ascendedDrawHelper.Update(delta);
		}
		base.Update(delta);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_justiceState)
		{
		case ETempleJusticeState.Idle:
			if (_justiceTimer >= 0.1f)
			{
				Point playerPosition = _level.GetPlayerPosition();
				int num3 = Math.Abs(Position.X - playerPosition.X);
				int num4 = Math.Abs(Position.Y - playerPosition.Y);
				if (num4 <= 80 && num3 >= 32)
				{
					_justiceState = ETempleJusticeState.Attacking;
					_isPlayerToLeft = Position.X >= playerPosition.X;
					PlayCue(ESFX.EnemyJusticeBashMove);
				}
				_justiceTimer = 0f;
			}
			break;
		case ETempleJusticeState.Attacking:
		{
			float num = _justiceTimer / 0.5f;
			if (num >= 1f)
			{
				_level.RequestScreenShake(new Vector2(5f, 0f), 0.4f, 12f, isAffectedByTime: false);
				_level.PlayCue(ESFX.EnemyJusticeBashImpact, Position);
				int num2 = (_isPlayerToLeft ? 48 : (_level.RoomSize.X - 48));
				Vector2 where = new Vector2(num2, Bbox.Center.Y);
				Vector2 where2 = new Vector2(num2, Bbox.Center.Y - 8);
				Vector2 where3 = new Vector2(num2, Bbox.Center.Y + 8);
				_dustParticles.AddParticles(where);
				_dustParticles.AddParticles(where2);
				_dustParticles.AddParticles(where3);
				_pebbleParticles.AddParticles(where);
				_pebbleParticles.AddParticles(where2);
				_pebbleParticles.AddParticles(where3);
				_justiceState = ETempleJusticeState.Returning;
				_justiceTimer = 0f;
			}
			else
			{
				num = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
				Position = new Point(_spawnPoint.X + ((!_isPlayerToLeft) ? 1 : (-1)) * (int)(136f * num), _spawnPoint.Y);
			}
			break;
		}
		case ETempleJusticeState.Returning:
		{
			float num = _justiceTimer / 1.5f;
			if (num >= 1f)
			{
				Position = _spawnPoint;
				_justiceState = ETempleJusticeState.Idle;
				_justiceTimer = 0f;
			}
			else
			{
				num = (float)Math.Sin(num * ((float)Math.PI / 2f));
				Position = new Point(_spawnPoint.X + ((!_isPlayerToLeft) ? 1 : (-1)) * (int)(136f * (1f - num)), _spawnPoint.Y);
			}
			break;
		}
		}
		_justiceTimer += delta;
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_nextActionTimer = 100f;
		_currentAction = EAIAction.Custom;
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			DropLoot();
			_ascendedDrawHelper.ScatterSparkles();
			_ascendedDrawHelper.AddShockwave();
			if (_ascendedLoopCueInstance != null)
			{
				_ascendedLoopCueInstance.Stop();
			}
			PlayCue(ESFX.EnemyTempleAscendedDeath);
		}
		if (_deathScriptTimer < 0.35f)
		{
			float num = (float)Math.Cos(_deathScriptTimer / 0.35f * ((float)Math.PI / 2f));
			base.DrawColor = Color.White * num;
			base.AuraColor = BaseAuraColor * num;
			_ascendedDrawHelper.SetGlowColor(AscendedColor * num);
			UpdateCharacterSequences(delta * 0.5f);
		}
		else
		{
			RemoveInstance();
		}
		_deathScriptTimer += delta;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_ascendedDrawHelper.Draw(spriteBatch, isAbove: false);
		base.Draw(spriteBatch);
		_ascendedDrawHelper.Draw(spriteBatch, isAbove: true);
	}
}
