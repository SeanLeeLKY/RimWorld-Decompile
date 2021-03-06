using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class ApparelProperties
	{
		public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();

		public List<ApparelLayer> layers = new List<ApparelLayer>();

		public string wornGraphicPath = string.Empty;

		public List<string> tags = new List<string>();

		public List<string> defaultOutfitTags;

		public float wearPerDay = 0.4f;

		public bool careIfWornByCorpse = true;

		public bool hatRenderedFrontOfFace;

		[Unsaved]
		private float cachedHumanBodyCoverage = -1f;

		[Unsaved]
		private BodyPartGroupDef[][] interferingBodyPartGroups;

		private static BodyPartGroupDef[] apparelRelevantGroups;

		public ApparelLayer LastLayer
		{
			get
			{
				if (this.layers.Count > 0)
				{
					return this.layers[this.layers.Count - 1];
				}
				Log.ErrorOnce("Failed to get last layer on apparel item (see your config errors)", 31234937);
				return ApparelLayer.Belt;
			}
		}

		public float HumanBodyCoverage
		{
			get
			{
				if (this.cachedHumanBodyCoverage < 0.0)
				{
					this.cachedHumanBodyCoverage = 0f;
					List<BodyPartRecord> allParts = BodyDefOf.Human.AllParts;
					for (int i = 0; i < allParts.Count; i++)
					{
						if (this.CoversBodyPart(allParts[i]))
						{
							this.cachedHumanBodyCoverage += allParts[i].coverageAbs;
						}
					}
				}
				return this.cachedHumanBodyCoverage;
			}
		}

		public static void Reset()
		{
			ApparelProperties.apparelRelevantGroups = (from td in DefDatabase<ThingDef>.AllDefs
			where td.IsApparel
			select td).SelectMany((ThingDef td) => td.apparel.bodyPartGroups).Distinct().ToArray();
		}

		public IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (!this.layers.NullOrEmpty())
				yield break;
			yield return parentDef.defName + " apparel has no layers.";
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public bool CoversBodyPart(BodyPartRecord partRec)
		{
			for (int i = 0; i < partRec.groups.Count; i++)
			{
				if (this.bodyPartGroups.Contains(partRec.groups[i]))
				{
					return true;
				}
			}
			return false;
		}

		public string GetCoveredOuterPartsString(BodyDef body)
		{
			IEnumerable<BodyPartRecord> source = from x in body.AllParts
			where x.depth == BodyPartDepth.Outside && x.groups.Any((BodyPartGroupDef y) => this.bodyPartGroups.Contains(y))
			select x;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (BodyPartRecord item in source.Distinct())
			{
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				flag = false;
				stringBuilder.Append(item.def.label);
			}
			return stringBuilder.ToString().CapitalizeFirst();
		}

		public BodyPartGroupDef[] GetInterferingBodyPartGroups(BodyDef body)
		{
			if (this.interferingBodyPartGroups == null)
			{
				this.interferingBodyPartGroups = new BodyPartGroupDef[DefDatabase<BodyPartGroupDef>.DefCount][];
			}
			if (this.interferingBodyPartGroups[body.index] == null)
			{
				BodyPartRecord[] source = (from part in body.AllParts
				where part.groups.Any((BodyPartGroupDef @group) => this.bodyPartGroups.Contains(@group))
				select part).ToArray();
				BodyPartGroupDef[] array = (from bpgd in source.SelectMany((BodyPartRecord bpr) => bpr.groups).Distinct()
				where ApparelProperties.apparelRelevantGroups.Contains(bpgd)
				select bpgd).ToArray();
				this.interferingBodyPartGroups[body.index] = array;
			}
			return this.interferingBodyPartGroups[body.index];
		}
	}
}
