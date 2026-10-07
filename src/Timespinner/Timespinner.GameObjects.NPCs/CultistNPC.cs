using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class CultistNPC : NPCBase
{
	private const int DialogTickerCount = 3;

	private readonly bool _isPriest;

	private int _dialogTicker;

	public CultistNPC(Level inLevel, Point inPosition, int inID, TileSpecification tileSpec, bool isPriest)
		: base(inLevel, inPosition, inLevel.GCM.SpMerchantCrow, inID)
	{
		_isPriest = isPriest;
		if (_isPriest)
		{
			ChangeAnimation(22, 4, 0.25f, EAnimationType.Cycle);
			_npcType = ENPCType.CultistPriest;
			base.TriggerBbox = new Rectangle(0, 0, 24, 64);
		}
		else
		{
			ChangeAnimation(26, 4, 0.25f, EAnimationType.Cycle);
			_npcType = ENPCType.CultistWorshipper;
			base.TriggerBbox = new Rectangle(0, 0, 24, 64);
			base.TriggerBboxOffset = new Point(8, 0);
		}
		_bboxOffset = new Point(3, 1);
		_bbox = new Rectangle(0, 0, 16, 38);
		IsFacingLeft = !tileSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_npcTriggerType = ENPCTriggerType.Talk;
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		if (_isPriest)
		{
			if (base.PrimaryProgress < 1)
			{
				MovePlayerToTalkingPosition();
				AddDialogue("cs_cult_cul_00");
				AddDialogue("cs_cult_lun_01");
				AddDialogue("cs_cult_cul_02");
				AddDialogue("cs_cult_cul_03");
				SetPrimaryProgress(1);
			}
			else
			{
				AddDialogue("cs_cult_cul_00a");
			}
		}
		else
		{
			switch (_dialogTicker)
			{
			case 0:
				AddDialogue("cs_cultw_cul_00");
				break;
			case 1:
				AddDialogue("cs_cultw_cul_01");
				break;
			case 2:
				AddDialogue("cs_cultw_cul_02");
				break;
			}
			_dialogTicker = (_dialogTicker + 1) % 3;
		}
		EndNPCDialogue();
	}
}
