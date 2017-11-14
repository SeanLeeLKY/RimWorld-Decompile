using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.Planet
{
	public static class MultipleCaravansCellFinder
	{
		private const int TriesToFindPerfectOppositeSpots = 10;

		private const int TriesToFindGoodEnoughOppositeSpots = 10;

		private const int TriesToFindMatchingPair = 20;

		private const float PerfectIfDistPctToOppositeSpotsAtMost = 0.05f;

		private const float GoodEnoughIfDistPctToOppositeSpotsAtMost = 0.15f;

		private const float SpotDistPctToEdge = 0.2f;

		private const float TryMinDistPctBetweenFallbackEdgeCells = 0.6f;

		public static void FindStartingCellsFor2Groups(Map map, out IntVec3 first, out IntVec3 second)
		{
			int num = 0;
			while (num < 10)
			{
				if (!MultipleCaravansCellFinder.TryFindOppositeSpots(map, 0.05f, out first, out second))
				{
					num++;
					continue;
				}
				return;
			}
			int num2 = 0;
			while (num2 < 10)
			{
				if (!MultipleCaravansCellFinder.TryFindOppositeSpots(map, 0.15f, out first, out second))
				{
					num2++;
					continue;
				}
				return;
			}
			if (!CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 x) => x.Standable(map) && !x.Fogged(map)), map, CellFinder.EdgeRoadChance_Neutral, out first))
			{
				Log.Error("Could not find any valid starting cell for a caravan.");
				first = CellFinder.RandomCell(map);
				second = CellFinder.RandomCell(map);
			}
			else
			{
				IntVec3 localFirst = first;
				IntVec3 size = map.Size;
				int x2 = size.x;
				IntVec3 size2 = map.Size;
				float tryMinDistBetweenSpots = (float)((float)Mathf.Max(x2, size2.z) * 0.60000002384185791);
				if (!CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && !x.InHorDistOf(localFirst, tryMinDistBetweenSpots)), map, CellFinder.EdgeRoadChance_Neutral, out second) && !CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)((IntVec3 x) => x.Standable(map) && !x.Fogged(map)), map, 0.5f, out second))
				{
					Log.Error("Could not find any valid starting cell for a caravan.");
					second = CellFinder.RandomCell(map);
				}
				else
				{
					first = CellFinder.RandomClosewalkCellNear(first, map, 7, null);
					second = CellFinder.RandomClosewalkCellNear(second, map, 7, null);
				}
			}
		}

		private static bool TryFindOppositeSpots(Map map, float maxDistPctToOppositeSpots, out IntVec3 first, out IntVec3 second)
		{
			IntVec3 intVec = MultipleCaravansCellFinder.RandomSpotNearEdge(map);
			IntVec3 intVec2 = MultipleCaravansCellFinder.OppositeSpot(intVec, map);
			IntVec3 size = map.Size;
			int x = size.x;
			IntVec3 size2 = map.Size;
			int num = Mathf.Min(x, size2.z);
			CellRect cellRect = CellRect.CenteredOn(intVec, Mathf.Max(Mathf.RoundToInt((float)num * maxDistPctToOppositeSpots), 1)).ClipInsideMap(map);
			CellRect cellRect2 = CellRect.CenteredOn(intVec2, Mathf.Max(Mathf.RoundToInt((float)num * maxDistPctToOppositeSpots), 1)).ClipInsideMap(map);
			for (int i = 0; i < 20; i++)
			{
				IntVec3 intVec3 = (i != 0) ? cellRect.RandomCell : intVec;
				IntVec3 intVec4 = (i != 0) ? cellRect2.RandomCell : intVec2;
				if (intVec3.Standable(map) && !intVec3.Fogged(map) && intVec4.Standable(map) && !intVec4.Fogged(map) && map.reachability.CanReach(intVec3, intVec4, PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false)))
				{
					first = intVec3;
					second = intVec4;
					return true;
				}
			}
			first = IntVec3.Invalid;
			second = IntVec3.Invalid;
			return false;
		}

		private static IntVec3 RandomSpotNearEdge(Map map)
		{
			CellRect cellRect = CellRect.WholeMap(map);
			int minX = cellRect.minX;
			IntVec3 size = map.Size;
			cellRect.minX = minX + Mathf.RoundToInt((float)((float)size.x * 0.20000000298023224));
			int minZ = cellRect.minZ;
			IntVec3 size2 = map.Size;
			cellRect.minZ = minZ + Mathf.RoundToInt((float)((float)size2.z * 0.20000000298023224));
			int maxX = cellRect.maxX;
			IntVec3 size3 = map.Size;
			cellRect.maxX = maxX - Mathf.RoundToInt((float)((float)size3.x * 0.20000000298023224));
			int maxZ = cellRect.maxZ;
			IntVec3 size4 = map.Size;
			cellRect.maxZ = maxZ - Mathf.RoundToInt((float)((float)size4.z * 0.20000000298023224));
			return cellRect.EdgeCells.RandomElement();
		}

		private static IntVec3 OppositeSpot(IntVec3 spot, Map map)
		{
			IntVec3 size = map.Size;
			int newX = size.x - spot.x;
			int y = spot.y;
			IntVec3 size2 = map.Size;
			return new IntVec3(newX, y, size2.z - spot.z);
		}
	}
}
