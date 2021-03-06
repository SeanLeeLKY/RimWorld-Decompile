using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class UnfinishedThing : ThingWithComps
	{
		private Pawn creatorInt;

		private string creatorName = "ErrorCreatorName";

		private RecipeDef recipeInt;

		public List<Thing> ingredients = new List<Thing>();

		private Bill_ProductionWithUft boundBillInt;

		public float workLeft = -10000f;

		private const float CancelIngredientRecoveryFraction = 0.75f;

		public Pawn Creator
		{
			get
			{
				return this.creatorInt;
			}
			set
			{
				if (value == null)
				{
					Log.Error("Cannot set creator to null.");
				}
				else
				{
					this.creatorInt = value;
					this.creatorName = value.NameStringShort;
				}
			}
		}

		public RecipeDef Recipe
		{
			get
			{
				return this.recipeInt;
			}
		}

		public Bill_ProductionWithUft BoundBill
		{
			get
			{
				if (this.boundBillInt != null && (this.boundBillInt.DeletedOrDereferenced || this.boundBillInt.BoundUft != this))
				{
					this.boundBillInt = null;
				}
				return this.boundBillInt;
			}
			set
			{
				if (value != this.boundBillInt)
				{
					Bill_ProductionWithUft bill_ProductionWithUft = this.boundBillInt;
					this.boundBillInt = value;
					if (bill_ProductionWithUft != null && bill_ProductionWithUft.BoundUft == this)
					{
						bill_ProductionWithUft.SetBoundUft(null, false);
					}
					if (value != null)
					{
						this.recipeInt = value.recipe;
						if (value.BoundUft != this)
						{
							value.SetBoundUft(this, false);
						}
					}
				}
			}
		}

		public Thing BoundWorkTable
		{
			get
			{
				if (this.BoundBill == null)
				{
					return null;
				}
				IBillGiver billGiver = this.BoundBill.billStack.billGiver;
				Thing thing = billGiver as Thing;
				if (thing.Destroyed)
				{
					return null;
				}
				return thing;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (this.Recipe == null)
				{
					return base.LabelNoCount;
				}
				if (base.Stuff == null)
				{
					return "UnfinishedItem".Translate(this.Recipe.products[0].thingDef.label);
				}
				return "UnfinishedItemWithStuff".Translate(base.Stuff.LabelAsStuff, this.Recipe.products[0].thingDef.label);
			}
		}

		public bool Initialized
		{
			get
			{
				return this.workLeft > -5000.0;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving && this.boundBillInt != null && this.boundBillInt.DeletedOrDereferenced)
			{
				this.boundBillInt = null;
			}
			Scribe_References.Look<Pawn>(ref this.creatorInt, "creator", false);
			Scribe_Values.Look<string>(ref this.creatorName, "creatorName", (string)null, false);
			Scribe_References.Look<Bill_ProductionWithUft>(ref this.boundBillInt, "bill", false);
			Scribe_Defs.Look<RecipeDef>(ref this.recipeInt, "recipe");
			Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
			Scribe_Collections.Look<Thing>(ref this.ingredients, "ingredients", LookMode.Deep, new object[0]);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode == DestroyMode.Cancel)
			{
				for (int i = 0; i < this.ingredients.Count; i++)
				{
					int num = GenMath.RoundRandom((float)((float)this.ingredients[i].stackCount * 0.75));
					if (num > 0)
					{
						this.ingredients[i].stackCount = num;
						GenPlace.TryPlaceThing(this.ingredients[i], base.Position, base.Map, ThingPlaceMode.Near, null);
					}
				}
				this.ingredients.Clear();
			}
			base.Destroy(mode);
			this.BoundBill = null;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield return (Gizmo)new Command_Action
			{
				defaultLabel = "CommandCancelConstructionLabel".Translate(),
				defaultDesc = "CommandCancelConstructionDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true),
				hotKey = KeyBindingDefOf.DesignatorCancel,
				action = delegate
				{
					((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0115: stateMachine*/)._0024this.Destroy(DestroyMode.Cancel);
				}
			};
			/*Error: Unable to find new state assignment for yield return*/;
			IL_014f:
			/*Error near IL_0150: Unexpected return in MoveNext()*/;
		}

		public Bill_ProductionWithUft BillOnTableForMe(Thing workTable)
		{
			if (this.Recipe.AllRecipeUsers.Contains(workTable.def))
			{
				IBillGiver billGiver = (IBillGiver)workTable;
				for (int i = 0; i < billGiver.BillStack.Count; i++)
				{
					Bill_ProductionWithUft bill_ProductionWithUft = billGiver.BillStack[i] as Bill_ProductionWithUft;
					if (bill_ProductionWithUft != null && bill_ProductionWithUft.ShouldDoNow() && bill_ProductionWithUft != null && bill_ProductionWithUft.recipe == this.Recipe)
					{
						return bill_ProductionWithUft;
					}
				}
			}
			return null;
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (this.BoundWorkTable != null)
			{
				GenDraw.DrawLineBetween(this.TrueCenter(), this.BoundWorkTable.TrueCenter());
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = text + "Author".Translate() + ": " + this.creatorName;
			string text2 = text;
			return text2 + "\n" + "WorkLeft".Translate() + ": " + this.workLeft.ToStringWorkAmount();
		}
	}
}
