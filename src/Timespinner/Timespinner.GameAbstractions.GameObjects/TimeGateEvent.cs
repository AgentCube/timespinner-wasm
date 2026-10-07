using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameAbstractions.GameObjects;

public sealed class TimeGateEvent : GameEvent
{
	public enum EGateScriptType
	{
		OpenAndSpitLunais,
		Closed,
		OpenAndCycle,
		OpenAndStayOpen
	}

	private const int TriggerSize = 80;

	private const int FlipFrameOffsetY = 46;

	private const int FlashMaxDiameter = 160;

	private const int SpirographTicks = 40;

	private const float SpirographRotationIncrement = 4f;

	private const float TimeForGrowth = 0.4f;

	private const float TimeForSustain = 1.2f;

	private const float TimeForGateAnimation = 1.6f;

	private const float PortalAnimationSpeed = 0.066f;

	private const float TimeForPortalToStayOpened = 0.75f;

	private const float TimeForPortalToClose = 0.396f;

	private const float TimeForPortalToStartClosing = 2.3500001f;

	private const float TimeForEntireAnimation = 2.746f;

	private const float TimeForSparklesToEmit = 1.7320001f;

	private const float TimeForFlashToEmit = 1.6f;

	private const float TimeForFlashToLast = 0.15f;

	private const float RotationSpeed = 1f;

	private const float GradientTranslationRate = 3f;

	private const float TimeBetweenLevelCycles = 1f;

	private static readonly Vector4 GradientColor1 = new Vector4(0.4f, 0.4f, 0.3f, 1f);

	private static readonly Vector4 GradientColor2 = new Vector4(0.3f, 0.3f, 0.6f, 1f);

	private static readonly List<int> CycleLevelIDs = new List<int> { 8, 3, 1 };

	private readonly bool _isOneWayWarp;

	private readonly Point _sparkAnimationPoint;

	private readonly Vector2 _particleEmissionPoint;

	private readonly Appendage _portalAppendage;

	private readonly Texture2D _flashTexture;

	private readonly TimeGateLeakParticleSystem _pixelLeakParticleSystem;

	private bool _hasUpdatedOnce;

	private bool _isDrawingFlash;

	private bool _isDrawingPortal;

	private bool _isDrawingBrilliantGate = true;

	private bool _isFinished;

	private bool _doesStayOpen;

	private int _drawTicks;

	private int _cycleLevelIndex;

	private float _percentage;

	private float _gradientTranslation;

	private float _gateAnimationTimer = -1f;

	private float _baseRotation;

	private float _lastGateAnimationTimer;

	private float _flashDiameter;

	private float _flashTimer;

	private float _cycleTimer;

	private Color _flashColor;

	private IEnumerable<BackgroundSpecification> _warpBackgrounds;

	private readonly bool _doesMirrorRoomBackgrounds;

	private float _mirrorTimer;

	public bool IsActive { get; set; }

