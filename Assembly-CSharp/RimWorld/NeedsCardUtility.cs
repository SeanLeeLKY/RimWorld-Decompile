using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class NeedsCardUtility
	{
		private static List<Need> displayNeeds = new List<Need>();

		private static readonly Color MoodColor = new Color(0.1f, 1f, 0.1f);

		private static readonly Color MoodColorNegative = new Color(0.8f, 0.4f, 0.4f);

		private static readonly Color NoEffectColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);

		private const float ThoughtHeight = 20f;

		private const float ThoughtSpacing = 4f;

		private const float ThoughtIntervalY = 24f;

		private const float MoodX = 235f;

		private const float MoodNumberWidth = 32f;

		private const float NeedsColumnWidth = 225f;

		public static readonly Vector2 FullSize = new Vector2(580f, 520f);

		private static List<Thought> thoughtGroupsPresent = new List<Thought>();

		private static List<Thought> thoughtGroup = new List<Thought>();

		public static Vector2 GetSize(Pawn pawn)
		{
			NeedsCardUtility.UpdateDisplayNeeds(pawn);
			if (pawn.needs.mood != null)
			{
				return NeedsCardUtility.FullSize;
			}
			float num = (float)NeedsCardUtility.displayNeeds.Count;
			Vector2 fullSize = NeedsCardUtility.FullSize;
			return new Vector2(225f, num * Mathf.Min(70f, fullSize.y / (float)NeedsCardUtility.displayNeeds.Count));
		}

		public static void DoNeedsMoodAndThoughts(Rect rect, Pawn pawn, ref Vector2 thoughtScrollPosition)
		{
			Rect rect2 = new Rect(rect.x, rect.y, 225f, rect.height);
			NeedsCardUtility.DoNeeds(rect2, pawn);
			if (pawn.needs.mood != null)
			{
				Rect rect3 = new Rect(rect2.xMax, rect.y, rect.width - rect2.width, rect.height);
				NeedsCardUtility.DoMoodAndThoughts(rect3, pawn, ref thoughtScrollPosition);
			}
		}

		public static void DoNeeds(Rect rect, Pawn pawn)
		{
			NeedsCardUtility.UpdateDisplayNeeds(pawn);
			float num = 0f;
			for (int i = 0; i < NeedsCardUtility.displayNeeds.Count; i++)
			{
				Need need = NeedsCardUtility.displayNeeds[i];
				Rect rect2 = new Rect(rect.x, rect.y + num, rect.width, Mathf.Min(70f, rect.height / (float)NeedsCardUtility.displayNeeds.Count));
				if (!need.def.major)
				{
					if (i > 0 && NeedsCardUtility.displayNeeds[i - 1].def.major)
					{
						rect2.y += 10f;
					}
					rect2.width *= 0.73f;
					rect2.height = Mathf.Max((float)(rect2.height * 0.66600000858306885), 30f);
				}
				need.DrawOnGUI(rect2, 2147483647, -1f, true, true);
				num = rect2.yMax;
			}
		}

		private static void DoMoodAndThoughts(Rect rect, Pawn pawn, ref Vector2 thoughtScrollPosition)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, (float)(rect.width * 0.800000011920929), 70f);
			pawn.needs.mood.DrawOnGUI(rect2, 2147483647, -1f, true, true);
			Rect rect3 = new Rect(0f, 80f, rect.width, (float)(rect.height - 70.0 - 10.0));
			rect3 = rect3.ContractedBy(10f);
			NeedsCardUtility.DrawThoughtListing(rect3, pawn, ref thoughtScrollPosition);
			GUI.EndGroup();
		}

		private static void UpdateDisplayNeeds(Pawn pawn)
		{
			NeedsCardUtility.displayNeeds.Clear();
			List<Need> allNeeds = pawn.needs.AllNeeds;
			for (int i = 0; i < allNeeds.Count; i++)
			{
				if (allNeeds[i].def.showOnNeedList)
				{
					NeedsCardUtility.displayNeeds.Add(allNeeds[i]);
				}
			}
			PawnNeedsUIUtility.SortInDisplayOrder(NeedsCardUtility.displayNeeds);
		}

		private static void DrawThoughtListing(Rect listingRect, Pawn pawn, ref Vector2 thoughtScrollPosition)
		{
			if (Event.current.type != EventType.Layout)
			{
				Text.Font = GameFont.Small;
				PawnNeedsUIUtility.GetThoughtGroupsInDisplayOrder(pawn.needs.mood, NeedsCardUtility.thoughtGroupsPresent);
				float height = (float)((float)NeedsCardUtility.thoughtGroupsPresent.Count * 24.0);
				Widgets.BeginScrollView(listingRect, ref thoughtScrollPosition, new Rect(0f, 0f, (float)(listingRect.width - 16.0), height), true);
				Text.Anchor = TextAnchor.MiddleLeft;
				float num = 0f;
				for (int i = 0; i < NeedsCardUtility.thoughtGroupsPresent.Count; i++)
				{
					Rect rect = new Rect(0f, num, (float)(listingRect.width - 16.0), 20f);
					if (NeedsCardUtility.DrawThoughtGroup(rect, NeedsCardUtility.thoughtGroupsPresent[i], pawn))
					{
						num = (float)(num + 24.0);
					}
				}
				Widgets.EndScrollView();
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}

		private static bool DrawThoughtGroup(Rect rect, Thought group, Pawn pawn)
		{
			try
			{
				pawn.needs.mood.thoughts.GetMoodThoughts(group, NeedsCardUtility.thoughtGroup);
				Thought leadingThoughtInGroup = PawnNeedsUIUtility.GetLeadingThoughtInGroup(NeedsCardUtility.thoughtGroup);
				if (!leadingThoughtInGroup.VisibleInNeedsTab)
				{
					NeedsCardUtility.thoughtGroup.Clear();
					return false;
				}
				if (leadingThoughtInGroup != NeedsCardUtility.thoughtGroup[0])
				{
					NeedsCardUtility.thoughtGroup.Remove(leadingThoughtInGroup);
					NeedsCardUtility.thoughtGroup.Insert(0, leadingThoughtInGroup);
				}
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(leadingThoughtInGroup.Description);
				if (group.def.DurationTicks > 5)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					Thought_Memory thought_Memory = leadingThoughtInGroup as Thought_Memory;
					if (thought_Memory != null)
					{
						if (NeedsCardUtility.thoughtGroup.Count == 1)
						{
							stringBuilder.Append("ThoughtExpiresIn".Translate((group.def.DurationTicks - thought_Memory.age).ToStringTicksToPeriod(true, false, true)));
						}
						else
						{
							Thought_Memory thought_Memory2 = (Thought_Memory)NeedsCardUtility.thoughtGroup[NeedsCardUtility.thoughtGroup.Count - 1];
							stringBuilder.Append("ThoughtStartsExpiringIn".Translate((group.def.DurationTicks - thought_Memory.age).ToStringTicksToPeriod(true, false, true)));
							stringBuilder.AppendLine();
							stringBuilder.Append("ThoughtFinishesExpiringIn".Translate((group.def.DurationTicks - thought_Memory2.age).ToStringTicksToPeriod(true, false, true)));
						}
					}
				}
				if (NeedsCardUtility.thoughtGroup.Count > 1)
				{
					bool flag = false;
					for (int i = 1; i < NeedsCardUtility.thoughtGroup.Count; i++)
					{
						bool flag2 = false;
						int num = 0;
						while (num < i)
						{
							if (!(NeedsCardUtility.thoughtGroup[i].LabelCap == NeedsCardUtility.thoughtGroup[num].LabelCap))
							{
								num++;
								continue;
							}
							flag2 = true;
							break;
						}
						if (!flag2)
						{
							if (!flag)
							{
								stringBuilder.AppendLine();
								stringBuilder.AppendLine();
								flag = true;
							}
							stringBuilder.AppendLine("+ " + NeedsCardUtility.thoughtGroup[i].LabelCap);
						}
					}
				}
				TooltipHandler.TipRegion(rect, new TipSignal(stringBuilder.ToString(), 7291));
				Text.WordWrap = false;
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = new Rect((float)(rect.x + 10.0), rect.y, 225f, rect.height);
				rect2.yMin -= 3f;
				rect2.yMax += 3f;
				string text = leadingThoughtInGroup.LabelCap;
				if (NeedsCardUtility.thoughtGroup.Count > 1)
				{
					text = text + " x" + NeedsCardUtility.thoughtGroup.Count;
				}
				Widgets.Label(rect2, text);
				Text.Anchor = TextAnchor.MiddleCenter;
				float num2 = pawn.needs.mood.thoughts.MoodOffsetOfGroup(group);
				if (num2 == 0.0)
				{
					GUI.color = NeedsCardUtility.NoEffectColor;
				}
				else if (num2 > 0.0)
				{
					GUI.color = NeedsCardUtility.MoodColor;
				}
				else
				{
					GUI.color = NeedsCardUtility.MoodColorNegative;
				}
				Rect rect3 = new Rect((float)(rect.x + 235.0), rect.y, 32f, rect.height);
				Widgets.Label(rect3, num2.ToString("##0"));
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
				Text.WordWrap = true;
			}
			catch (Exception ex)
			{
				Log.ErrorOnce("Exception in DrawThoughtGroup for " + group.def + " on " + pawn + ": " + ex.ToString(), 3452698);
			}
			return true;
		}
	}
}
