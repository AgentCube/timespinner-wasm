using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Treasure;

internal sealed class JournalLetterEvent : BaseJournalEntryEvent
{
	private const float OscillationFrequency = (float)Math.PI;

	private const float OscillationAmplitude = 3f;

	private const float GlowTextureColorMultiplier = 0.035f;

	private readonly Point _basePosition;

	private readonly GlowTexture _glowTexture;

	private readonly PassiveBuffSparkleParticleSystem _sparkleParticles;

	private float _oscillationDelta;

	public JournalLetterEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpMenuIcons;
		_basePosition = Position;
		ChangeAnimation((base.JournalType == EInventoryJournalType.Letter10) ? 30 : 29);
		_isAffectedByTime = true;
		base.MoneyGiven = 250;
		_glowTexture = new GlowTexture(_level)
		{
			GlowColorMultiplier = 0.035f
		};
		_sparkleParticles = new PassiveBuffSparkleParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleSystems.Add(_sparkleParticles);
		_glowTexture.BaseColor = Color.MistyRose;
		_sparkleParticles.BaseColor = Color.MistyRose.ToVector4();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_oscillationDelta += delta * (float)Math.PI;
			if (_oscillationDelta > (float)Math.PI * 2f)
			{
				_oscillationDelta -= (float)Math.PI * 2f;
			}
			int num = (int)Math.Ceiling(Math.Sin(_oscillationDelta) * 3.0);
			Position = new Point(_basePosition.X, _basePosition.Y + num);
			_glowTexture.Center = Bbox.Center;
			_glowTexture.Update(delta);
			if (!base.IsFading)
			{
				_sparkleParticles.AddParticles(Bbox.Center.ToVector2());
			}
			else
			{
				float num2 = 1f - (float)Math.Sin(base.FadePercentage * ((float)Math.PI / 4f));
				base.DrawColor = Color.Transparent;
				_glowTexture.GlowColorMultiplier = 0.035f * num2;
			}
		}
		base.Update(delta);
	}

	internal override bool OnGetJournalEntry()
	{
		bool result = true;
		_level.AddAnimation(new BattleAnimation(_level.GCM.SpItems, Bbox.Center, _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationSpeed = 0.04f,
			AnimationStart = 261,
			AnimationLength = 5,
			DoesFadeOut = true
		});
		bool flag = true;
		Dictionary<int, InventoryJournal> inventory = _level.GameSave.Inventory.JournalCollection.Inventory;
		for (int i = 32; i <= 42; i++)
		{
			if (inventory.ContainsKey(i))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			DoFirstLetterCutscene();
			result = false;
		}
		return result;
	}

	private void DoFirstLetterCutscene()
	{
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.TogglePlayerIsInvulnerable(isInvulnerable: true);
		AddDialogue("cs_let_lun_00");
		AddDialogue("cs_let_lun_01");
		AddLevelScriptAction(new ScriptAction(base.JournalType, 1));
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = MakePlayerNotInvulnerable
		});
	}

	private void MakePlayerNotInvulnerable()
	{
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_glowTexture.Draw(spriteBatch);
		base.Draw(spriteBatch);
	}
}
