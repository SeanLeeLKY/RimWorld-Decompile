using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Tradeable_Pawn : Tradeable
	{
		public override Window NewInfoDialog
		{
			get
			{
				return new Dialog_InfoCard(this.AnyPawn);
			}
		}

		public override string Label
		{
			get
			{
				string text = base.Label;
				if (this.AnyPawn.Name != null && !this.AnyPawn.Name.Numerical)
				{
					text = text + ", " + this.AnyPawn.def.label;
				}
				string text2 = text;
				return text2 + " (" + this.AnyPawn.gender.GetLabel() + ", " + this.AnyPawn.ageTracker.AgeBiologicalYearsFloat.ToString("F0") + ")";
			}
		}

		public override string TipDescription
		{
			get
			{
				if (!this.HasAnyThing)
				{
					return string.Empty;
				}
				string str = this.AnyPawn.MainDesc(true);
				return str + "\n\n" + this.AnyPawn.def.description;
			}
		}

		private Pawn AnyPawn
		{
			get
			{
				return (Pawn)this.AnyThing;
			}
		}

		public override void ResolveTrade()
		{
			if (base.ActionToDo == TradeAction.PlayerSells)
			{
				List<Pawn> list = base.thingsColony.Take(-base.CountToTransfer).Cast<Pawn>().ToList();
				for (int i = 0; i < list.Count; i++)
				{
					TradeSession.trader.GiveSoldThingToTrader(list[i], 1, TradeSession.playerNegotiator);
				}
			}
			else if (base.ActionToDo == TradeAction.PlayerBuys)
			{
				List<Pawn> list2 = base.thingsTrader.Take(base.CountToTransfer).Cast<Pawn>().ToList();
				for (int j = 0; j < list2.Count; j++)
				{
					TradeSession.trader.GiveSoldThingToPlayer(list2[j], 1, TradeSession.playerNegotiator);
				}
			}
		}
	}
}
