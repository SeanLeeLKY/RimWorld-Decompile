using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public static class GenClosest
	{
		private const int DefaultLocalTraverseRegionsBeforeGlobal = 30;

		private static bool EarlyOutSearch(IntVec3 start, Map map, ThingRequest thingReq, IEnumerable<Thing> customGlobalSearchSet)
		{
			if (thingReq.group == ThingRequestGroup.Everything)
			{
				Log.Error("Cannot do ClosestThingReachable searching everything without restriction.");
				return true;
			}
			if (!start.InBounds(map))
			{
				Log.Error("Did FindClosestThing with start out of bounds (" + start + "), thingReq=" + thingReq);
				return true;
			}
			if (thingReq.group == ThingRequestGroup.Nothing)
			{
				return true;
			}
			if (customGlobalSearchSet == null && !thingReq.IsUndefined && map.listerThings.ThingsMatching(thingReq).Count == 0)
			{
				return true;
			}
			return false;
		}

		public static Thing ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionsMax = -1, bool forceGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			if (searchRegionsMax > 0 && customGlobalSearchSet != null && !forceGlobalSearch)
			{
				Log.ErrorOnce("searchRegionsMax > 0 && customGlobalSearchSet != null && !forceGlobalSearch. customGlobalSearchSet will never be used.", 634984);
			}
			if (GenClosest.EarlyOutSearch(root, map, thingReq, customGlobalSearchSet))
			{
				return null;
			}
			Thing thing = null;
			if (!thingReq.IsUndefined)
			{
				int maxRegions = (searchRegionsMax <= 0) ? 30 : searchRegionsMax;
				thing = GenClosest.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, null, searchRegionsMin, maxRegions, maxDistance, traversableRegionTypes, ignoreEntirelyForbiddenRegions);
			}
			if (thing == null && (searchRegionsMax < 0 || forceGlobalSearch))
			{
				if (traversableRegionTypes != RegionType.Set_Passable)
				{
					Log.ErrorOnce("ClosestThingReachable had to do a global search, but traversableRegionTypes is not set to passable only. It's not supported, because Reachability is based on passable regions only.", 14384767);
				}
				Predicate<Thing> validator2 = delegate(Thing t)
				{
					if (!map.reachability.CanReach(root, t, peMode, traverseParams))
					{
						return false;
					}
					if (validator != null && !validator(t))
					{
						return false;
					}
					return true;
				};
				IEnumerable<Thing> searchSet = customGlobalSearchSet ?? map.listerThings.ThingsMatching(thingReq);
				thing = GenClosest.ClosestThing_Global(root, searchSet, maxDistance, validator2, null);
			}
			return thing;
		}

		public static Thing ClosestThing_Regionwise_ReachablePrioritized(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null, int minRegions = 24, int maxRegions = 30)
		{
			if (GenClosest.EarlyOutSearch(root, map, thingReq, null))
			{
				return null;
			}
			if (maxRegions < minRegions)
			{
				Log.ErrorOnce("maxRegions < minRegions", 754343);
			}
			Thing result = null;
			if (!thingReq.IsUndefined)
			{
				result = GenClosest.RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, priorityGetter, minRegions, maxRegions, maxDistance, RegionType.Set_Passable, false);
			}
			return result;
		}

		public static Thing RegionwiseBFSWorker(IntVec3 root, Map map, ThingRequest req, PathEndMode peMode, TraverseParms traverseParams, Predicate<Thing> validator, Func<Thing, float> priorityGetter, int minRegions, int maxRegions, float maxDistance, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false)
		{
			if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
			{
				Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThings. Use ClosestThingGlobal.");
				return null;
			}
			Region region = root.GetRegion(map, traversableRegionTypes);
			if (region == null)
			{
				return null;
			}
			float maxDistSquared = maxDistance * maxDistance;
			RegionEntryPredicate entryCondition = delegate(Region from, Region to)
			{
				if (!to.Allows(traverseParams, false))
				{
					return false;
				}
				return maxDistance > 5000.0 || to.extentsClose.ClosestDistSquaredTo(root) < maxDistSquared;
			};
			Thing closestThing = null;
			float closestDistSquared = 9999999f;
			float bestPrio = -3.40282347E+38f;
			int regionsSeen = 0;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.portal == null && !r.Allows(traverseParams, true))
				{
					return false;
				}
				if (!ignoreEntirelyForbiddenRegions || !r.IsForbiddenEntirely(traverseParams.pawn))
				{
					List<Thing> list = r.ListerThings.ThingsMatching(req);
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing = list[i];
						if (ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, peMode, traverseParams.pawn))
						{
							float num = (float)((priorityGetter == null) ? 0.0 : priorityGetter(thing));
							if (!(num < bestPrio))
							{
								float num2 = (float)(thing.Position - root).LengthHorizontalSquared;
								if ((num > bestPrio || num2 < closestDistSquared) && num2 < maxDistSquared && (validator == null || validator(thing)))
								{
									closestThing = thing;
									closestDistSquared = num2;
									bestPrio = num;
								}
							}
						}
					}
				}
				regionsSeen++;
				return regionsSeen >= minRegions && closestThing != null;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, maxRegions, traversableRegionTypes);
			return closestThing;
		}

		public static Thing ClosestThing_Global(IntVec3 center, IEnumerable searchSet, float maxDistance = 99999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null)
		{
			if (searchSet == null)
			{
				return null;
			}
			float num = 2.14748365E+09f;
			Thing result = null;
			float num2 = -3.40282347E+38f;
			float num3 = maxDistance * maxDistance;
			IEnumerator enumerator = searchSet.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Thing thing = (Thing)enumerator.Current;
					float num4;
					float num5;
					if (thing.Spawned)
					{
						num4 = (float)(center - thing.Position).LengthHorizontalSquared;
						if (!(num4 > num3) && (priorityGetter != null || num4 < num) && (validator == null || validator(thing)))
						{
							num5 = 0f;
							if (priorityGetter != null)
							{
								num5 = priorityGetter(thing);
								if (!(num5 < num2) && (num5 != num2 || !(num4 >= num)))
								{
									goto IL_00d2;
								}
								continue;
							}
							goto IL_00d2;
						}
					}
					continue;
					IL_00d2:
					result = thing;
					num = num4;
					num2 = num5;
				}
				return result;
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public static Thing ClosestThing_Global_Reachable(IntVec3 center, Map map, IEnumerable<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null)
		{
			if (searchSet == null)
			{
				return null;
			}
			int num = 0;
			int num2 = 0;
			Thing result = null;
			float num3 = -3.40282347E+38f;
			float num4 = maxDistance * maxDistance;
			float num5 = 2.14748365E+09f;
			foreach (Thing item in searchSet)
			{
				float num6;
				float num7;
				if (item.Spawned)
				{
					num2++;
					num6 = (float)(center - item.Position).LengthHorizontalSquared;
					if (!(num6 > num4) && (priorityGetter != null || num6 < num5) && map.reachability.CanReach(center, item, peMode, traverseParams) && (validator == null || validator(item)))
					{
						num7 = 0f;
						if (priorityGetter != null)
						{
							num7 = priorityGetter(item);
							if (!(num7 < num3) && (num7 != num3 || !(num6 >= num5)))
							{
								goto IL_00fe;
							}
							continue;
						}
						goto IL_00fe;
					}
				}
				continue;
				IL_00fe:
				result = item;
				num5 = num6;
				num3 = num7;
				num++;
			}
			return result;
		}
	}
}
