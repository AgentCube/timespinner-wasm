using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Sandman;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;

internal sealed class EnvPrefabTempleWall : EnvironmentPrefabBase
{
	private const int TileCount = 13;

	private const int TileWidth = 22;

	private const int TileHeight = 16;

	private const int WallHeight = 208;

	private const float TimeToFade = 1f;

	private const int Anim_TileStart = 23;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandDrawHelper _sandDrawHelper;

	private bool _isDrawingSand;

	private bool _isFading;

	private bool _isFadingOut;

	private float _fadeTimer;

	public EnvPrefabTempleWall(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		Bbox = new Rectangle(0, 0, 22, 208);
		ChangeAnimation(-1);
		_sprite = _level.GCM.SpSandmanBoss;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = false;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		IsFacingLeft = objectSpec.IsFlippedHorizontally;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		base.DrawPlane = EDrawPlane.Front;
		_sandDrawHelper = new SandDrawHelper(this);
		Point bboxDimensions = new Point(22, 16);
		int num = 0;
		for (int i = 0; i < 13; i++)
		{
			Appendage appendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(0, num)
			};
			appendage.ChangeAnimation(23 + i % 3);
			num -= 16;
			base.Appendages.Add(appendage);
		}
		_isFading = true;
		_isFadingOut = false;
		base.DrawColor = Color.Transparent;
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

	internal void FadeOut()
	{
		_isFading = true;
		_isFadingOut = true;
		_fadeTimer = 0f;
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
