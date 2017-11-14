using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ListerHaulables
	{
		private Map map;

		private List<Thing> haulables = new List<Thing>();

		private const int CellsPerTick = 4;

		private static int groupCycleIndex;

		private List<int> cellCycleIndices = new List<int>();

		private string debugOutput = "uninitialized";

		public ListerHaulables(Map map)
		{
			this.map = map;
		}

		public List<Thing> ThingsPotentiallyNeedingHauling()
		{
			return this.haulables;
		}

		public void Notify_Spawned(Thing t)
		{
			this.CheckAdd(t);
		}

		public void Notify_DeSpawned(Thing t)
		{
			this.TryRemove(t);
		}

		public void HaulDesignationAdded(Thing t)
		{
			this.CheckAdd(t);
		}

		public void HaulDesignationRemoved(Thing t)
		{
			this.TryRemove(t);
		}

		public void Notify_Unforbidden(Thing t)
		{
			this.CheckAdd(t);
		}

		public void Notify_Forbidden(Thing t)
		{
			this.TryRemove(t);
		}

		public void Notify_SlotGroupChanged(SlotGroup sg)
		{
			if (sg.CellsList != null)
			{
				for (int i = 0; i < sg.CellsList.Count; i++)
				{
					this.RecalcAllInCell(sg.CellsList[i]);
				}
			}
		}

		public void ListerHaulablesTick()
		{
			ListerHaulables.groupCycleIndex++;
			if (ListerHaulables.groupCycleIndex >= 2147473647)
			{
				ListerHaulables.groupCycleIndex = 0;
			}
			List<SlotGroup> allGroupsListForReading = this.map.slotGroupManager.AllGroupsListForReading;
			if (allGroupsListForReading.Count != 0)
			{
				int num = ListerHaulables.groupCycleIndex % allGroupsListForReading.Count;
				SlotGroup slotGroup = allGroupsListForReading[ListerHaulables.groupCycleIndex % allGroupsListForReading.Count];
				while (this.cellCycleIndices.Count <= num)
				{
					this.cellCycleIndices.Add(0);
				}
				if (this.cellCycleIndices[num] >= 2147473647)
				{
					this.cellCycleIndices[num] = 0;
				}
				for (int i = 0; i < 4; i++)
				{
					List<int> list;
					int index;
					(list = this.cellCycleIndices)[index = num] = list[index] + 1;
					IntVec3 c = slotGroup.CellsList[this.cellCycleIndices[num] % slotGroup.CellsList.Count];
					List<Thing> thingList = c.GetThingList(this.map);
					int num2 = 0;
					while (num2 < thingList.Count)
					{
						if (!thingList[num2].def.EverHaulable)
						{
							num2++;
							continue;
						}
						this.Check(thingList[num2]);
						break;
					}
				}
			}
		}

		public void RecalcAllInCell(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList(this.map);
			for (int i = 0; i < thingList.Count; i++)
			{
				this.Check(thingList[i]);
			}
		}

		private void Check(Thing t)
		{
			if (this.ShouldBeHaulable(t))
			{
				if (!this.haulables.Contains(t))
				{
					this.haulables.Add(t);
				}
			}
			else if (this.haulables.Contains(t))
			{
				this.haulables.Remove(t);
			}
		}

		private bool ShouldBeHaulable(Thing t)
		{
			if (t.IsForbidden(Faction.OfPlayer))
			{
				return false;
			}
			if (!t.def.alwaysHaulable)
			{
				if (!t.def.EverHaulable)
				{
					return false;
				}
				if (this.map.designationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInAnyStorage())
				{
					return false;
				}
			}
			if (t.IsInValidBestStorage())
			{
				return false;
			}
			return true;
		}

		private void CheckAdd(Thing t)
		{
			if (this.ShouldBeHaulable(t) && !this.haulables.Contains(t))
			{
				this.haulables.Add(t);
			}
		}

		private void TryRemove(Thing t)
		{
			if (t.def.category == ThingCategory.Item && this.haulables.Contains(t))
			{
				this.haulables.Remove(t);
			}
		}

		internal string DebugString()
		{
			if (Time.frameCount % 10 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("======= All haulables (Count " + this.haulables.Count + ")");
				int num = 0;
				foreach (Thing haulable in this.haulables)
				{
					stringBuilder.AppendLine(haulable.ThingID);
					num++;
					if (num > 200)
						break;
				}
				this.debugOutput = stringBuilder.ToString();
			}
			return this.debugOutput;
		}
	}
}
