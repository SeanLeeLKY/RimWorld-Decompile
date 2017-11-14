using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_OpinionOfMyLover : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
			if (directPawnRelation == null)
			{
				return false;
			}
			if (directPawnRelation.otherPawn.IsColonist && !directPawnRelation.otherPawn.IsWorldPawn() && directPawnRelation.otherPawn.relations.everSeenByPlayer)
			{
				return p.relations.OpinionOf(directPawnRelation.otherPawn) != 0;
			}
			return false;
		}
	}
}
