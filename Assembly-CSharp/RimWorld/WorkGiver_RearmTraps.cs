using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_RearmTraps : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			using (IEnumerator<Designation> enumerator = pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.RearmTrap).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Designation des = enumerator.Current;
					yield return des.target.Thing;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00d2:
			/*Error near IL_00d3: Unexpected return in MoveNext()*/;
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.RearmTrap) == null)
			{
				return false;
			}
			LocalTargetInfo target = t;
			if (!pawn.CanReserve(target, 1, -1, null, forced))
			{
				return false;
			}
			List<Thing> thingList = t.Position.GetThingList(t.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				IntVec3 intVec = default(IntVec3);
				if (thingList[i] != t && thingList[i].def.category == ThingCategory.Item && (thingList[i].IsForbidden(pawn) || thingList[i].IsInValidStorage() || !HaulAIUtility.CanHaulAside(pawn, thingList[i], out intVec)))
				{
					return false;
				}
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			List<Thing> thingList = t.Position.GetThingList(t.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] != t && thingList[i].def.category == ThingCategory.Item)
				{
					Job job = HaulAIUtility.HaulAsideJobFor(pawn, thingList[i]);
					if (job != null)
					{
						return job;
					}
				}
			}
			return new Job(JobDefOf.RearmTrap, t);
		}
	}
}
