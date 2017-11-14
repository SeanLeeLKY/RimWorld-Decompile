using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AIFightEnemies : JobGiver_AIFightEnemy
	{
		protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
		{
			bool allowManualCastWeapons = !pawn.IsColonist;
			Verb verb = pawn.TryGetAttackVerb(allowManualCastWeapons);
			if (verb == null)
			{
				dest = IntVec3.Invalid;
				return false;
			}
			CastPositionRequest newReq = default(CastPositionRequest);
			newReq.caster = pawn;
			newReq.target = pawn.mindState.enemyTarget;
			newReq.verb = verb;
			newReq.maxRangeFromTarget = verb.verbProps.range;
			newReq.wantCoverFromTarget = (verb.verbProps.range > 5.0);
			return CastPositionFinder.TryFindCastPosition(newReq, out dest);
		}
	}
}
