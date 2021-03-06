using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TrainingCardUtility
	{
		private const float RowHeight = 28f;

		private const float InfoHeaderHeight = 50f;

		private static readonly Texture2D TrainedTrainableTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheck", true);

		public static void DrawTrainingCard(Rect rect, Pawn pawn)
		{
			GUI.BeginGroup(rect);
			string label = "TrainableIntelligence".Translate() + ": " + pawn.RaceProps.TrainableIntelligence.label;
			Widgets.Label(new Rect(0f, 0f, rect.width, 25f), label);
			if (pawn.training.IsCompleted(TrainableDefOf.Obedience))
			{
				Rect rect2 = new Rect(0f, 20f, rect.width, 25f);
				Widgets.Label(rect2, "Master".Translate() + ": ");
				Vector2 center = rect2.center;
				rect2.xMin = center.x;
				string label2 = TrainableUtility.MasterString(pawn);
				if (Widgets.ButtonText(rect2, label2, true, false, true))
				{
					TrainableUtility.OpenMasterSelectMenu(pawn);
				}
			}
			float num = 50f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (TrainingCardUtility.TryDrawTrainableRow(new Rect(0f, num, rect.width, 28f), pawn, trainableDefsInListOrder[i]))
				{
					num = (float)(num + 28.0);
				}
			}
			GUI.EndGroup();
		}

		private static bool TryDrawTrainableRow(Rect rect, Pawn pawn, TrainableDef td)
		{
			bool flag = pawn.training.IsCompleted(td);
			bool flag2 = default(bool);
			AcceptanceReport canTrain = pawn.training.CanAssignToTrain(td, out flag2);
			if (!flag2)
			{
				return false;
			}
			Widgets.DrawHighlightIfMouseover(rect);
			Rect rect2 = rect;
			rect2.width -= 50f;
			rect2.xMin += (float)((float)td.indent * 10.0);
			Rect rect3 = rect;
			rect3.xMin = (float)(rect3.xMax - 50.0 + 17.0);
			if (!flag)
			{
				TrainingCardUtility.DoTrainableCheckbox(rect2, pawn, td, canTrain, true, false);
			}
			else
			{
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect2, td.LabelCap);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			if (flag)
			{
				GUI.color = Color.green;
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect3, pawn.training.GetSteps(td) + " / " + td.steps);
			Text.Anchor = TextAnchor.UpperLeft;
			if (DebugSettings.godMode && !pawn.training.IsCompleted(td))
			{
				Rect rect4 = rect3;
				rect4.yMin = (float)(rect4.yMax - 10.0);
				rect4.xMin = (float)(rect4.xMax - 10.0);
				if (Widgets.ButtonText(rect4, "+", true, false, true))
				{
					pawn.training.Train(td, pawn.Map.mapPawns.FreeColonistsSpawned.RandomElement());
				}
			}
			TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
			GUI.color = Color.white;
			return true;
		}

		public static void DoTrainableCheckbox(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain, bool drawLabel, bool doTooltip)
		{
			if (pawn.training.IsCompleted(td))
			{
				if (!drawLabel)
				{
					GUI.DrawTexture(rect, TrainingCardUtility.TrainedTrainableTex);
				}
			}
			else
			{
				bool wanted = pawn.training.GetWanted(td);
				bool flag = wanted;
				if (drawLabel)
				{
					Widgets.CheckboxLabeled(rect, td.LabelCap, ref wanted, !canTrain.Accepted);
				}
				else
				{
					Widgets.Checkbox(rect.position, ref wanted, rect.width, !canTrain.Accepted);
				}
				if (wanted != flag)
				{
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTraining, KnowledgeAmount.Total);
					pawn.training.SetWantedRecursive(td, wanted);
				}
			}
			if (doTooltip)
			{
				TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
			}
		}

		private static void DoTrainableTooltip(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain)
		{
			TooltipHandler.TipRegion(rect, delegate
			{
				string text = td.LabelCap + "\n\n" + td.description;
				if (!canTrain.Accepted)
				{
					text = text + "\n\n" + canTrain.Reason;
				}
				else if (!td.prerequisites.NullOrEmpty())
				{
					text += "\n";
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						if (!pawn.training.IsCompleted(td.prerequisites[i]))
						{
							text = text + "\n" + "TrainingNeedsPrerequisite".Translate(td.prerequisites[i].LabelCap);
						}
					}
				}
				return text;
			}, (int)(rect.y * 612.0 + rect.x));
		}
	}
}
