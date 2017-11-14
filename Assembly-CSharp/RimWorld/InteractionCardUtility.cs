using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class InteractionCardUtility
	{
		private static Vector2 logScrollPosition = Vector2.zero;

		private static List<Pair<string, int>> logStrings = new List<Pair<string, int>>();

		public static void DrawInteractionsLog(Rect rect, Pawn pawn, List<LogEntry> entries, int maxEntries)
		{
			float width = (float)(rect.width - 26.0 - 3.0 - 16.0 - 10.0);
			InteractionCardUtility.logStrings.Clear();
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < entries.Count; i++)
			{
				if (entries[i].Concerns(pawn))
				{
					string text = entries[i].ToGameStringFromPOV(pawn);
					InteractionCardUtility.logStrings.Add(new Pair<string, int>(text, i));
					num += Mathf.Max(26f, Text.CalcHeight(text, width));
					num2++;
					if (num2 >= maxEntries)
						break;
				}
			}
			Rect viewRect = new Rect(0f, 0f, (float)(rect.width - 16.0), num);
			Widgets.BeginScrollView(rect, ref InteractionCardUtility.logScrollPosition, viewRect, true);
			float num3 = 0f;
			for (int j = 0; j < InteractionCardUtility.logStrings.Count; j++)
			{
				string first = InteractionCardUtility.logStrings[j].First;
				LogEntry entry = entries[InteractionCardUtility.logStrings[j].Second];
				if (entry.Age > 7500)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				float num4 = Mathf.Max(26f, Text.CalcHeight(first, width));
				Texture2D texture2D = entry.IconFromPOV(pawn);
				if ((Object)texture2D != (Object)null)
				{
					Rect position = new Rect(0f, num3, 26f, 26f);
					GUI.DrawTexture(position, texture2D);
				}
				Rect rect2 = new Rect(29f, num3, width, num4);
				Widgets.DrawHighlightIfMouseover(rect2);
				Widgets.Label(rect2, first);
				TooltipHandler.TipRegion(rect2, () => entry.GetTipString(), 613261 + j * 611);
				if (Widgets.ButtonInvisible(rect2, false))
				{
					entry.ClickedFromPOV(pawn);
				}
				GUI.color = Color.white;
				num3 += num4;
			}
			GUI.EndScrollView();
		}
	}
}
