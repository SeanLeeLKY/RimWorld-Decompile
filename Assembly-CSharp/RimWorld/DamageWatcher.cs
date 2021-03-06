using Verse;

namespace RimWorld
{
	public class DamageWatcher : IExposable
	{
		private float everDamage;

		private float recentDamage;

		private int lastSeriousDamageTick;

		private const int UpdateInterval = 2000;

		private const float ColonistDamageFactor = 5f;

		private const float DamageFallPerInterval = 70f;

		public const float MaxRecentDamage = 8000f;

		private const float SeriousDamageThreshold = 7500f;

		public float DamageTakenEver
		{
			get
			{
				return this.everDamage;
			}
		}

		public float DamageTakenRecently
		{
			get
			{
				return this.recentDamage;
			}
		}

		public float DaysSinceSeriousDamage
		{
			get
			{
				return Find.TickManager.TicksGame.TicksToDays() - this.lastSeriousDamageTick.TicksToDays();
			}
		}

		public void Notify_DamageTaken(Thing damagee, float amount)
		{
			if (damagee.Faction == Faction.OfPlayer)
			{
				if (damagee.def.category == ThingCategory.Pawn && damagee.def.race.Humanlike)
				{
					amount = (float)(amount * 5.0);
				}
				this.recentDamage += amount;
				this.everDamage += amount;
				if (this.recentDamage > 8000.0)
				{
					this.recentDamage = 8000f;
				}
				if (this.recentDamage >= 7500.0)
				{
					this.lastSeriousDamageTick = Find.TickManager.TicksGame;
				}
			}
		}

		public void DamageWatcherTick()
		{
			if (Find.TickManager.TicksGame % 2000 == 0)
			{
				this.recentDamage -= 70f;
				if (this.recentDamage < 0.0)
				{
					this.recentDamage = 0f;
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<float>(ref this.everDamage, "everDamage", 0f, false);
			Scribe_Values.Look<float>(ref this.recentDamage, "recentDamage", 0f, false);
			Scribe_Values.Look<int>(ref this.lastSeriousDamageTick, "lastSeriousDamageTick", 0, false);
		}
	}
}
