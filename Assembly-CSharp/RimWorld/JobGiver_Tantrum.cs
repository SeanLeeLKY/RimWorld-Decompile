using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Tantrum : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_Tantrum mentalState_Tantrum = pawn.MentalState as MentalState_Tantrum;
			if (mentalState_Tantrum != null && mentalState_Tantrum.target != null && pawn.CanReach(mentalState_Tantrum.target, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				Verb verbToUse = null;
				Pawn pawn2 = mentalState_Tantrum.target as Pawn;
				if (pawn2 != null)
				{
					if (pawn2.Downed)
					{
						return null;
					}
					if (!InteractionUtility.TryGetRandomVerbForSocialFight(pawn, out verbToUse))
					{
						return null;
					}
				}
				Job job = new Job(JobDefOf.AttackMelee, mentalState_Tantrum.target);
				job.maxNumMeleeAttacks = 1;
				job.verbToUse = verbToUse;
				return job;
			}
			return null;
		}
	}
}
