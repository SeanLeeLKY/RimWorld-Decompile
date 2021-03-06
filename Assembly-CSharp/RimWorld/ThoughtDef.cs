using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtDef : Def
	{
		public Type thoughtClass;

		public Type workerClass;

		public List<ThoughtStage> stages = new List<ThoughtStage>();

		public int stackLimit = 1;

		public float stackedEffectMultiplier = 0.75f;

		public float durationDays;

		public bool invert;

		public bool validWhileDespawned;

		public ThoughtDef nextThought;

		public List<TraitDef> nullifyingTraits;

		public List<TaleDef> nullifyingOwnTales;

		public List<TraitDef> requiredTraits;

		public int requiredTraitsDegree = -2147483648;

		public StatDef effectMultiplyingStat;

		public HediffDef hediff;

		public GameConditionDef gameCondition;

		public bool nullifiedIfNotColonist;

		public ThoughtDef thoughtToMake;

		[NoTranslate]
		private string icon;

		public bool showBubble;

		public int stackLimitPerPawn = -1;

		public float lerpOpinionToZeroAfterDurationPct = 0.7f;

		public bool socialThoughtAffectingMood;

		public float maxCumulatedOpinionOffset = 3.40282347E+38f;

		public TaleDef taleDef;

		[Unsaved]
		private ThoughtWorker workerInt;

		private Texture2D iconInt;

		public string Label
		{
			get
			{
				if (!base.label.NullOrEmpty())
				{
					return base.label;
				}
				if (!this.stages.NullOrEmpty())
				{
					if (!this.stages[0].label.NullOrEmpty())
					{
						return this.stages[0].label;
					}
					if (!this.stages[0].labelSocial.NullOrEmpty())
					{
						return this.stages[0].labelSocial;
					}
				}
				Log.Error("Cannot get good label for ThoughtDef " + base.defName);
				return base.defName;
			}
		}

		public int DurationTicks
		{
			get
			{
				return (int)(this.durationDays * 60000.0);
			}
		}

		public bool IsMemory
		{
			get
			{
				return this.durationDays > 0.0 || typeof(Thought_Memory).IsAssignableFrom(this.thoughtClass);
			}
		}

		public bool IsSituational
		{
			get
			{
				return this.Worker != null;
			}
		}

		public bool IsSocial
		{
			get
			{
				return typeof(ISocialThought).IsAssignableFrom(this.ThoughtClass);
			}
		}

		public bool RequiresSpecificTraitsDegree
		{
			get
			{
				return this.requiredTraitsDegree != -2147483648;
			}
		}

		public ThoughtWorker Worker
		{
			get
			{
				if (this.workerInt == null && this.workerClass != null)
				{
					this.workerInt = (ThoughtWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public Type ThoughtClass
		{
			get
			{
				if (this.thoughtClass != null)
				{
					return this.thoughtClass;
				}
				if (this.IsMemory)
				{
					return typeof(Thought_Memory);
				}
				return typeof(Thought_Situational);
			}
		}

		public Texture2D Icon
		{
			get
			{
				if ((UnityEngine.Object)this.iconInt == (UnityEngine.Object)null)
				{
					if (this.icon == null)
					{
						return null;
					}
					this.iconInt = ContentFinder<Texture2D>.Get(this.icon, true);
				}
				return this.iconInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string error = enumerator.Current;
					yield return error;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (this.stages.NullOrEmpty())
			{
				yield return "no stages";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (this.workerClass != null && this.nextThought != null)
			{
				yield return "has a nextThought but also has a workerClass. nextThought only works for memories";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (this.IsMemory && this.workerClass != null)
			{
				yield return "has a workerClass but is a memory. workerClass only works for situational thoughts, not memories";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (this.IsMemory)
				yield break;
			if (this.workerClass != null)
				yield break;
			if (!this.IsSituational)
				yield break;
			yield return "is a situational thought but has no workerClass. Situational thoughts require workerClasses to analyze the situation";
			/*Error: Unable to find new state assignment for yield return*/;
			IL_01ca:
			/*Error near IL_01cb: Unexpected return in MoveNext()*/;
		}

		public static ThoughtDef Named(string defName)
		{
			return DefDatabase<ThoughtDef>.GetNamed(defName, true);
		}
	}
}
