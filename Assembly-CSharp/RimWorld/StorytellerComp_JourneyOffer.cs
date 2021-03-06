using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_JourneyOffer : StorytellerComp
	{
		private const int StartOnDay = 14;

		private int IntervalsPassed
		{
			get
			{
				return Find.TickManager.TicksGame / 1000;
			}
		}

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (this.IntervalsPassed != 840)
				yield break;
			IncidentDef inc = IncidentDefOf.JourneyOffer;
			if (!inc.TargetAllowed(target))
				yield break;
			FiringIncident fi = new FiringIncident(inc, this, this.GenerateParms(inc.category, target));
			yield return fi;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
