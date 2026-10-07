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

internal sealed class JournalMemoryEvent : BaseJournalEntryEvent
{
	private const float RotationSpeed = 1f;

	private const float SpirographRotationIncrement = 4f;

	private const float OscillationFrequency = (float)Math.PI;

	private const float OscillationAmplitude = 3f;

	private const float BackGlowColorMultiplier = 0.035f;

	private const float FrontGlowColorMultiplier = 0.05f;

	private static readonly Vector4 GradientColor1 = new Vector4(0.4f, 0.4f, 0.3f, 1f);

	private static readonly Vector4 GradientColor2 = new Vector4(0.3f, 0.3f, 0.6f, 1f);

	private readonly Point _basePosition;

	private readonly GlowTexture _backGlowTexture;

	private readonly GlowTexture _frontGlowTexture;

	private readonly PassiveBuffSparkleParticleSystem _sparkleParticles;

	private int _drawTicks;

	private float _baseRotation;

	private float _spirographTimer;

	private float _gradientTranslation;

	private float _oscillationDelta;

	public JournalMemoryEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpItems;
		ChangeAnimation(255);
		DrawOrigin = new Vector2(8f, 8f);
		_basePosition = Position;
		_isAffectedByTime = false;
		base.MoneyGiven = 250;
		_backGlowTexture = new GlowTexture(_level)
		{
			GlowColorMultiplier = 0.035f
		};
		_frontGlowTexture = new GlowTexture(_level)
		{
			GlowCircleRadius = 13,
			GlowColorMultiplier = 0.05f
		};
		_sparkleParticles = new PassiveBuffSparkleParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_backGlowTexture.BaseColor = Color.Purple;
		_frontGlowTexture.BaseColor = Color.Teal;
		_sparkleParticles.BaseColor = Color.Purple.ToVector4();
	}

	public override void Update(float delta)
	{
		UpdateSpirograph(delta);
		_oscillationDelta += delta * (float)Math.PI;
		if (_oscillationDelta > (float)Math.PI * 2f)
		{
			_oscillationDelta -= (float)Math.PI * 2f;
		}
		int num = (int)Math.Ceiling(Math.Sin(_oscillationDelta) * 3.0);
		Position = new Point(_basePosition.X, _basePosition.Y + num);
		Point center = Bbox.Center;
		_backGlowTexture.Center = center;
		_frontGlowTexture.Center = center;
		if (!base.IsFading)
		{
			_sparkleParticles.AddParticles(center.ToVector2());
		}
		else
		{
			float num2 = 1f - base.FadePercentage;
			_backGlowTexture.GlowColorMultiplier = 0.035f * num2;
			_frontGlowTexture.GlowColorMultiplier = 0.05f * num2;
		}
		_backGlowTexture.Update(delta);
		_frontGlowTexture.Update(delta);
		_sparkleParticles.Update(delta);
		base.Update(delta);
	}

	private void UpdateSpirograph(float delta)
	{
		_baseRotation += delta * 1f;
		_spirographTimer += delta;
		if (_spirographTimer > (float)Math.PI * 2f)
		{
			_spirographTimer -= (float)Math.PI * 2f;
		}
		_gradientTranslation = ((float)Math.Sin(_spirographTimer) + 1f) / 2f;
		_drawTicks = 16;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_backGlowTexture.Draw(spriteBatch);
		DrawSpirograph(spriteBatch);
		_frontGlowTexture.Draw(spriteBatch);
		_sparkleParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
	}

	private void DrawSpirograph(SpriteBatch spriteBatch)
	{
		if (!base.IsFading)
		{
			spriteBatch.End();
			_level.GCM.EfSlidingGradient.Parameters["colorOne"].SetValue(GradientColor1);
			_level.GCM.EfSlidingGradient.Parameters["colorTwo"].SetValue(GradientColor2);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSlidingGradient);
			base.DrawColor = new Color(_gradientTranslation, 0f, 0f, 0f);
		}
		else
		{
			base.DrawColor = Color.Purple * 0.5f * (1f - base.FadePercentage);
		}
		for (int i = 0; i < _drawTicks; i++)
		{
			base.Rotation = _baseRotation + (float)i * 4f;
			base.Draw(spriteBatch);
		}
		if (!base.IsFading)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
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
			DrawColor = new Color(0.6f, 0.5f, 0.7f, 1f),
			DoesFadeOut = true
		});
		bool flag = true;
		Dictionary<int, InventoryJournal> inventory = _level.GameSave.Inventory.JournalCollection.Inventory;
		for (int i = 0; i <= 10; i++)
		{
			if (inventory.ContainsKey(i))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			DoFirstMemoryCutscene();
			result = false;
		}
		return result;
	}

	private void DoFirstMemoryCutscene()
	{
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.TogglePlayerIsInvulnerable(isInvulnerable: true);
		AddDialogue("cs_mem_lun_00");
		AddDialogue("cs_mem_lun_01");
		AddDialogue("cs_mem_lun_02");
		AddDialogue("cs_mem_lun_03");
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
}
