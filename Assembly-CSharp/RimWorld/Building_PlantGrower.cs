using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_PlantGrower : Building, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow;

		private CompPowerTrader compPower;

		public IEnumerable<Plant> PlantsOnMe
		{
			get
			{
				Building_PlantGrower.<>c__Iterator15A <>c__Iterator15A = new Building_PlantGrower.<>c__Iterator15A();
				<>c__Iterator15A.<>f__this = this;
				Building_PlantGrower.<>c__Iterator15A expr_0E = <>c__Iterator15A;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			Building_PlantGrower.<GetGizmos>c__Iterator15B <GetGizmos>c__Iterator15B = new Building_PlantGrower.<GetGizmos>c__Iterator15B();
			<GetGizmos>c__Iterator15B.<>f__this = this;
			Building_PlantGrower.<GetGizmos>c__Iterator15B expr_0E = <GetGizmos>c__Iterator15B;
			expr_0E.$PC = -2;
			return expr_0E;
		}

		public override void PostMake()
		{
			base.PostMake();
			this.plantDefToGrow = this.def.building.defaultPlantToGrow;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.compPower = base.GetComp<CompPowerTrader>();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<ThingDef>(ref this.plantDefToGrow, "plantDefToGrow");
		}

		public override void TickRare()
		{
			if (this.compPower != null && !this.compPower.PowerOn)
			{
				foreach (Plant current in this.PlantsOnMe)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 4, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
					current.TakeDamage(dinfo);
				}
			}
		}

		public override void DeSpawn()
		{
			foreach (Plant current in this.PlantsOnMe.ToList<Plant>())
			{
				current.Destroy(DestroyMode.Vanish);
			}
			base.DeSpawn();
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (base.Spawned)
			{
				if (GenPlant.GrowthSeasonNow(base.Position, base.Map))
				{
					text = text + "\n" + "GrowSeasonHereNow".Translate();
				}
				else
				{
					text = text + "\n" + "CannotGrowBadSeasonTemperature".Translate();
				}
			}
			return text;
		}

		public ThingDef GetPlantDefToGrow()
		{
			return this.plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			this.plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			return this.compPower == null || this.compPower.PowerOn;
		}

		virtual Map get_Map()
		{
			return base.Map;
		}
	}
}