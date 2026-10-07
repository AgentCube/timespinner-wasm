using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal class MusicFaderEvent : GameEvent
{
	private readonly float _musicVolume;

	public MusicFaderEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_musicVolume = ((objectSpec == null) ? 1f : ((float)objectSpec.Argument / 100f));
		_level.JukeBox.AdjustMusicVolume(_musicVolume);
		base.DoesDrawBaseSprite = false;
		base.CanBeTriggered = false;
		Bbox = new Rectangle(0, 0, 16, 16);
		base.EventType = EEventTileType.MusicFader;
	}
}
