using Verse;

namespace RimWorld
{
	public static class HuntJobUtility
	{
		public static bool WasKilledByHunter(Pawn pawn, DamageInfo? dinfo)
		{
			if (!dinfo.HasValue)
			{
				return false;
			}
			Pawn pawn2 = dinfo.Value.Instigator as Pawn;
			if (pawn2 != null && pawn2.CurJob != null)
			{
				JobDriver_Hunt jobDriver_Hunt = pawn2.jobs.curDriver as JobDriver_Hunt;
				return jobDriver_Hunt != null && jobDriver_Hunt.Victim == pawn;
			}
			return false;
		}
	}
}
