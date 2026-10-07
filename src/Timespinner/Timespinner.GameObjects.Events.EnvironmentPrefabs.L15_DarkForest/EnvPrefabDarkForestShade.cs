using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Bosses.Sandman;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L15_DarkForest;

internal sealed class EnvPrefabDarkForestShade : EnvironmentPrefabBase
{
	private const float TimeToFade = 1f;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly SandDrawHelper _sandDrawHelper;

	private bool _isFading;

	private bool _isFadingOut;

	private bool _isDrawingSand;

	private float _fadeTimer;

	public EnvPrefabDarkForestShade(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpSandmanBoss;
		_idleSequence = GetCharacterSequenceByName("Idle");
		SetCharacterSequence(_idleSequence);
		_sandDrawHelper = new SandDrawHelper(this);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		ChangeAnimation(-1);
		IsFacingLeft = true;
		_isFading = true;
		_isFadingOut = false;
		base.DrawColor = Color.Transparent;
	}

	internal void FadeOut()
	{
		_isFading = true;
		_isFadingOut = true;
		_fadeTimer = 0f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_sandDrawHelper.Update(delta);
			if (_isFading)
			{
				_fadeTimer += delta;
				float num = 1f;
				if (_fadeTimer >= 1f)
				{
					_isFading = false;
				}
				else
				{
					num = _fadeTimer / 1f;
				}
				if (_isFadingOut)
				{
					num = 1f - num;
				}
				base.DrawColor = Color.White * num;
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
