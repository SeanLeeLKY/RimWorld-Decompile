using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class WalkPathFinder
	{
		private const int NumPathNodes = 8;

		private const float StepDistMin = 2f;

		private const float StepDistMax = 14f;

		private static readonly int StartRadialIndex = GenRadial.NumCellsInRadius(14f);

		private static readonly int EndRadialIndex = GenRadial.NumCellsInRadius(2f);

		private static readonly int RadialIndexStride = 3;

		public static bool TryFindWalkPath(Pawn pawn, IntVec3 root, out List<IntVec3> result)
		{
			List<IntVec3> list = new List<IntVec3>();
			list.Add(root);
			IntVec3 intVec = root;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec2 = IntVec3.Invalid;
				float num = -1f;
				for (int num2 = WalkPathFinder.StartRadialIndex; num2 > WalkPathFinder.EndRadialIndex; num2 -= WalkPathFinder.RadialIndexStride)
				{
					IntVec3 intVec3 = intVec + GenRadial.RadialPattern[num2];
					if (intVec3.InBounds(pawn.Map) && intVec3.Standable(pawn.Map) && !intVec3.IsForbidden(pawn) && GenSight.LineOfSight(intVec, intVec3, pawn.Map, false, null, 0, 0) && !intVec3.Roofed(pawn.Map) && !PawnUtility.KnownDangerAt(intVec3, pawn))
					{
						float num3 = 10000f;
						for (int j = 0; j < list.Count; j++)
						{
							num3 += (float)(list[j] - intVec3).LengthManhattan;
						}
						float num4 = (float)(intVec3 - root).LengthManhattan;
						if (num4 > 40.0)
						{
							num3 *= Mathf.InverseLerp(70f, 40f, num4);
						}
						if (list.Count >= 2)
						{
							float angleFlat = (list[list.Count - 1] - list[list.Count - 2]).AngleFlat;
							float angleFlat2 = (intVec3 - intVec).AngleFlat;
							float num5;
							if (angleFlat2 > angleFlat)
							{
								num5 = angleFlat2 - angleFlat;
							}
							else
							{
								angleFlat = (float)(angleFlat - 360.0);
								num5 = angleFlat2 - angleFlat;
							}
							if (num5 > 110.0)
							{
								num3 = (float)(num3 * 0.0099999997764825821);
							}
						}
						if (list.Count >= 4 && (intVec - root).LengthManhattan < (intVec3 - root).LengthManhattan)
						{
							num3 = (float)(num3 * 9.9999997473787516E-06);
						}
						if (num3 > num)
						{
							intVec2 = intVec3;
							num = num3;
						}
					}
				}
				if (num < 0.0)
				{
					result = null;
					return false;
				}
				list.Add(intVec2);
				intVec = intVec2;
			}
			list.Add(root);
			result = list;
			return true;
		}

		public static void DebugFlashWalkPath(IntVec3 root, int numEntries = 8)
		{
			Map visibleMap = Find.VisibleMap;
			List<IntVec3> list = default(List<IntVec3>);
			if (!WalkPathFinder.TryFindWalkPath(visibleMap.mapPawns.FreeColonistsSpawned.First(), root, out list))
			{
				visibleMap.debugDrawer.FlashCell(root, 0.2f, "NOPATH", 50);
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					visibleMap.debugDrawer.FlashCell(list[i], (float)i / (float)numEntries, i.ToString(), 50);
					if (i > 0)
					{
						visibleMap.debugDrawer.FlashLine(list[i], list[i - 1], 50);
					}
				}
			}
		}
	}
}
