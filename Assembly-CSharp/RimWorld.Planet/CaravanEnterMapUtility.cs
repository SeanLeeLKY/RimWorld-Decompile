using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanEnterMapUtility
	{
		private static List<Pawn> tmpPawns = new List<Pawn>();

		public static void Enter(Caravan caravan, Map map, CaravanEnterMode enterMode, CaravanDropInventoryMode dropInventoryMode = CaravanDropInventoryMode.DoNotDrop, bool draftColonists = false, Predicate<IntVec3> extraCellValidator = null)
		{
			if (enterMode == CaravanEnterMode.None)
			{
				Log.Error("Caravan " + caravan + " tried to enter map " + map + " with enter mode " + enterMode);
				enterMode = CaravanEnterMode.Edge;
			}
			IntVec3 enterCell = CaravanEnterMapUtility.GetEnterCell(caravan, map, enterMode, extraCellValidator);
			Func<Pawn, IntVec3> spawnCellGetter = (Pawn p) => CellFinder.RandomSpawnCellForPawnNear(enterCell, map, 4);
			CaravanEnterMapUtility.Enter(caravan, map, spawnCellGetter, dropInventoryMode, draftColonists);
		}

		public static void Enter(Caravan caravan, Map map, Func<Pawn, IntVec3> spawnCellGetter, CaravanDropInventoryMode dropInventoryMode = CaravanDropInventoryMode.DoNotDrop, bool draftColonists = false)
		{
			CaravanEnterMapUtility.tmpPawns.Clear();
			CaravanEnterMapUtility.tmpPawns.AddRange(caravan.PawnsListForReading);
			for (int i = 0; i < CaravanEnterMapUtility.tmpPawns.Count; i++)
			{
				IntVec3 loc = spawnCellGetter(CaravanEnterMapUtility.tmpPawns[i]);
				GenSpawn.Spawn(CaravanEnterMapUtility.tmpPawns[i], loc, map, Rot4.Random, false);
			}
			switch (dropInventoryMode)
			{
			case CaravanDropInventoryMode.DropInstantly:
				CaravanEnterMapUtility.DropAllInventory(CaravanEnterMapUtility.tmpPawns);
				break;
			case CaravanDropInventoryMode.UnloadIndividually:
				for (int j = 0; j < CaravanEnterMapUtility.tmpPawns.Count; j++)
				{
					CaravanEnterMapUtility.tmpPawns[j].inventory.UnloadEverything = true;
				}
				break;
			}
			if (draftColonists)
			{
				CaravanEnterMapUtility.DraftColonists(CaravanEnterMapUtility.tmpPawns);
			}
			if (map.IsPlayerHome)
			{
				for (int k = 0; k < CaravanEnterMapUtility.tmpPawns.Count; k++)
				{
					if (CaravanEnterMapUtility.tmpPawns[k].IsPrisoner)
					{
						CaravanEnterMapUtility.tmpPawns[k].guest.WaitInsteadOfEscapingForDefaultTicks();
					}
				}
			}
			caravan.RemoveAllPawns();
			if (caravan.Spawned)
			{
				Find.WorldObjects.Remove(caravan);
			}
			CaravanEnterMapUtility.tmpPawns.Clear();
		}

		private static IntVec3 GetEnterCell(Caravan caravan, Map map, CaravanEnterMode enterMode, Predicate<IntVec3> extraCellValidator)
		{
			switch (enterMode)
			{
			case CaravanEnterMode.Edge:
				return CaravanEnterMapUtility.FindNearEdgeCell(map, extraCellValidator);
			case CaravanEnterMode.Center:
				return CaravanEnterMapUtility.FindCenterCell(map, extraCellValidator);
			default:
				throw new NotImplementedException("CaravanEnterMode");
			}
		}

		private static IntVec3 FindNearEdgeCell(Map map, Predicate<IntVec3> extraCellValidator)
		{
			Predicate<IntVec3> baseValidator = (IntVec3 x) => x.Standable(map) && !x.Fogged(map);
			Faction hostFaction = map.ParentFaction;
			IntVec3 root = default(IntVec3);
			if (CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 x) => baseValidator(x) && (extraCellValidator == null || extraCellValidator(x)) && ((hostFaction != null && map.reachability.CanReachFactionBase(x, hostFaction)) || (hostFaction == null && map.reachability.CanReachBiggestMapEdgeRoom(x)))), map, CellFinder.EdgeRoadChance_Neutral, out root))
			{
				return CellFinder.RandomClosewalkCellNear(root, map, 5, null);
			}
			if (extraCellValidator != null && CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 x) => baseValidator(x) && extraCellValidator(x)), map, CellFinder.EdgeRoadChance_Neutral, out root))
			{
				return CellFinder.RandomClosewalkCellNear(root, map, 5, null);
			}
			if (CellFinder.TryFindRandomEdgeCellWith(baseValidator, map, CellFinder.EdgeRoadChance_Neutral, out root))
			{
				return CellFinder.RandomClosewalkCellNear(root, map, 5, null);
			}
			Log.Warning("Could not find any valid edge cell.");
			return CellFinder.RandomCell(map);
		}

		private static IntVec3 FindCenterCell(Map map, Predicate<IntVec3> extraCellValidator)
		{
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false);
			Predicate<IntVec3> baseValidator = (IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReachMapEdge(x, traverseParms);
			IntVec3 result = default(IntVec3);
			if (extraCellValidator != null && RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((Predicate<IntVec3>)((IntVec3 x) => baseValidator(x) && extraCellValidator(x)), map, out result))
			{
				return result;
			}
			if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(baseValidator, map, out result))
			{
				return result;
			}
			Log.Warning("Could not find any valid cell.");
			return CellFinder.RandomCell(map);
		}

		public static void DropAllInventory(List<Pawn> pawns)
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				pawns[i].inventory.DropAllNearPawn(pawns[i].Position, false, true);
			}
		}

		private static void DraftColonists(List<Pawn> pawns)
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].IsColonist)
				{
					pawns[i].drafter.Drafted = true;
				}
			}
		}

		private static bool TryRandomNonOccupiedClosewalkCellNear(IntVec3 root, Map map, int radius, out IntVec3 result)
		{
			return CellFinder.TryFindRandomReachableCellNear(root, map, (float)radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (Predicate<IntVec3>)((IntVec3 c) => c.Standable(map) && c.GetFirstPawn(map) == null), (Predicate<Region>)null, out result, 999999);
		}
	}
}
