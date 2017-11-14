using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalState_SadisticRageTantrum : MentalState_TantrumRandom
	{
		private int hits;

		public const int MaxHits = 7;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.hits, "hits", 0, false);
		}

		protected override void GetPotentialTargets(List<Thing> outThings)
		{
			TantrumMentalStateUtility.GetSmashableThingsNear(base.pawn, base.pawn.Position, outThings, this.GetCustomValidator(), 0, 40);
		}

		protected override Predicate<Thing> GetCustomValidator()
		{
			return (Thing x) => TantrumMentalStateUtility.CanAttackPrisoner(base.pawn, x);
		}

		public override void Notify_AttackedTarget(LocalTargetInfo hitTarget)
		{
			base.Notify_AttackedTarget(hitTarget);
			if (base.target != null && hitTarget.Thing == base.target)
			{
				this.hits++;
				if (this.hits >= 7)
				{
					base.RecoverFromState();
				}
			}
		}
	}
}
