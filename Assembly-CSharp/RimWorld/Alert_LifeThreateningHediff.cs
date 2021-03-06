using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_LifeThreateningHediff : Alert_Critical
	{
		private IEnumerable<Pawn> SickPawns
		{
			get
			{
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
				{
					foreach (Hediff hediff in item.health.hediffSet.hediffs)
					{
						if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && !hediff.FullyImmune())
						{
							yield return item;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				yield break;
				IL_015d:
				/*Error near IL_015e: Unexpected return in MoveNext()*/;
			}
		}

		public override string GetLabel()
		{
			return "PawnsWithLifeThreateningDisease".Translate();
		}

		public override string GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (Pawn sickPawn in this.SickPawns)
			{
				stringBuilder.AppendLine("    " + sickPawn.NameStringShort);
				foreach (Hediff hediff in sickPawn.health.hediffSet.hediffs)
				{
					if (hediff.CurStage != null && hediff.CurStage.lifeThreatening && hediff.Part != null && hediff.Part != sickPawn.RaceProps.body.corePart)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				return string.Format("PawnsWithLifeThreateningDiseaseAmputationDesc".Translate(), stringBuilder.ToString());
			}
			return string.Format("PawnsWithLifeThreateningDiseaseDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritIs(this.SickPawns.FirstOrDefault());
		}
	}
}
