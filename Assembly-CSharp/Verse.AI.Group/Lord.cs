using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse.AI.Group
{
	[StaticConstructorOnStartup]
	public class Lord : IExposable, ILoadReferenceable
	{
		public LordManager lordManager;

		private LordToil curLordToil;

		private StateGraph graph;

		public int loadID = -1;

		private LordJob curJob;

		public Faction faction;

		public List<Pawn> ownedPawns = new List<Pawn>();

		public List<Thing> extraForbiddenThings = new List<Thing>();

		private bool initialized;

		public int ticksInToil;

		public int numPawnsLostViolently;

		public int numPawnsEverGained;

		public int initialColonyHealthTotal;

		public int lastPawnHarmTick = -99999;

		private const int AttackTargetCacheInterval = 60;

		private static readonly Material FlagTex = MaterialPool.MatFrom("UI/Overlays/SquadFlag");

		private int tmpCurLordToilIdx = -1;

		private Dictionary<int, LordToilData> tmpLordToilData = new Dictionary<int, LordToilData>();

		private Dictionary<int, TriggerData> tmpTriggerData = new Dictionary<int, TriggerData>();

		public Map Map
		{
			get
			{
				return this.lordManager.map;
			}
		}

		public StateGraph Graph
		{
			get
			{
				return this.graph;
			}
		}

		public LordToil CurLordToil
		{
			get
			{
				return this.curLordToil;
			}
		}

		public LordJob LordJob
		{
			get
			{
				return this.curJob;
			}
		}

		private bool CanExistWithoutPawns
		{
			get
			{
				return this.curJob is LordJob_VoluntarilyJoinable;
			}
		}

		private void Init()
		{
			this.initialized = true;
			this.initialColonyHealthTotal = this.Map.wealthWatcher.HealthTotal;
		}

		public string GetUniqueLoadID()
		{
			return "Lord_" + this.loadID;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Collections.Look<Thing>(ref this.extraForbiddenThings, "extraForbiddenThings", LookMode.Reference, new object[0]);
			Scribe_Collections.Look<Pawn>(ref this.ownedPawns, "ownedPawns", LookMode.Reference, new object[0]);
			Scribe_Deep.Look<LordJob>(ref this.curJob, "lordJob", new object[0]);
			Scribe_Values.Look<bool>(ref this.initialized, "initialized", true, false);
			Scribe_Values.Look<int>(ref this.ticksInToil, "ticksInToil", 0, false);
			Scribe_Values.Look<int>(ref this.numPawnsEverGained, "numPawnsEverGained", 0, false);
			Scribe_Values.Look<int>(ref this.numPawnsLostViolently, "numPawnsLostViolently", 0, false);
			Scribe_Values.Look<int>(ref this.initialColonyHealthTotal, "initialColonyHealthTotal", 0, false);
			Scribe_Values.Look<int>(ref this.lastPawnHarmTick, "lastPawnHarmTick", -99999, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.extraForbiddenThings.RemoveAll((Thing x) => x == null);
			}
			this.ExposeData_StateGraph();
		}

		private void ExposeData_StateGraph()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.tmpLordToilData.Clear();
				for (int i = 0; i < this.graph.lordToils.Count; i++)
				{
					if (this.graph.lordToils[i].data != null)
					{
						this.tmpLordToilData.Add(i, this.graph.lordToils[i].data);
					}
				}
				this.tmpTriggerData.Clear();
				int num = 0;
				for (int j = 0; j < this.graph.transitions.Count; j++)
				{
					for (int k = 0; k < this.graph.transitions[j].triggers.Count; k++)
					{
						if (this.graph.transitions[j].triggers[k].data != null)
						{
							this.tmpTriggerData.Add(num, this.graph.transitions[j].triggers[k].data);
						}
						num++;
					}
				}
				this.tmpCurLordToilIdx = this.graph.lordToils.IndexOf(this.curLordToil);
			}
			Scribe_Collections.Look<int, LordToilData>(ref this.tmpLordToilData, "lordToilData", LookMode.Value, LookMode.Deep);
			Scribe_Collections.Look<int, TriggerData>(ref this.tmpTriggerData, "triggerData", LookMode.Value, LookMode.Deep);
			Scribe_Values.Look<int>(ref this.tmpCurLordToilIdx, "curLordToilIdx", -1, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.curJob.LostImportantReferenceDuringLoading)
				{
					this.lordManager.RemoveLord(this);
				}
				else
				{
					LordJob job = this.curJob;
					this.curJob = null;
					this.SetJob(job);
					foreach (KeyValuePair<int, LordToilData> tmpLordToilDatum in this.tmpLordToilData)
					{
						if (tmpLordToilDatum.Key < 0 || tmpLordToilDatum.Key >= this.graph.lordToils.Count)
						{
							Log.Error("Could not find lord toil for lord toil data of type \"" + tmpLordToilDatum.Value.GetType() + "\" (lord job: \"" + this.curJob.GetType() + "\"), because lord toil index is out of bounds: " + tmpLordToilDatum.Key);
						}
						else
						{
							this.graph.lordToils[tmpLordToilDatum.Key].data = tmpLordToilDatum.Value;
						}
					}
					this.tmpLordToilData.Clear();
					foreach (KeyValuePair<int, TriggerData> tmpTriggerDatum in this.tmpTriggerData)
					{
						Trigger triggerByIndex = this.GetTriggerByIndex(tmpTriggerDatum.Key);
						if (triggerByIndex == null)
						{
							Log.Error("Could not find trigger for trigger data of type \"" + tmpTriggerDatum.Value.GetType() + "\" (lord job: \"" + this.curJob.GetType() + "\"), because trigger index is out of bounds: " + tmpTriggerDatum.Key);
						}
						else
						{
							triggerByIndex.data = tmpTriggerDatum.Value;
						}
					}
					this.tmpTriggerData.Clear();
					if (this.tmpCurLordToilIdx < 0 || this.tmpCurLordToilIdx >= this.graph.lordToils.Count)
					{
						Log.Error("Current lord toil index out of bounds (lord job: \"" + this.curJob.GetType() + "\"): " + this.tmpCurLordToilIdx);
					}
					else
					{
						this.curLordToil = this.graph.lordToils[this.tmpCurLordToilIdx];
					}
				}
			}
		}

		public void SetJob(LordJob lordJob)
		{
			if (this.curJob != null)
			{
				this.curJob.Cleanup();
			}
			this.curJob = lordJob;
			this.curLordToil = null;
			lordJob.lord = this;
			Rand.PushState();
			Rand.Seed = this.loadID * 193;
			this.graph = lordJob.CreateGraph();
			Rand.PopState();
			this.graph.ErrorCheck();
			if (this.faction != null && this.faction.def.autoFlee)
			{
				LordToil_PanicFlee lordToil_PanicFlee = new LordToil_PanicFlee();
				lordToil_PanicFlee.avoidGridMode = AvoidGridMode.Smart;
				for (int i = 0; i < this.graph.lordToils.Count; i++)
				{
					Transition transition = new Transition(this.graph.lordToils[i], lordToil_PanicFlee);
					transition.AddPreAction(new TransitionAction_Message("MessageFightersFleeing".Translate(this.faction.def.pawnsPlural.CapitalizeFirst(), this.faction.Name)));
					transition.AddTrigger(new Trigger_FractionPawnsLost(0.5f));
					this.graph.AddTransition(transition);
				}
				this.graph.AddToil(lordToil_PanicFlee);
			}
			for (int j = 0; j < this.graph.lordToils.Count; j++)
			{
				this.graph.lordToils[j].lord = this;
			}
			for (int k = 0; k < this.ownedPawns.Count; k++)
			{
				this.Map.attackTargetsCache.UpdateTarget(this.ownedPawns[k]);
			}
		}

		public void Cleanup()
		{
			this.curJob.Cleanup();
			if (this.curLordToil != null)
			{
				this.curLordToil.Cleanup();
			}
			for (int i = 0; i < this.ownedPawns.Count; i++)
			{
				if (this.ownedPawns[i].mindState != null)
				{
					this.ownedPawns[i].mindState.duty = null;
				}
				this.Map.attackTargetsCache.UpdateTarget(this.ownedPawns[i]);
				if (this.ownedPawns[i].Spawned && this.ownedPawns[i].CurJob != null)
				{
					this.ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				}
			}
		}

		public void AddPawn(Pawn p)
		{
			if (this.ownedPawns.Contains(p))
			{
				Log.Error("Lord for " + this.faction.ToStringSafe() + " tried to add " + p + " whom it already controls.");
			}
			else if (p.GetLord() != null)
			{
				Log.Error("Tried to add pawn " + p + " to lord " + this + " but this pawn is already a member of lord " + p.GetLord() + ". Pawns can't be members of more than one lord at the same time.");
			}
			else
			{
				this.ownedPawns.Add(p);
				this.numPawnsEverGained++;
				this.Map.attackTargetsCache.UpdateTarget(p);
				this.curLordToil.UpdateAllDuties();
				this.curJob.Notify_PawnAdded(p);
			}
		}

		private void RemovePawn(Pawn p)
		{
			this.ownedPawns.Remove(p);
			if (p.mindState != null)
			{
				p.mindState.duty = null;
			}
			this.Map.attackTargetsCache.UpdateTarget(p);
		}

		public void GotoToil(LordToil newLordToil)
		{
			LordToil previousToil = this.curLordToil;
			if (this.curLordToil != null)
			{
				this.curLordToil.Cleanup();
			}
			this.curLordToil = newLordToil;
			this.ticksInToil = 0;
			if (this.curLordToil.lord != this)
			{
				Log.Error("curLordToil lord is " + ((this.curLordToil.lord != null) ? this.curLordToil.lord.ToString() : "null (forgot to add toil to graph?)"));
				this.curLordToil.lord = this;
			}
			this.curLordToil.Init();
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				if (this.graph.transitions[i].sources.Contains(this.curLordToil))
				{
					this.graph.transitions[i].SourceToilBecameActive(this.graph.transitions[i], previousToil);
				}
			}
			this.curLordToil.UpdateAllDuties();
		}

		public void LordTick()
		{
			if (!this.initialized)
			{
				this.Init();
			}
			this.curLordToil.LordToilTick();
			this.CheckTransitionOnSignal(TriggerSignal.ForTick);
			this.ticksInToil++;
		}

		private Trigger GetTriggerByIndex(int index)
		{
			int num = 0;
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				for (int j = 0; j < this.graph.transitions[i].triggers.Count; j++)
				{
					if (num == index)
					{
						return this.graph.transitions[i].triggers[j];
					}
					num++;
				}
			}
			return null;
		}

		public void ReceiveMemo(string memo)
		{
			this.CheckTransitionOnSignal(TriggerSignal.ForMemo(memo));
		}

		public void Notify_FactionRelationsChanged(Faction otherFaction)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.FactionRelationsChanged;
			signal.faction = otherFaction;
			this.CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnLost(Pawn pawn, PawnLostCondition cond)
		{
			if (this.ownedPawns.Contains(pawn))
			{
				this.RemovePawn(pawn);
				if (cond == PawnLostCondition.IncappedOrKilled || cond == PawnLostCondition.MadePrisoner)
				{
					this.numPawnsLostViolently++;
				}
				if (this.ownedPawns.Count == 0 && !this.CanExistWithoutPawns)
				{
					this.lordManager.RemoveLord(this);
				}
				else
				{
					this.curLordToil.Notify_PawnLost(pawn, cond);
					this.curJob.Notify_PawnLost(pawn, cond);
					TriggerSignal signal = default(TriggerSignal);
					signal.type = TriggerSignalType.PawnLost;
					signal.thing = pawn;
					signal.condition = cond;
					this.CheckTransitionOnSignal(signal);
				}
			}
			else
			{
				Log.Error("Lord lost pawn " + pawn + " it didn't have. Condition=" + cond);
			}
		}

		public void Notify_BuildingDamaged(Building building, DamageInfo dinfo)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.BuildingDamaged;
			signal.thing = building;
			signal.dinfo = dinfo;
			this.CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnDamaged(Pawn victim, DamageInfo dinfo)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.PawnDamaged;
			signal.thing = victim;
			signal.dinfo = dinfo;
			this.CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnAttemptArrested(Pawn victim)
		{
			TriggerSignal signal = default(TriggerSignal);
			signal.type = TriggerSignalType.PawnArrestAttempted;
			signal.thing = victim;
			this.CheckTransitionOnSignal(signal);
		}

		public void Notify_PawnAcquiredTarget(Pawn detector, Thing newTarg)
		{
		}

		public void Notify_ReachedDutyLocation(Pawn pawn)
		{
			this.curLordToil.Notify_ReachedDutyLocation(pawn);
		}

		public void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
		{
			this.curLordToil.Notify_ConstructionFailed(pawn, frame, newBlueprint);
		}

		private bool CheckTransitionOnSignal(TriggerSignal signal)
		{
			if (Trigger_PawnHarmed.SignalIsHarm(signal))
			{
				this.lastPawnHarmTick = Find.TickManager.TicksGame;
			}
			for (int i = 0; i < this.graph.transitions.Count; i++)
			{
				if (this.graph.transitions[i].sources.Contains(this.curLordToil) && this.graph.transitions[i].CheckSignal(this, signal))
				{
					return true;
				}
			}
			return false;
		}

		private Vector3 DebugCenter()
		{
			Vector3 result = this.Map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			if ((from p in this.ownedPawns
			where p.Spawned
			select p).Any())
			{
				result.x = (from p in this.ownedPawns
				where p.Spawned
				select p).Average(delegate(Pawn p)
				{
					Vector3 drawPos2 = p.DrawPos;
					return drawPos2.x;
				});
				result.z = (from p in this.ownedPawns
				where p.Spawned
				select p).Average(delegate(Pawn p)
				{
					Vector3 drawPos = p.DrawPos;
					return drawPos.z;
				});
			}
			return result;
		}

		public void DebugDraw()
		{
			Vector3 a = this.DebugCenter();
			IntVec3 flagLoc = this.curLordToil.FlagLoc;
			if (flagLoc.IsValid)
			{
				Graphics.DrawMesh(MeshPool.plane14, flagLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Building), Quaternion.identity, Lord.FlagTex, 0);
			}
			GenDraw.DrawLineBetween(a, flagLoc.ToVector3Shifted(), SimpleColor.Red);
			foreach (Pawn ownedPawn in this.ownedPawns)
			{
				SimpleColor color = (SimpleColor)(ownedPawn.InMentalState ? 5 : 0);
				GenDraw.DrawLineBetween(a, ownedPawn.DrawPos, color);
			}
		}

		public void DebugOnGUI()
		{
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			string label = (this.CurLordToil == null) ? "toil=NULL" : ("toil " + this.graph.lordToils.IndexOf(this.CurLordToil) + "\n" + this.CurLordToil.ToString());
			Vector2 vector = this.DebugCenter().MapToUIPosition();
			Widgets.Label(new Rect((float)(vector.x - 100.0), (float)(vector.y - 100.0), 200f, 200f), label);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Start steal threshold: " + StealAIUtility.StartStealingMarketValueThreshold(this).ToString("F0"));
			stringBuilder.AppendLine("Duties:");
			foreach (Pawn ownedPawn in this.ownedPawns)
			{
				stringBuilder.AppendLine("   " + ownedPawn.LabelCap + " - " + ownedPawn.mindState.duty);
			}
			if (this.faction != null)
			{
				stringBuilder.AppendLine("Faction data:");
				stringBuilder.AppendLine(this.faction.DebugString());
			}
			stringBuilder.AppendLine("Raw save data:");
			stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(this));
			return stringBuilder.ToString();
		}

		private bool ShouldDoDebugOutput()
		{
			IntVec3 a = UI.MouseCell();
			IntVec3 flagLoc = this.curLordToil.FlagLoc;
			if (flagLoc.IsValid && a == flagLoc)
			{
				return true;
			}
			for (int i = 0; i < this.ownedPawns.Count; i++)
			{
				if (a == this.ownedPawns[i].Position)
				{
					return true;
				}
			}
			return false;
		}
	}
}
