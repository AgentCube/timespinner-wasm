using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs;

internal sealed class NPCQuestIndicator : Appendage
{
	internal enum EQuestIndicatorIcon
	{
		None,
		NewQuest,
		FinishQuest,
		RingShop,
		ItemShop,
		OrbShop
	}

	private const int Anim_FrameIndex = 43;

	private const int Anim_IconsStartIndex = 44;

	private const float TimeToFade = 0.25f;

	private readonly Appendage _stateIcon;

	private bool _isHidden;

	private bool _isFading;

	private bool _hasBeenSet;

	private float _floatingTimer;

	private float _fadeTimer;

	private EQuestIndicatorIcon _icon;

	internal bool IsAwaitingNewIcon { get; set; }

	internal bool HasBeenInitialized { get; set; }

	internal EQuestIndicatorIcon Icon => _icon;

	public NPCQuestIndicator(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		base.AnchorObject = parent;
		base.AnchorOffset = new Point(0, -64);
		base.FollowType = EAppendageFollowType.AnchorLocked;
		base.DoesInheritDrawColor = false;
		base.DoesDrawAura = true;
		base.AuraColor = Color.PaleGoldenrod;
		base.AuraSize = 0.1f;
		base.AuraOffset = new Vector2(1f, 1f);
		_icon = EQuestIndicatorIcon.None;
		_isHidden = true;
		_isFading = false;
		base.DrawColor = Color.Transparent;
		_stateIcon = new Appendage(this, new Point(7, 7), Point.Zero, _level, _sprite)
		{
			DrawPriority = 1,
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, -3)
		};
		_stateIcon.ChangeAnimation(44);
		base.Appendages.Add(_stateIcon);
		ChangeAnimation(43);
		IsAwaitingNewIcon = true;
	}

	internal void ChangeQuestState(EQuestIndicatorIcon icon)
	{
		if (_icon != icon)
		{
			bool flag = icon == EQuestIndicatorIcon.None;
			if (_isHidden != flag)
			{
				if (!_hasBeenSet && !flag)
				{
					base.DrawColor = Color.White;
				}
				else
				{
					_isFading = true;
					_fadeTimer = 0f;
				}
			}
			_isHidden = flag;
			_icon = icon;
			if (!_isHidden)
			{
				_stateIcon.ChangeAnimation((int)(44 + _icon - 1));
			}
			else
			{
				_stateIcon.ChangeAnimation(-1);
			}
		}
		else if (icon == EQuestIndicatorIcon.None && !_isHidden)
		{
			_isHidden = true;
			_isFading = false;
			base.DrawColor = Color.Transparent;
		}
		_hasBeenSet = true;
	}

	public override void Update(float delta)
	{
		UpdateFloating(delta);
		UpdateFading(delta);
		base.DoesDrawAura = !_isHidden;
		base.Update(delta);
	}

	private void UpdateFloating(float delta)
	{
		int num = -50;
		float num2 = 4f;
		float num3 = 3f;
		_floatingTimer += delta * num2;
		if (_floatingTimer >= (float)Math.PI * 2f)
		{
			_floatingTimer -= (float)Math.PI * 2f;
		}
		int num4 = (int)Math.Ceiling(Math.Sin(_floatingTimer) * (double)num3);
		int height = base.AnchorObject.Bbox.Height;
		base.AnchorOffset = new Point(1, num + num4 - height);
	}

	private void UpdateFading(float delta)
	{
		if (_isFading)
		{
			_fadeTimer += delta;
			float amount = 1f;
			if (_fadeTimer >= 0.25f)
			{
				_isFading = false;
			}
			else
			{
				amount = _fadeTimer / 0.25f;
			}
			base.DrawColor = (_isHidden ? Color.White : Color.Transparent).SineInterpolate(_isHidden ? Color.Transparent : Color.White, amount);
		}
	}
}
