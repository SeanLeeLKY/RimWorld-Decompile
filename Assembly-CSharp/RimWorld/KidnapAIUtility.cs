using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class KidnapAIUtility
	{
		public static bool TryFindGoodKidnapVictim(Pawn kidnapper, float maxDist, out Pawn victim, List<Thing> disallowed = null)
		{
			if (kidnapper.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && kidnapper.Map.reachability.CanReachMapEdge(kidnapper.Position, TraverseParms.For(kidnapper, Danger.Some, TraverseMode.ByPawn, false)))
			{
				Predicate<Thing> validator = delegate(Thing t)
				{
					Pawn pawn = t as Pawn;
					if (!pawn.RaceProps.Humanlike)
					{
						return false;
					}
					if (!pawn.Downed)
					{
						return false;
					}
					if (pawn.Faction != Faction.OfPlayer)
					{
						return false;
					}
					if (!pawn.Faction.HostileTo(kidnapper.Faction))
					{
						return false;
					}
					if (!kidnapper.CanReserve(pawn, 1, -1, null, false))
					{
						return false;
					}
					if (disallowed != null && disallowed.Contains(pawn))
					{
						return false;
					}
					return true;
				};
				victim = (Pawn)GenClosest.ClosestThingReachable(kidnapper.Position, kidnapper.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some, false), maxDist, validator, null, 0, -1, false, RegionType.Set_Passable, false);
				return victim != null;
			}
			victim = null;
			return false;
		}

		public static Pawn ReachableWoundedGuest(Pawn searcher)
		{
			List<Pawn> list = searcher.Map.mapPawns.SpawnedPawnsInFaction(searcher.Faction);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.guest != null && !pawn.IsPrisoner && pawn.Downed && searcher.CanReserveAndReach(pawn, PathEndMode.OnCell, Danger.Some, 1, -1, null, false))
				{
					return pawn;
				}
			}
			return null;
		}
	}
}
