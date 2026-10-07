using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public abstract class ScreenEffect
{
	public bool IsFinished { get; set; }

	public Color EffectColor { get; set; }

	public abstract void Update(float delta);

	public abstract void Draw(SpriteBatch spriteBatch, Texture2D blankTexture, Rectangle drawRect);
}
