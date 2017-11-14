using Verse;

namespace RimWorld.Planet
{
	public class CaravansBattlefield : MapParent
	{
		private bool wonBattle;

		public bool WonBattle
		{
			get
			{
				return this.wonBattle;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				alsoRemoveWorldObject = true;
				return true;
			}
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void Tick()
		{
			base.Tick();
			if (base.HasMap)
			{
				this.CheckWonBattle();
			}
		}

		private void CheckWonBattle()
		{
			if (!this.wonBattle && !GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				Messages.Message("MessageAmbushVictory".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(60000)), this, MessageTypeDefOf.PositiveEvent);
				TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushDefeated, base.Map.mapPawns.FreeColonists.RandomElement());
				this.wonBattle = true;
				base.GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown();
			}
		}
	}
}
