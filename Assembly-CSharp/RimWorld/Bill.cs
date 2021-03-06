using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Bill : IExposable, ILoadReferenceable
	{
		[Unsaved]
		public BillStack billStack;

		private int loadID = -1;

		public RecipeDef recipe;

		public bool suspended;

		public ThingFilter ingredientFilter;

		public float ingredientSearchRadius = 999f;

		public IntRange allowedSkillRange = new IntRange(0, 20);

		public bool deleted;

		public int lastIngredientSearchFailTicks = -99999;

		public const int MaxIngredientSearchRadius = 999;

		public const float ButSize = 24f;

		private const float InterfaceBaseHeight = 53f;

		private const float InterfaceStatusLineHeight = 17f;

		public Map Map
		{
			get
			{
				return this.billStack.billGiver.Map;
			}
		}

		public virtual string Label
		{
			get
			{
				return this.recipe.label;
			}
		}

		public virtual string LabelCap
		{
			get
			{
				return this.Label.CapitalizeFirst();
			}
		}

		public virtual bool CheckIngredientsIfSociallyProper
		{
			get
			{
				return true;
			}
		}

		public virtual bool CompletableEver
		{
			get
			{
				return true;
			}
		}

		protected virtual string StatusString
		{
			get
			{
				return null;
			}
		}

		protected virtual float StatusLineMinHeight
		{
			get
			{
				return 0f;
			}
		}

		public bool DeletedOrDereferenced
		{
			get
			{
				if (this.deleted)
				{
					return true;
				}
				Thing thing = this.billStack.billGiver as Thing;
				if (thing != null && thing.Destroyed)
				{
					return true;
				}
				return false;
			}
		}

		public Bill()
		{
		}

		public Bill(RecipeDef recipe)
		{
			this.recipe = recipe;
			this.ingredientFilter = new ThingFilter();
			this.ingredientFilter.CopyAllowancesFrom(recipe.defaultIngredientFilter);
			this.loadID = Find.UniqueIDsManager.GetNextBillID();
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.loadID, "loadID", 0, false);
			Scribe_Defs.Look<RecipeDef>(ref this.recipe, "recipe");
			Scribe_Values.Look<bool>(ref this.suspended, "suspended", false, false);
			Scribe_Values.Look<float>(ref this.ingredientSearchRadius, "ingredientSearchRadius", 999f, false);
			Scribe_Values.Look<IntRange>(ref this.allowedSkillRange, "allowedSkillRange", default(IntRange), false);
			if (Scribe.mode == LoadSaveMode.Saving && this.recipe.fixedIngredientFilter != null)
			{
				foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				{
					if (!this.recipe.fixedIngredientFilter.Allows(allDef))
					{
						this.ingredientFilter.SetAllow(allDef, false);
					}
				}
			}
			Scribe_Deep.Look<ThingFilter>(ref this.ingredientFilter, "ingredientFilter", new object[0]);
		}

		public virtual bool PawnAllowedToStartAnew(Pawn p)
		{
			if (this.recipe.workSkill != null)
			{
				int level = p.skills.GetSkill(this.recipe.workSkill).Level;
				if (level >= this.allowedSkillRange.min && level <= this.allowedSkillRange.max)
				{
					goto IL_0050;
				}
				return false;
			}
			goto IL_0050;
			IL_0050:
			return true;
		}

		public virtual void Notify_PawnDidWork(Pawn p)
		{
		}

		public virtual void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
		}

		public abstract bool ShouldDoNow();

		public virtual void Notify_DoBillStarted(Pawn billDoer)
		{
		}

		protected virtual void DoConfigInterface(Rect rect, Color baseColor)
		{
			rect.yMin += 29f;
			Vector2 center = rect.center;
			float y = center.y;
			float num = rect.xMax - (rect.yMax - y);
			Widgets.InfoCardButton((float)(num - 12.0), (float)(y - 12.0), this.recipe);
		}

		public virtual void DoStatusLineInterface(Rect rect)
		{
		}

		public Rect DoInterface(float x, float y, float width, int index)
		{
			Rect rect = new Rect(x, y, width, 53f);
			float num = 0f;
			if (!this.StatusString.NullOrEmpty())
			{
				num = Mathf.Max(17f, this.StatusLineMinHeight);
			}
			rect.height += num;
			Color color = Color.white;
			if (!this.ShouldDoNow())
			{
				color = new Color(1f, 0.7f, 0.7f, 0.7f);
			}
			GUI.color = color;
			Text.Font = GameFont.Small;
			if (index % 2 == 0)
			{
				Widgets.DrawAltRect(rect);
			}
			GUI.BeginGroup(rect);
			Rect butRect = new Rect(0f, 0f, 24f, 24f);
			if (this.billStack.IndexOf(this) > 0 && Widgets.ButtonImage(butRect, TexButton.ReorderUp, color))
			{
				this.billStack.Reorder(this, -1);
				SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
			}
			if (this.billStack.IndexOf(this) < this.billStack.Count - 1)
			{
				Rect butRect2 = new Rect(0f, 24f, 24f, 24f);
				if (Widgets.ButtonImage(butRect2, TexButton.ReorderDown, color))
				{
					this.billStack.Reorder(this, 1);
					SoundDefOf.TickLow.PlayOneShotOnCamera(null);
				}
			}
			Rect rect2 = new Rect(28f, 0f, (float)(rect.width - 48.0 - 20.0), (float)(rect.height + 5.0));
			Widgets.Label(rect2, this.LabelCap);
			this.DoConfigInterface(rect.AtZero(), color);
			Rect rect3 = new Rect((float)(rect.width - 24.0), 0f, 24f, 24f);
			if (Widgets.ButtonImage(rect3, TexButton.DeleteX, color))
			{
				this.billStack.Delete(this);
			}
			Rect butRect3 = new Rect(rect3);
			butRect3.x -= (float)(butRect3.width + 4.0);
			if (Widgets.ButtonImage(butRect3, TexButton.Suspend, color))
			{
				this.suspended = !this.suspended;
			}
			if (!this.StatusString.NullOrEmpty())
			{
				Text.Font = GameFont.Tiny;
				Rect rect4 = new Rect(24f, rect.height - num, (float)(rect.width - 24.0), num);
				Widgets.Label(rect4, this.StatusString);
				this.DoStatusLineInterface(rect4);
			}
			GUI.EndGroup();
			if (this.suspended)
			{
				Text.Font = GameFont.Medium;
				Text.Anchor = TextAnchor.MiddleCenter;
				Rect rect5 = new Rect((float)(rect.x + rect.width / 2.0 - 70.0), (float)(rect.y + rect.height / 2.0 - 20.0), 140f, 40f);
				GUI.DrawTexture(rect5, TexUI.GrayTextBG);
				Widgets.Label(rect5, "SuspendedCaps".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				Text.Font = GameFont.Small;
			}
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			return rect;
		}

		public bool IsFixedOrAllowedIngredient(Thing thing)
		{
			for (int i = 0; i < this.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = this.recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(thing))
				{
					return true;
				}
			}
			return this.recipe.fixedIngredientFilter.Allows(thing) && this.ingredientFilter.Allows(thing);
		}

		public bool IsFixedOrAllowedIngredient(ThingDef def)
		{
			for (int i = 0; i < this.recipe.ingredients.Count; i++)
			{
				IngredientCount ingredientCount = this.recipe.ingredients[i];
				if (ingredientCount.IsFixedIngredient && ingredientCount.filter.Allows(def))
				{
					return true;
				}
			}
			return this.recipe.fixedIngredientFilter.Allows(def) && this.ingredientFilter.Allows(def);
		}

		public static void CreateNoPawnsWithSkillDialog(RecipeDef recipe)
		{
			string str = "RecipeRequiresSkills".Translate(recipe.LabelCap);
			str += "\n\n";
			str += recipe.MinSkillString;
			Find.WindowStack.Add(new Dialog_MessageBox(str, null, null, null, null, null, false));
		}

		public virtual BillStoreModeDef GetStoreMode()
		{
			return BillStoreModeDefOf.BestStockpile;
		}

		public string GetUniqueLoadID()
		{
			return "Bill_" + this.recipe.defName + "_" + this.loadID;
		}

		public override string ToString()
		{
			return this.GetUniqueLoadID();
		}
	}
}
