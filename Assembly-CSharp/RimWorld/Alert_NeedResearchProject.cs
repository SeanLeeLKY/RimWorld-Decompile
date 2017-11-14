using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedResearchProject : Alert
	{
		public Alert_NeedResearchProject()
		{
			base.defaultLabel = "NeedResearchProject".Translate();
			base.defaultExplanation = "NeedResearchProjectDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (Find.AnyPlayerHomeMap == null)
			{
				return false;
			}
			if (Find.ResearchManager.currentProj != null)
			{
				return false;
			}
			bool flag = false;
			List<Map> maps = Find.Maps;
			int num = 0;
			while (num < maps.Count)
			{
				if (!maps[num].IsPlayerHome || !maps[num].listerBuildings.ColonistsHaveResearchBench())
				{
					num++;
					continue;
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				return false;
			}
			if (!Find.ResearchManager.AnyProjectIsAvailable)
			{
				return false;
			}
			return true;
		}
	}
}
