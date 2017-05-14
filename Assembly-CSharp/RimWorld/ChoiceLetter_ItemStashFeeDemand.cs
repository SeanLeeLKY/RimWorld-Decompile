using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_ItemStashFeeDemand : ChoiceLetter, IThingHolder
	{
		public Map map;

		public int fee;

		public int siteDaysTimeout;

		public ThingOwner items;

		public Faction siteFaction;

		public SitePartDef sitePart;

		public Faction alliedFaction;

		public bool sitePartsKnown;

		protected override IEnumerable<DiaOption> Choices
		{
			get
			{
				ChoiceLetter_ItemStashFeeDemand.<>c__Iterator19C <>c__Iterator19C = new ChoiceLetter_ItemStashFeeDemand.<>c__Iterator19C();
				<>c__Iterator19C.<>f__this = this;
				ChoiceLetter_ItemStashFeeDemand.<>c__Iterator19C expr_0E = <>c__Iterator19C;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		public override bool StillValid
		{
			get
			{
				return base.StillValid && !this.alliedFaction.HostileTo(Faction.OfPlayer) && (this.map == null || Find.Maps.Contains(this.map));
			}
		}

		public ChoiceLetter_ItemStashFeeDemand()
		{
			this.items = new ThingOwner<Thing>(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Map>(ref this.map, "map", false);
			Scribe_Values.Look<int>(ref this.fee, "fee", 0, false);
			Scribe_Values.Look<int>(ref this.siteDaysTimeout, "siteDaysTimeout", 0, false);
			Scribe_Deep.Look<ThingOwner>(ref this.items, "items", new object[]
			{
				this
			});
			Scribe_References.Look<Faction>(ref this.siteFaction, "siteFaction", false);
			Scribe_Defs.Look<SitePartDef>(ref this.sitePart, "sitePart");
			Scribe_References.Look<Faction>(ref this.alliedFaction, "alliedFaction", false);
			Scribe_Values.Look<bool>(ref this.sitePartsKnown, "sitePartsKnown", false, false);
		}

		public override void Removed()
		{
			base.Removed();
			this.items.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.items;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		virtual IThingHolder get_ParentHolder()
		{
			return base.ParentHolder;
		}
	}
}
