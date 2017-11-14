using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanUtility
	{
		public static bool IsOwner(Pawn pawn, Faction caravanFaction)
		{
			if (caravanFaction == null)
			{
				Log.Warning("Called IsOwner with null faction.");
				return false;
			}
			return !pawn.NonHumanlikeOrWildMan() && pawn.Faction == caravanFaction && pawn.HostFaction == null;
		}

		public static Caravan GetCaravan(this Pawn pawn)
		{
			return pawn.ParentHolder as Caravan;
		}

		public static bool IsCaravanMember(this Pawn pawn)
		{
			return pawn.GetCaravan() != null;
		}

		public static bool IsPlayerControlledCaravanMember(this Pawn pawn)
		{
			Caravan caravan = pawn.GetCaravan();
			return caravan != null && caravan.IsPlayerControlled;
		}

		public static int BestGotoDestNear(int tile, Caravan c)
		{
			Predicate<int> predicate = delegate(int t)
			{
				if (Find.World.Impassable(t))
				{
					return false;
				}
				if (!c.CanReach(t))
				{
					return false;
				}
				return true;
			};
			if (predicate(tile))
			{
				return tile;
			}
			int result = default(int);
			GenWorldClosest.TryFindClosestTile(tile, predicate, out result, 50, true);
			return result;
		}

		public static bool PlayerHasAnyCaravan()
		{
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (caravans[i].IsPlayerControlled)
				{
					return true;
				}
			}
			return false;
		}

		public static Pawn RandomOwner(this Caravan caravan)
		{
			return (from p in caravan.PawnsListForReading
			where caravan.IsOwner(p)
			select p).RandomElement();
		}
	}
}
