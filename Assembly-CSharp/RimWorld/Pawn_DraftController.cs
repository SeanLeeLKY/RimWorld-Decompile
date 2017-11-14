using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_DraftController : IExposable
	{
		public Pawn pawn;

		private bool draftedInt;

		private bool fireAtWillInt = true;

		private AutoUndrafter autoUndrafter;

		public bool Drafted
		{
			get
			{
				return this.draftedInt;
			}
			set
			{
				if (value != this.draftedInt)
				{
					this.pawn.mindState.priorityWork.ClearPrioritizedWorkAndJobQueue();
					this.fireAtWillInt = true;
					this.draftedInt = value;
					if (!value && this.pawn.Spawned)
					{
						this.pawn.Map.pawnDestinationReservationManager.ReleaseAllClaimedBy(this.pawn);
					}
					if (this.pawn.jobs.curJob != null && this.pawn.jobs.IsCurrentJobPlayerInterruptible())
					{
						this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
					}
					if (this.draftedInt)
					{
						foreach (Pawn item in PawnUtility.SpawnedMasteredPawns(this.pawn))
						{
							item.jobs.Notify_MasterDrafted();
						}
						Lord lord = this.pawn.GetLord();
						if (lord != null && lord.LordJob is LordJob_VoluntarilyJoinable)
						{
							lord.Notify_PawnLost(this.pawn, PawnLostCondition.Drafted);
						}
						this.autoUndrafter.Notify_Drafted();
					}
					else if (this.pawn.playerSettings != null)
					{
						this.pawn.playerSettings.animalsReleased = false;
					}
				}
			}
		}

		public bool FireAtWill
		{
			get
			{
				return this.fireAtWillInt;
			}
			set
			{
				this.fireAtWillInt = value;
				if (!this.fireAtWillInt && this.pawn.stances.curStance is Stance_Warmup)
				{
					this.pawn.stances.CancelBusyStanceSoft();
				}
			}
		}

		public Pawn_DraftController(Pawn pawn)
		{
			this.pawn = pawn;
			this.autoUndrafter = new AutoUndrafter(pawn);
		}

		public void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.draftedInt, "drafted", false, false);
			Scribe_Values.Look<bool>(ref this.fireAtWillInt, "fireAtWill", true, false);
			Scribe_Deep.Look<AutoUndrafter>(ref this.autoUndrafter, "autoUndrafter", new object[1]
			{
				this.pawn
			});
		}

		public void DraftControllerTick()
		{
			this.autoUndrafter.AutoUndraftTick();
		}

		internal IEnumerable<Gizmo> GetGizmos()
		{
			Command_Toggle draft = new Command_Toggle
			{
				hotKey = KeyBindingDefOf.CommandColonistDraft,
				isActive = this.get_Drafted,
				toggleAction = delegate
				{
					((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0062: stateMachine*/)._0024this.Drafted = !((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0062: stateMachine*/)._0024this.Drafted;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
				},
				defaultDesc = "CommandToggleDraftDesc".Translate(),
				icon = TexCommand.Draft,
				turnOnSound = SoundDefOf.DraftOn,
				turnOffSound = SoundDefOf.DraftOff
			};
			if (!this.Drafted)
			{
				draft.defaultLabel = "CommandDraftLabel".Translate();
			}
			if (this.pawn.Downed)
			{
				draft.Disable("IsIncapped".Translate(this.pawn.NameStringShort));
			}
			if (!this.Drafted)
			{
				draft.tutorTag = "Draft";
			}
			else
			{
				draft.tutorTag = "Undraft";
			}
			yield return (Gizmo)draft;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		internal void Notify_PrimaryWeaponChanged()
		{
			this.fireAtWillInt = true;
		}
	}
}
