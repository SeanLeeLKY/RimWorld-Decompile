using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_ImportantTraderCaravanPeopleLost : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost && (signal.condition == PawnLostCondition.IncappedOrKilled || signal.condition == PawnLostCondition.MadePrisoner))
			{
				TraderCaravanRole traderCaravanRole = signal.Pawn.GetTraderCaravanRole();
				if (traderCaravanRole != TraderCaravanRole.Trader && traderCaravanRole != TraderCaravanRole.Carrier)
				{
					if (lord.numPawnsLostViolently > 0 && (float)lord.numPawnsLostViolently / (float)lord.numPawnsEverGained >= 0.5)
					{
						return true;
					}
					goto IL_006b;
				}
				return true;
			}
			goto IL_006b;
			IL_006b:
			return false;
		}
	}
}
