using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal class BreakLevelEffect : LevelEffect
{
	private const int ScreenWidth = 400;

	private const int ScreenHeight = 240;

	private const int HalfScreenWidth = 200;

	private const int HalfScreenHeight = 120;

	private const int ParticlesEmittedPerLine = 8;

	private const int ParticleEmissionOffsetY = 3;

	private const int DarkLineCount = 3;

	private const int GlassWidth = 256;

	private const int GlassOffsetX = 72;

	private const float DarkLineAlphaMaximum = 0.8f;

	private const float DarkLineAlphaIncrement = -0.2f;

	private const float ScreenFlashTime = 0.2f;

	private const float TimeForGlassToGlow = 0.1f;

	private const float TimeForDisintegration = 8f;

	private const float TimeForFinalWait = 4f;

	private const float TimeBeforeShowingGlass = 0.1f;

	private const float TimeBeforeDisintegration = 0.2f;

	private const float TimeBeforeFinalWait = 8.2f;

	internal const float TimeForEntireBreakSequence = 12.2f;

	private const float GlassFinalColorMultiplier = 0.2f;

	private readonly BreakEffectParticleSystem _breakParticles;

	private readonly BreakFastEffectParticleSystem _breakFastParticles;

	private readonly SpriteSheet _brokenGlassSprite;

	private bool _isDrawingGlass;

	private bool _isFinished;

	private int _lastHeight;

	private float _sequenceTimer;

	private Color _glassDrawColor;

	private Rectangle _blackRectangle;

	public BreakLevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.DrawPlane = EDrawPlane.Front;
		_brokenGlassSprite = _level.GCM.SpBrokenGlass;
		_lastHeight = -1;
		_breakParticles = new BreakEffectParticleSystem(_level.GCM.TxBlankSquare, 512);
		_breakFastParticles = new BreakFastEffectParticleSystem(_level.GCM.TxBlankSquare, 128);
		Reset();
	}

	private void Reset()
	{
		_sequenceTimer = 0f;
		_breakParticles.KillOffParticles(0f);
		_breakFastParticles.KillOffParticles(0f);
		_isDrawingGlass = false;
		_glassDrawColor = Color.Transparent;
		_blackRectangle = Rectangle.Empty;
		_level.ForceFreezeEvents();
		_level.FreezeTime(ETeamSide.Heroes, reFreeze: true);
		_level.IsUsingGrayscaleEffect = false;
		_level.IsUIRequestingHide = true;
		_level.JukeBox.PlayCue(ESFX.CsRealityShatter);
	}

	public override void Update(float delta)
	{
		_level.IsUIRequestingHide = true;
		_level.IsPreventingPauseMenuUsage = true;
		if (!base.IsFrozen)
		{
			_level.FreezeTime(ETeamSide.Heroes, reFreeze: true);
			_level.ForceFreezeEvents();
		}
		_isFrozen = false;
		UpdateBreakSequence(delta);
		_breakParticles.Update(delta);
		_breakFastParticles.Update(delta);
		base.Update(delta);
		_isFrozen = true;
	}

	private void UpdateBreakSequence(float delta)
	{
		float sequenceTimer = _sequenceTimer;
		_sequenceTimer += delta;
		if (sequenceTimer <= 0f)
		{
			_level.RequestScreenFlash(new ScreenFlash(0.2f)
			{
				Frequency = 1f
			});
		}
		if (_sequenceTimer < 0.2f)
		{
			if (_sequenceTimer >= 0.1f && sequenceTimer < 0.1f)
			{
				_isDrawingGlass = true;
				_glassDrawColor = Color.White * 0.2f;
			}
		}
		else if (_sequenceTimer < 8.2f)
		{
			double num = 1.0 - Math.Cos((float)Math.PI / 2f * (_sequenceTimer - 0.2f) / 8f);
			int num2 = (int)Math.Round(num * 240.0);
			_blackRectangle = new Rectangle(0, 0, 400, num2);
			if (_lastHeight < num2)
			{
				EmitParticles();
			}
			_lastHeight = num2;
		}
		else if (_sequenceTimer < 12.2f && !_isFinished)
		{
			_isFinished = true;
			CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Alt3_Teleport, _level, Point.Zero);
		}
	}

	private void EmitParticles()
	{
		int num = _blackRectangle.Height + 3;
		for (int i = 0; i < 8; i++)
		{
			int num2 = _level.NextRandomInt(0, 400);
			_breakParticles.AddParticles(new Vector2(num2, num));
			num2 = _level.NextRandomInt(0, 400);
			_breakFastParticles.AddParticles(new Vector2(num2, num + 1));
			num2 = _level.NextRandomInt(0, 400);
			_breakFastParticles.AddParticles(new Vector2(num2, num + 2));
			num2 = _level.NextRandomInt(0, 400);
			_breakFastParticles.AddParticles(new Vector2(num2, num + 3));
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isDrawingGlass)
		{
			spriteBatch.Draw(_brokenGlassSprite.Texture, new Vector2((int)(_level.LevelRenderCenter.X - 200f + 72f), (int)(_level.LevelRenderCenter.Y - 120f)), null, _glassDrawColor);
			spriteBatch.Draw(_brokenGlassSprite.Texture, new Vector2((int)(_level.LevelRenderCenter.X - 200f + 256f + 72f), (int)(_level.LevelRenderCenter.Y - 120f)), null, _glassDrawColor, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(_brokenGlassSprite.Texture, new Vector2((int)(_level.LevelRenderCenter.X - 200f - 256f + 72f), (int)(_level.LevelRenderCenter.Y - 120f)), null, _glassDrawColor, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
		}
		if (_blackRectangle != Rectangle.Empty)
		{
			spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - 200f), (int)(_level.LevelRenderCenter.Y - 120f), _blackRectangle.Width, _blackRectangle.Height), null, Color.Black);
			for (int i = 0; i < 3; i++)
			{
				spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - 200f), (int)(_level.LevelRenderCenter.Y - 120f + (float)_blackRectangle.Height + (float)i), _blackRectangle.Width, 1), null, Color.Black * (0.8f + -0.2f * (float)i));
			}
		}
		_breakParticles.Draw(spriteBatch, _level.LevelRenderCenter, new Vector2(200f, 120f), _level.CameraZoom);
		_breakFastParticles.Draw(spriteBatch, _level.LevelRenderCenter, new Vector2(200f, 120f), _level.CameraZoom);
		base.Draw(spriteBatch);
	}
}