	internal bool CanPlayerUseTimeGate => _level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.TimespinnerSpindle);

	internal bool IsCameraLockedAfterJump { get; set; }

	internal int TargetLevelID { get; set; }

	public EGateScriptType GateScriptType { get; set; }

	internal CutsceneBase.ECutsceneType CutsceneToCall { get; set; }

	public Point WarpInPoint { get; private set; }

	public TimeGateEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition.Add(new Point(30, -38)), inID, objectSpec)
	{
		_sprite = _level.GCM.SpTimeGateAnimation;
		_doesPersist = true;
		ChangeAnimation(0);
		DrawOrigin = Vector2.Zero;
		_portalAppendage = new Appendage(this, new Rectangle(0, 0, 1, 1), Point.Zero, _level, _sprite)
		{
			DrawPriority = 1,
			FollowType = EAppendageFollowType.ParentObjectLocked,
			AnchorOffset = new Point(-84, -45),
			AuraColor = Color.White * 0.9f,
			AuraOffset = new Vector2(1f, 1f),
			AuraSize = 0.015f,
			AuraFrequency = 8f,
			DoesDrawAura = true
		};
		if (objectSpec != null && objectSpec.Argument == 1)
		{
			_isOneWayWarp = false;
			Position = Position.Add(0, 14);
			SnapBboxToPosition();
		}
		else if (objectSpec != null && objectSpec.Argument == -1)
		{
			_doesStayOpen = true;
			_doesMirrorRoomBackgrounds = true;
			IsActive = true;
			GateScriptType = EGateScriptType.OpenAndStayOpen;
		}
		else
		{
			_isOneWayWarp = true;
			if (_level.ID == 1 && _level.RoomID == 0)
			{
				TargetLevelID = 15;
			}
		}
		WarpInPoint = new Point(Position.X - 38, Position.Y - 4);
		_pixelLeakParticleSystem = new TimeGateLeakParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleEmissionPoint = WarpInPoint.ToVector2();
		_sparkAnimationPoint = new Point(WarpInPoint.X, WarpInPoint.Y + 4);
		_flashTexture = _level.GCM.TxLargeCircle;
	}

	public override void Update(float delta)
	{
		if (!_hasUpdatedOnce)
		{
			_hasUpdatedOnce = true;
			if (GateScriptType == EGateScriptType.OpenAndSpitLunais && IsActive)
			{
				_level.IsUIRequestingHide = true;
			}
		}
		if (IsActive && !_isFinished)
		{
			if (GateScriptType != EGateScriptType.Closed)
			{
				_level.IsPreventingPauseMenuUsage = true;
				UpdateGate(delta);
				UpdatePortal(delta);
				UpdateFlash(delta);
				if (GateScriptType == EGateScriptType.OpenAndCycle)
				{
					UpdateCycle(delta);
				}
			}
			base.Update(delta);
			_lastGateAnimationTimer = _gateAnimationTimer;
		}
		else if (!_isOneWayWarp)
		{
			base.TriggerBbox = new Rectangle(WarpInPoint.X - 40, WarpInPoint.Y + 12, 80, 80);
		}
		_pixelLeakParticleSystem.Update(delta);
		if (_doesMirrorRoomBackgrounds)
		{
			_mirrorTimer += delta;
			if (_mirrorTimer >= 1f)
			{
				_mirrorTimer -= 1f;
				_warpBackgrounds = _level.LevelSpecification.GetBackgroundsForRoom(_level.RoomID);
			}
		}
	}

	private void UpdateGate(float delta)
	{
		_gateAnimationTimer += delta;
		if (_gateAnimationTimer > 1000f)
		{
			_gateAnimationTimer = 1000f;
		}
		if (_gateAnimationTimer >= 0f && _lastGateAnimationTimer <= 0f)
		{
			_level.AddAnimation(new BattleAnimation(_sprite, _sparkAnimationPoint, _level)
			{
				TeamSide = ETeamSide.Heroes,
				IsFacingLeft = true,
				AnimationSpeed = 0.035f,
				AnimationStart = 11,
				AnimationLength = 3
			});
			_level.PlayCue(ESFX.LunaisTimeGateWarpin, _particleEmissionPoint.ToPoint());
		}
		if (_gateAnimationTimer > 2.746f && !_doesStayOpen)
		{
			_isFinished = true;
			_gateAnimationTimer = -1f;
			_isDrawingBrilliantGate = true;
		}
		_percentage = _gateAnimationTimer / 1.6f;
		_drawTicks = (int)((float)Math.PI * 80f * Math.Min(_percentage, 1f));
		_scale = ((_gateAnimationTimer > 0.4f) ? 1f : ((float)Math.Sin(_gateAnimationTimer / 0.4f * ((float)Math.PI / 2f))));
		_baseRotation += delta * 1f;
		_gradientTranslation = _percentage * 3f;
		_gradientTranslation -= (int)Math.Floor(_gradientTranslation);
	}

	private void UpdatePortal(float delta)
	{
		_isDrawingPortal = _gateAnimationTimer >= 1.6f;
		if (_isDrawingPortal)
		{
			if (_lastGateAnimationTimer < 1.6f)
			{
				_portalAppendage.ChangeAnimation(1, 4, 0.066f, EAnimationType.Once);
				if (GateScriptType == EGateScriptType.OpenAndSpitLunais)
				{
					float duration = 2f;
					Vector4 args;
					if (CutsceneToCall != 0)
					{
						if (CutsceneToCall != CutsceneBase.ECutsceneType.Forest0_Warp)
						{
							args = ((CutsceneToCall == CutsceneBase.ECutsceneType.LakeDesolation0_Warp) ? new Vector4(WarpInPoint.X, WarpInPoint.Y, 0f, 1f) : ((CutsceneToCall != CutsceneBase.ECutsceneType.Alt0_Nuvius && CutsceneToCall != CutsceneBase.ECutsceneType.Alt1_Vol) ? new Vector4(WarpInPoint.X, WarpInPoint.Y, 0f, 0f) : new Vector4(WarpInPoint.X, WarpInPoint.Y, 1f, 0f)));
						}
						else
						{
							args = new Vector4(WarpInPoint.X, WarpInPoint.Y, 1f, 0f);
							duration = 5f;
						}
					}
					else
					{
						args = new Vector4(WarpInPoint.X, WarpInPoint.Y, IsCameraLockedAfterJump ? 1 : 0, 0f);
					}
					_level.InsertScript(new ScriptAction(EScriptActionType.GateJump, 0f, duration, args)
					{
						TargetType = EScriptTargetType.Player1
					});
					if (CutsceneToCall == CutsceneBase.ECutsceneType.None)
					{
						AddDelegateScript(delegate
						{
							_level.IsUIRequestingHide = false;
						});
					}
					if (CutsceneToCall != 0)
					{
						CutsceneBase.CreateAndCallCutscene(CutsceneToCall, _level, WarpInPoint);
					}
				}
			}
			else if (_gateAnimationTimer >= 2.3500001f && !_doesStayOpen)
			{
				if (_lastGateAnimationTimer < 2.3500001f)
				{
					_portalAppendage.ChangeAnimation(5, 6, 0.066f, EAnimationType.Once);
					_isDrawingBrilliantGate = false;
				}
			}
			else if (_gateAnimationTimer >= 1.7320001f)
			{
				_pixelLeakParticleSystem.AddParticles(_particleEmissionPoint);
			}
			_portalAppendage.Update(delta);
		}
		if (_warpBackgrounds == null)
		{
			ChangeTargetLevel(TargetLevelID);
		}
	}

	private void UpdateFlash(float delta)
	{
		if (_gateAnimationTimer >= 1.6f && _lastGateAnimationTimer < 1.6f)
		{
			_isDrawingFlash = true;
			_flashTimer = 0f;
		}
		if (_isDrawingFlash)
		{
			_flashTimer += delta;
			float num = _flashTimer / 0.15f;
			if (num > 1f)
			{
				_isDrawingFlash = false;
				return;
			}
			float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
			_flashDiameter = num2 * 160f;
			_flashColor = Color.White * (1f - num2);
		}
	}

	private void UpdateCycle(float delta)
	{
		if (!_isDrawingPortal)
		{
			return;
		}
		_cycleTimer += delta;
		if (_cycleTimer > 1f)
		{
			_cycleTimer = 0f;
			_cycleLevelIndex++;
			if (_cycleLevelIndex >= CycleLevelIDs.Count)
			{
				_cycleLevelIndex = 0;
			}
			ChangeTargetLevel(CycleLevelIDs[_cycleLevelIndex]);
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if ((!IsActive || _isFinished) && !_isOneWayWarp && CanPlayerUseTimeGate)
		{
			Protagonist protagonist = who as Protagonist;
			_isTriggered = true;
			_level.RequestButtonPrompt(4, WarpInPoint);
			if (protagonist != null && protagonist.CheckButton(4, isNewPressOnly: true) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				return false;
			}
		}
		return base.TriggerEvent(who, depth);
	}

	internal void ChangeTargetLevel(int levelID)
	{
		if (levelID != TargetLevelID && _warpBackgrounds != null)
		{
			_isDrawingFlash = true;
			_flashTimer = 0f;
		}
		_warpBackgrounds = _level.GetWarpBackgroundsForLevelID(levelID);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (IsActive && !_isFinished)
		{
			DrawBrilliantGate(spriteBatch);
			DrawPortal(spriteBatch);
			DrawFlash(spriteBatch);
		}
		_pixelLeakParticleSystem.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		if (base.DoesDrawTriggerBbox)
		{
			base.Draw(spriteBatch);
		}
	}

	private void DrawBrilliantGate(SpriteBatch spriteBatch)
	{
		if (_isDrawingBrilliantGate)
		{
			spriteBatch.End();
			_level.GCM.EfSlidingGradient.Parameters["colorOne"].SetValue(GradientColor1);
			_level.GCM.EfSlidingGradient.Parameters["colorTwo"].SetValue(GradientColor2);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSlidingGradient);
			base.DrawColor = new Color(_gradientTranslation, 0f, 0f, 0f);
			for (int i = 0; i < _drawTicks; i++)
			{
				base.Rotation = _baseRotation + (float)i * 4f;
				base.Draw(spriteBatch);
			}
		}
	}

	private void DrawPortal(SpriteBatch spriteBatch)
	{
		if (!_isDrawingPortal)
		{
			return;
		}
		_level.GCM.EfDrawUnder.Parameters["shinyAmount"].SetValue(1f);
		_level.GCM.EfDrawUnder.Parameters["underAlpha"].SetValue(1f);
		Rectangle frameSource = _portalAppendage.GetFrameSource();
		Vector4 value = new Vector4((float)frameSource.X / (float)_sprite.Texture.Width, (float)frameSource.Y / (float)_sprite.Texture.Height, (float)frameSource.Width / (float)_sprite.Texture.Width, (float)frameSource.Height / (float)_sprite.Texture.Height);
		_level.GCM.EfDrawUnder.Parameters["FrameSource"].SetValue(value);
		for (int i = 0; i < 3; i++)
		{
			if (i == 0)
			{
				spriteBatch.End();
				_level.GCM.EfSlidingGradient.Parameters["colorOne"].SetValue(GradientColor1);
				_level.GCM.EfSlidingGradient.Parameters["colorTwo"].SetValue(GradientColor2);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSlidingGradient);
				_portalAppendage.SpriteEffects = SpriteEffects.None;
				_portalAppendage.Draw(spriteBatch);
				_portalAppendage.SpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
				_portalAppendage.DrawPosition = _portalAppendage.DrawPosition.Add(new Point(0, 46));
				_portalAppendage.Draw(spriteBatch);
				_portalAppendage.DrawPosition = _portalAppendage.DrawPosition.Add(new Point(0, -46));
				continue;
			}
			_portalAppendage.DoesDrawAura = false;
			if (i == 1)
			{
				_portalAppendage.SpriteEffects = SpriteEffects.None;
				_level.GCM.EfDrawUnder.Parameters["IsFlippedVertically"].SetValue(0f);
				_level.GCM.EfDrawUnder.Parameters["IsFlippedHorizontally"].SetValue(0f);
				_level.GCM.EfDrawUnder.Parameters["UnderFrameOffset"].SetValue(Vector2.Zero);
			}
			else
			{
				_portalAppendage.SpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
				_portalAppendage.DrawPosition = _portalAppendage.DrawPosition.Add(new Point(0, 45));
				_level.GCM.EfDrawUnder.Parameters["IsFlippedVertically"].SetValue(1f);
				_level.GCM.EfDrawUnder.Parameters["IsFlippedHorizontally"].SetValue(1f);
				_level.GCM.EfDrawUnder.Parameters["UnderFrameOffset"].SetValue(new Vector2(0f, value.W));
			}
			foreach (BackgroundSpecification warpBackground in _warpBackgrounds)
			{
				SpriteSheet spriteFromType = warpBackground.GetSpriteFromType(_level.GCM);
				Texture2D texture = spriteFromType.Texture;
				Rectangle frameSource2 = spriteFromType.GetFrameSource(warpBackground.FrameIndex);
				Vector2 value2 = Vector2.Zero;
				if (warpBackground.PanRatio.X != 0f && warpBackground.PanRatio.Y != 0f)
				{
					value2 = new Vector2((float)warpBackground.StartPoint.X / (float)texture.Width, (float)(32 + warpBackground.StartPoint.Y) / (float)texture.Height);
				}
				Vector4 value3 = new Vector4((float)frameSource2.X / (float)texture.Width, (float)frameSource2.Y / (float)texture.Height, (float)frameSource2.Width / (float)texture.Width, (float)frameSource2.Height / (float)texture.Height);
				_portalAppendage.DrawColor = new Color(1f, 1f, 1f, 1f);
				spriteBatch.End();
				spriteBatch.GraphicsDevice.Textures[1] = texture;
				_level.GCM.EfDrawUnder.Parameters["UnderFrameSource"].SetValue(value3);
				_level.GCM.EfDrawUnder.Parameters["UnderOffset"].SetValue(value2);
				_level.GCM.EfDrawUnder.Parameters["DimensionRatio"].SetValue(new Vector2((float)_sprite.Texture.Width / (float)texture.Width, (float)_sprite.Texture.Height / (float)texture.Height));
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, _level.GCM.EfDrawUnder);
				_portalAppendage.Draw(spriteBatch);
			}
		}
		_portalAppendage.DoesDrawAura = true;
		_portalAppendage.DrawPosition = _portalAppendage.DrawPosition.Add(new Point(0, -45));
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}

	private void DrawFlash(SpriteBatch spriteBatch)
	{
		if (_isDrawingFlash)
		{
			Vector2 value = new Vector2(_particleEmissionPoint.X - _flashDiameter / 2f, _particleEmissionPoint.Y - _flashDiameter / 2f);
			Vector2 vector = Vector2.Subtract(_level.LevelRenderCenter, Vector2.Subtract(_level.CameraPosition, value));
			spriteBatch.Draw(destinationRectangle: new Rectangle((int)vector.X, (int)vector.Y, (int)_flashDiameter, (int)_flashDiameter), texture: _flashTexture, color: _flashColor);
		}
	}

	internal void OpenAndStayOpen()
	{
		IsActive = true;
		GateScriptType = EGateScriptType.OpenAndStayOpen;
		_doesStayOpen = true;
	}

	internal void CloseGate()
	{
		_doesStayOpen = false;
		_lastGateAnimationTimer = 2.25f;
		_gateAnimationTimer = 2.3500001f;
	}
}
