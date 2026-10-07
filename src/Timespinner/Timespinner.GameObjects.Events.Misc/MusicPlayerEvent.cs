using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal class MusicPlayerEvent : GameEvent
{
	private readonly EBGM _songToPlay;

	public MusicPlayerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_songToPlay = (EBGM)(objectSpec?.Argument ?? 99);
		if (_songToPlay == EBGM.LevelBGM)
		{
			_songToPlay = Level.GetLevelSong(_level.ID, _level.RoomID, _level.GameSave);
		}
		if (_songToPlay != EBGM.Default)
		{
			Jukebox jukeBox = _level.JukeBox;
			if (jukeBox.CurrentSongEnum == _songToPlay)
			{
				jukeBox.FadeInSong(0.5f);
			}
			else
			{
				jukeBox.PlaySong(_songToPlay, shouldForceRestart: true, shouldImmediatelyStopPreviousSong: false);
			}
		}
		base.DoesDrawBaseSprite = false;
		base.CanBeTriggered = false;
		Bbox = new Rectangle(0, 0, 16, 16);
		base.EventType = EEventTileType.MusicPlayer;
	}
}
