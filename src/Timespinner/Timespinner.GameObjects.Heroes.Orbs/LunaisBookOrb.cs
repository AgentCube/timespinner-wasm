using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisBookOrb : LunaisOrb
{
	private const int MaxSwords = 3;

	private const int AnimIdleStart = 0;

	private const int AnimIdleLength = 18;

	private const int AnimOpenStart = 18;

	private const int AnimOpenLength = 6;

	private const int AnimReadStart = 24;

	private const int AnimReadLength = 2;

	private const float CooldownTime = 0.6f;

	private readonly BookOrbSwordAppendage[] _swords = new BookOrbSwordAppendage[3];

	private bool _isOpen;

	private SFXCueInstance _bookLoopCueInstance;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Book;

	public LunaisBookOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 2;
		_doesDrawBrushTrail = false;
		ClearTrailHistory();
		_bbox = new Rectangle(0, 0, 8, 8);
		_bboxOffset = new Point(4, 4);
		DrawOrigin = new Vector2(8f, 8f);
		ChangeAnimation(0, 18, 0.05f, EAnimationType.Cycle);
		for (int i = 0; i < 3; i++)
		{
			BookOrbSwordAppendage bookOrbSwordAppendage = new BookOrbSwordAppendage(this, new Point(1, 1), new Point(36, 6), _level, _sprite);
			_swords[i] = bookOrbSwordAppendage;
		}
		Update(0f);
	}

	public override void Update(float delta, Point currentTarget)
	{
		bool flag = true;
		BookOrbSwordAppendage[] swords = _swords;
		foreach (BookOrbSwordAppendage bookOrbSwordAppendage in swords)
		{
			if (!bookOrbSwordAppendage.IsAvailable)
			{
				flag = false;
			}
			if (!bookOrbSwordAppendage.IsFinished)
			{
				bookOrbSwordAppendage.CurrentTarget = currentTarget;
			}
			else if (!bookOrbSwordAppendage.IsAvailable)
			{
				base.Appendages.Remove(bookOrbSwordAppendage);
				bookOrbSwordAppendage.IsAvailable = true;
			}
		}
		if (flag && _isOpen)
		{
			CloseBook();
		}
		base.Update(delta, currentTarget);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		OpenBook();
	}

	private void OpenBook()
	{
		if (_isOpen)
		{
			return;
		}
		_isOpen = true;
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 18,
				Length = 6,
				Speed = 0.075f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 24,
				Length = 2,
				Speed = 0.03f,
				Type = EAnimationType.Cycle
			}
		});
		if (_bookLoopCueInstance == null)
		{
			_bookLoopCueInstance = CreateCue(ESFX.LunaisOrbBookLoop, Position, isLooped: true);
			if (_bookLoopCueInstance != null)
			{
				_bookLoopCueInstance.FadeIn(0.2f);
				_bookLoopCueInstance.Play();
			}
		}
		else if (_bookLoopCueInstance.IsPaused)
		{
			_bookLoopCueInstance.FadeIn(0.2f);
			_bookLoopCueInstance.Resume();
		}
	}

	private void CloseBook()
	{
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 18,
				Length = 6,
				Speed = 0.1f,
				Type = EAnimationType.Once,
				IsInReverse = true
			},
			new AnimationSpec
			{
				Start = 0,
				Length = 18,
				Speed = 0.05f,
				Type = EAnimationType.Cycle
			}
		});
		_isOpen = false;
		if (_bookLoopCueInstance != null && !_bookLoopCueInstance.IsFinished)
		{
			_bookLoopCueInstance.Pause(0.2f);
		}
	}

	private void DoSlash()
	{
		for (int i = 0; i < 3; i++)
		{
			BookOrbSwordAppendage bookOrbSwordAppendage = _swords[i];
			if (bookOrbSwordAppendage.IsAvailable)
			{
				bookOrbSwordAppendage.Reset(base.CurrentTarget, base.IsThrowingLeft, base.OrbDamage);
				bookOrbSwordAppendage.PlayCue(ESFX.LunaisOrbBookMelee);
				base.Appendages.Add(bookOrbSwordAppendage);
				break;
			}
		}
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		if (_currentThrowTime <= 0f)
		{
			base.IsAtAttackApex = true;
			DoSlash();
		}
		_currentThrowTime += delta;
		if (_currentThrowTime >= 0.6f)
		{
			bool flag = false;
			BookOrbSwordAppendage[] swords = _swords;
			foreach (BookOrbSwordAppendage bookOrbSwordAppendage in swords)
			{
				if (bookOrbSwordAppendage.IsAvailable)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				base.State = EOrbState.Idle;
				_trailColor = base.IdleTrailColor;
			}
		}
		Position = _baseOrbitPosition;
	}

	public override void DisposeOrb()
	{
		if (_bookLoopCueInstance != null)
		{
			_bookLoopCueInstance.Stop();
		}
		for (int i = 0; i < 3; i++)
		{
			BookOrbSwordAppendage bookOrbSwordAppendage = _swords[i];
			bookOrbSwordAppendage.DisposeOrb();
		}
		base.DisposeOrb();
	}
}
