using UnityEngine;

namespace Verse
{
	public abstract class Mote : Thing
	{
		public Vector3 exactPosition;

		public float exactRotation;

		public Vector3 exactScale = new Vector3(1f, 1f, 1f);

		public float rotationRate;

		public Color instanceColor = Color.white;

		private int lastMaintainTick;

		public int spawnTick;

		public float spawnRealTime;

		public MoteAttachLink link1 = MoteAttachLink.Invalid;

		protected float skidSpeedMultiplierPerTick = Rand.Range(0.3f, 0.95f);

		protected const float MinSpeed = 0.02f;

		public float Scale
		{
			set
			{
				this.exactScale = new Vector3(value, 1f, value);
			}
		}

		public float AgeSecs
		{
			get
			{
				if (base.def.mote.realTime)
				{
					return Time.realtimeSinceStartup - this.spawnRealTime;
				}
				return (float)((float)(Find.TickManager.TicksGame - this.spawnTick) / 60.0);
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				return this.exactPosition;
			}
		}

		protected virtual float LifespanSecs
		{
			get
			{
				return base.def.mote.fadeInTime + base.def.mote.solidTime + base.def.mote.fadeOutTime;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.spawnTick = Find.TickManager.TicksGame;
			this.spawnRealTime = Time.realtimeSinceStartup;
			RealTime.moteList.MoteSpawned(this);
			base.Map.moteCounter.Notify_MoteSpawned();
			this.exactPosition.y = Altitudes.AltitudeFor(base.def.altitudeLayer);
		}

		public override void DeSpawn()
		{
			Map map = base.Map;
			base.DeSpawn();
			RealTime.moteList.MoteDespawned(this);
			map.moteCounter.Notify_MoteDespawned();
		}

		public override void Tick()
		{
			if (!base.def.mote.realTime)
			{
				this.TimeInterval(0.0166666675f);
			}
		}

		public void RealtimeUpdate()
		{
			if (base.def.mote.realTime)
			{
				this.TimeInterval(Time.deltaTime);
			}
		}

		protected virtual void TimeInterval(float deltaTime)
		{
			if (this.AgeSecs >= this.LifespanSecs && !base.Destroyed)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			else if (base.def.mote.needsMaintenance && Find.TickManager.TicksGame - 1 > this.lastMaintainTick)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			else if (base.def.mote.growthRate > 0.0)
			{
				this.exactScale = new Vector3(this.exactScale.x + base.def.mote.growthRate * deltaTime, this.exactScale.y, this.exactScale.z + base.def.mote.growthRate * deltaTime);
			}
		}

		public override void Draw()
		{
			this.exactPosition.y = Altitudes.AltitudeFor(base.def.altitudeLayer);
			base.Draw();
		}

		public void Maintain()
		{
			this.lastMaintainTick = Find.TickManager.TicksGame;
		}

		public void Attach(TargetInfo a)
		{
			this.link1 = new MoteAttachLink(a);
		}

		public override void Notify_MyMapRemoved()
		{
			base.Notify_MyMapRemoved();
			RealTime.moteList.MoteDespawned(this);
		}
	}
}
