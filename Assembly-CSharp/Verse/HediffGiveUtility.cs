using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class HediffGiveUtility
	{
		public static bool TryApply(Pawn pawn, HediffDef hediff, List<BodyPartDef> partsToAffect, bool canAffectAnyLivePart = false, int countToAffect = 1, List<Hediff> outAddedHediffs = null)
		{
			if (!canAffectAnyLivePart && partsToAffect == null)
			{
				if (!pawn.health.hediffSet.HasHediff(hediff, false))
				{
					Hediff hediff2 = HediffMaker.MakeHediff(hediff, pawn, null);
					pawn.health.AddHediff(hediff2, null, null);
					if (outAddedHediffs != null)
					{
						outAddedHediffs.Add(hediff2);
					}
					return true;
				}
				return false;
			}
			bool result = false;
			int num = 0;
			while (num < countToAffect)
			{
				IEnumerable<BodyPartRecord> source = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined);
				if (partsToAffect != null)
				{
					source = from p in source
					where partsToAffect.Contains(p.def)
					select p;
				}
				if (canAffectAnyLivePart)
				{
					source = from p in source
					where p.def.isAlive
					select p;
				}
				source = from p in source
				where !pawn.health.hediffSet.HasHediff(hediff, p, false) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)
				select p;
				if (source.Any())
				{
					BodyPartRecord partRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
					Hediff hediff3 = HediffMaker.MakeHediff(hediff, pawn, partRecord);
					pawn.health.AddHediff(hediff3, null, null);
					if (outAddedHediffs != null)
					{
						outAddedHediffs.Add(hediff3);
					}
					result = true;
					num++;
					continue;
				}
				break;
			}
			return result;
		}
	}
}
