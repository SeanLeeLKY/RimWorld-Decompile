using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ITab_Pawn_Visitor : ITab
	{
		private const float CheckboxInterval = 30f;

		private const float CheckboxMargin = 50f;

		public ITab_Pawn_Visitor()
		{
			base.size = new Vector2(280f, 450f);
		}

		protected override void FillTab()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.FrameDisplayed);
			Text.Font = GameFont.Small;
			Rect rect = new Rect(0f, 0f, base.size.x, base.size.y);
			Rect rect2 = rect.ContractedBy(10f);
			rect2.yMin += 24f;
			bool isPrisonerOfColony = base.SelPawn.IsPrisonerOfColony;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect2);
			Rect rect3 = listing_Standard.GetRect(Text.LineHeight);
			rect3.width *= 0.75f;
			bool getsFood = base.SelPawn.guest.GetsFood;
			Widgets.CheckboxLabeled(rect3, "GetsFood".Translate(), ref getsFood, false);
			base.SelPawn.guest.GetsFood = getsFood;
			listing_Standard.Gap(12f);
			Rect rect4 = listing_Standard.GetRect(28f);
			rect4.width = 140f;
			MedicalCareUtility.MedicalCareSetter(rect4, ref base.SelPawn.playerSettings.medCare);
			listing_Standard.Gap(4f);
			if (isPrisonerOfColony)
			{
				listing_Standard.Label("RecruitmentDifficulty".Translate() + ": " + base.SelPawn.RecruitDifficulty(Faction.OfPlayer, false).ToStringPercent(), -1f);
				if (base.SelPawn.guilt.IsGuilty)
				{
					listing_Standard.Label("ConsideredGuilty".Translate(base.SelPawn.guilt.TicksUntilInnocent.ToStringTicksToPeriod(true, false, true)), -1f);
				}
				if (Prefs.DevMode)
				{
					listing_Standard.Label("Dev: Prison break MTB days: " + (int)PrisonBreakUtility.InitiatePrisonBreakMtbDays(base.SelPawn), -1f);
				}
				Rect rect5 = listing_Standard.GetRect(200f).Rounded();
				Widgets.DrawMenuSection(rect5);
				Rect position = rect5.ContractedBy(10f);
				GUI.BeginGroup(position);
				Rect rect6 = new Rect(0f, 0f, position.width, 30f);
				foreach (PrisonerInteractionModeDef item in from pim in DefDatabase<PrisonerInteractionModeDef>.AllDefs
				orderby pim.listOrder
				select pim)
				{
					if (Widgets.RadioButtonLabeled(rect6, item.LabelCap, base.SelPawn.guest.interactionMode == item))
					{
						base.SelPawn.guest.interactionMode = item;
						if (item == PrisonerInteractionModeDefOf.Execution && base.SelPawn.MapHeld != null && !this.ColonyHasAnyWardenCapableOfViolence(base.SelPawn.MapHeld))
						{
							Messages.Message("MessageCantDoExecutionBecauseNoWardenCapableOfViolence".Translate(), base.SelPawn, MessageTypeDefOf.CautionInput);
						}
					}
					rect6.y += 28f;
				}
				GUI.EndGroup();
			}
			listing_Standard.End();
		}

		private bool ColonyHasAnyWardenCapableOfViolence(Map map)
		{
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.workSettings.WorkIsActive(WorkTypeDefOf.Warden) && (item.story == null || !item.story.WorkTagIsDisabled(WorkTags.Violent)))
				{
					return true;
				}
			}
			return false;
		}
	}
}
