using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class GameConditionManager : IExposable
	{
		public Map map;

		private List<GameCondition> activeConditions = new List<GameCondition>();

		public List<GameCondition> ActiveConditions
		{
			get
			{
				return this.activeConditions;
			}
		}

		public GameConditionManager Parent
		{
			get
			{
				if (this.map == null)
				{
					return null;
				}
				return Find.World.gameConditionManager;
			}
		}

		public GameConditionManager(Map map)
		{
			this.map = map;
		}

		public void RegisterCondition(GameCondition cond)
		{
			this.activeConditions.Add(cond);
			cond.gameConditionManager = this;
			cond.Init();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<GameCondition>(ref this.activeConditions, "activeConditions", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < this.activeConditions.Count; i++)
				{
					this.activeConditions[i].gameConditionManager = this;
				}
			}
		}

		public void GameConditionManagerTick()
		{
			for (int num = this.activeConditions.Count - 1; num >= 0; num--)
			{
				GameCondition gameCondition = this.activeConditions[num];
				if (gameCondition.Expired)
				{
					gameCondition.End();
				}
				else
				{
					gameCondition.GameConditionTick();
				}
			}
		}

		public void GameConditionManagerDraw()
		{
			for (int num = this.activeConditions.Count - 1; num >= 0; num--)
			{
				this.activeConditions[num].GameConditionDraw();
			}
			if (this.Parent != null)
			{
				this.Parent.GameConditionManagerDraw();
			}
		}

		public bool ConditionIsActive(GameConditionDef def)
		{
			return this.GetActiveCondition(def) != null;
		}

		public GameCondition GetActiveCondition(GameConditionDef def)
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				if (def == this.activeConditions[i].def)
				{
					return this.activeConditions[i];
				}
			}
			if (this.Parent != null)
			{
				return this.Parent.GetActiveCondition(def);
			}
			return null;
		}

		public T GetActiveCondition<T>() where T : GameCondition
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				T val = (T)(this.activeConditions[i] as T);
				if (val != null)
				{
					return val;
				}
			}
			if (this.Parent != null)
			{
				return this.Parent.GetActiveCondition<T>();
			}
			return (T)null;
		}

		public float TotalHeightAt(float width)
		{
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num += Text.CalcHeight(this.activeConditions[i].LabelCap, width);
			}
			if (this.Parent != null)
			{
				num += this.Parent.TotalHeightAt(width);
			}
			return num;
		}

		public void DoConditionsUI(Rect rect)
		{
			GUI.BeginGroup(rect);
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				float width = (float)(rect.width - 15.0);
				Rect rect2 = new Rect(0f, num, width, Text.CalcHeight(this.activeConditions[i].LabelCap, width));
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleRight;
				Widgets.Label(rect2, this.activeConditions[i].LabelCap);
				GameCondition localCond = this.activeConditions[i];
				TooltipHandler.TipRegion(rect2, () => localCond.TooltipString, i * 631);
				num += rect2.height;
			}
			rect.yMin += num;
			GUI.EndGroup();
			Text.Anchor = TextAnchor.UpperLeft;
			if (this.Parent != null)
			{
				this.Parent.DoConditionsUI(rect);
			}
		}

		internal float AggregateSkyTargetLerpFactor()
		{
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num = (float)(num + (1.0 - num) * this.activeConditions[i].SkyTargetLerpFactor());
			}
			if (this.Parent != null)
			{
				num += this.Parent.AggregateSkyTargetLerpFactor();
			}
			return Mathf.Clamp01(num);
		}

		internal SkyTarget? AggregateSkyTarget()
		{
			SkyTarget value = default(SkyTarget);
			float num = 0f;
			this.AggregateSkyTargetWorker(ref value, ref num);
			if (num == 0.0)
			{
				return null;
			}
			return value;
		}

		private void AggregateSkyTargetWorker(ref SkyTarget total, ref float lfTotal)
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				GameCondition gameCondition = this.activeConditions[i];
				float num = gameCondition.SkyTargetLerpFactor();
				if (num > 0.0)
				{
					if (lfTotal == 0.0)
					{
						total = gameCondition.SkyTarget().Value;
						lfTotal = num;
					}
					else
					{
						lfTotal += num;
						total = SkyTarget.LerpDarken(total, gameCondition.SkyTarget().Value, num / lfTotal);
					}
				}
			}
			if (this.Parent != null)
			{
				this.Parent.AggregateSkyTargetWorker(ref total, ref lfTotal);
			}
		}

		internal float AggregateTemperatureOffset()
		{
			float num = 0f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num += this.activeConditions[i].TemperatureOffset();
			}
			if (this.Parent != null)
			{
				num += this.Parent.AggregateTemperatureOffset();
			}
			return num;
		}

		internal float AggregateAnimalDensityFactor()
		{
			float num = 1f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num *= this.activeConditions[i].AnimalDensityFactor();
			}
			if (this.Parent != null)
			{
				num += this.Parent.AggregateAnimalDensityFactor();
			}
			return num;
		}

		internal float AggregatePlantDensityFactor()
		{
			float num = 1f;
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				num *= this.activeConditions[i].PlantDensityFactor();
			}
			if (this.Parent != null)
			{
				num += this.Parent.AggregatePlantDensityFactor();
			}
			return num;
		}

		internal bool AllowEnjoyableOutsideNow()
		{
			GameConditionDef gameConditionDef = default(GameConditionDef);
			return this.AllowEnjoyableOutsideNow(out gameConditionDef);
		}

		internal bool AllowEnjoyableOutsideNow(out GameConditionDef reason)
		{
			for (int i = 0; i < this.activeConditions.Count; i++)
			{
				GameCondition gameCondition = this.activeConditions[i];
				if (!gameCondition.AllowEnjoyableOutsideNow())
				{
					reason = gameCondition.def;
					return false;
				}
			}
			reason = null;
			if (this.Parent != null)
			{
				return this.Parent.AllowEnjoyableOutsideNow(out reason);
			}
			return true;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GameCondition activeCondition in this.activeConditions)
			{
				stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(activeCondition));
			}
			return stringBuilder.ToString();
		}
	}
}
