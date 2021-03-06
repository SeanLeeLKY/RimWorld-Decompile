using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
	{
		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer) && (desperate || (float)GenDate.DaysPassed >= f.def.earliestRaidDays);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!base.TryExecuteWorker(parms))
			{
				return false;
			}
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			Find.StoryWatcher.statsRecord.numRaidsEnemy++;
			return true;
		}

		protected override bool TryResolveRaidFaction(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.faction != null)
			{
				return true;
			}
			float num = parms.points;
			if (num <= 0.0)
			{
				num = 999999f;
			}
			if (!PawnGroupMakerUtility.TryGetRandomFactionForNormalPawnGroup(num, out parms.faction, (Predicate<Faction>)((Faction f) => this.FactionCanBeGroupSource(f, map, false)), true, true, true, true) && !PawnGroupMakerUtility.TryGetRandomFactionForNormalPawnGroup(num, out parms.faction, (Predicate<Faction>)((Faction f) => this.FactionCanBeGroupSource(f, map, true)), true, true, true, true))
			{
				return false;
			}
			return true;
		}

		protected override void ResolveRaidPoints(IncidentParms parms)
		{
			if (parms.points <= 0.0)
			{
				Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
				parms.points = (float)Rand.Range(50, 300);
			}
		}

		protected override void ResolveRaidStrategy(IncidentParms parms)
		{
			if (parms.raidStrategy == null)
			{
				Map map = (Map)parms.target;
				parms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs
				where d.Worker.CanUseWith(parms)
				select d).RandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionChance(map));
			}
		}

		protected override string GetLetterLabel(IncidentParms parms)
		{
			return parms.raidStrategy.letterLabelEnemy;
		}

		protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
		{
			string str = null;
			switch (parms.raidArrivalMode)
			{
			case PawnsArriveMode.EdgeWalkIn:
				str = "EnemyRaidWalkIn".Translate(parms.faction.def.pawnsPlural, parms.faction.Name);
				break;
			case PawnsArriveMode.EdgeDrop:
				str = "EnemyRaidEdgeDrop".Translate(parms.faction.def.pawnsPlural, parms.faction.Name);
				break;
			case PawnsArriveMode.CenterDrop:
				str = "EnemyRaidCenterDrop".Translate(parms.faction.def.pawnsPlural, parms.faction.Name);
				break;
			}
			str += "\n\n";
			str += parms.raidStrategy.arrivalTextEnemy;
			Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
			if (pawn != null)
			{
				str += "\n\n";
				str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort);
			}
			return str;
		}

		protected override LetterDef GetLetterDef()
		{
			return LetterDefOf.ThreatBig;
		}

		protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
		{
			return "LetterRelatedPawnsRaidEnemy".Translate(parms.faction.def.pawnsPlural);
		}
	}
}
