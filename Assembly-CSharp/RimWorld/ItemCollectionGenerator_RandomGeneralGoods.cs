using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGenerator_RandomGeneralGoods : ItemCollectionGenerator
	{
		private enum GoodsType
		{
			None,
			Meals,
			RawFood,
			Medicine,
			Drugs,
			Resources
		}

		private static Pair<GoodsType, float>[] GoodsWeights = new Pair<GoodsType, float>[5]
		{
			new Pair<GoodsType, float>(GoodsType.Meals, 1f),
			new Pair<GoodsType, float>(GoodsType.RawFood, 0.75f),
			new Pair<GoodsType, float>(GoodsType.Medicine, 0.234f),
			new Pair<GoodsType, float>(GoodsType.Drugs, 0.234f),
			new Pair<GoodsType, float>(GoodsType.Resources, 0.234f)
		};

		protected override void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings)
		{
			int? count = parms.count;
			int num = (!count.HasValue) ? Rand.RangeInclusive(10, 20) : count.Value;
			TechLevel? techLevel = parms.techLevel;
			TechLevel techLevel2 = (!techLevel.HasValue) ? TechLevel.Spacer : techLevel.Value;
			for (int i = 0; i < num; i++)
			{
				outThings.Add(this.GenerateSingle(techLevel2));
			}
		}

		private Thing GenerateSingle(TechLevel techLevel)
		{
			GoodsType first = ItemCollectionGenerator_RandomGeneralGoods.GoodsWeights.RandomElementByWeight((Pair<GoodsType, float> x) => x.Second).First;
			switch (first)
			{
			case GoodsType.Meals:
				return this.RandomMeals(techLevel);
			case GoodsType.RawFood:
				return this.RandomRawFood(techLevel);
			case GoodsType.Medicine:
				return this.RandomMedicine(techLevel);
			case GoodsType.Drugs:
				return this.RandomDrugs(techLevel);
			case GoodsType.Resources:
				return this.RandomResources(techLevel);
			default:
				Log.Error("Goods type not handled: " + first);
				return null;
			}
		}

		private Thing RandomMeals(TechLevel techLevel)
		{
			ThingDef thingDef;
			if (techLevel.IsNeolithicOrWorse())
			{
				thingDef = ThingDefOf.Pemmican;
			}
			else
			{
				float value = Rand.Value;
				thingDef = ((!(value < 0.5)) ? ((!((double)value < 0.75)) ? ThingDefOf.MealSurvivalPack : ThingDefOf.MealFine) : ThingDefOf.MealSimple);
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int num = Mathf.Min(thingDef.stackLimit, 10);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}

		private Thing RandomRawFood(TechLevel techLevel)
		{
			ThingDef thingDef = default(ThingDef);
			if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.HasComp(typeof(CompHatcher)) && (int)x.techLevel <= (int)techLevel && (int)x.ingestible.preferability < 6
			select x).TryRandomElement<ThingDef>(out thingDef))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int max = Mathf.Min(thingDef.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomMedicine(TechLevel techLevel)
		{
			ThingDef thingDef = default(ThingDef);
			if (Rand.Value < 0.75 && (int)techLevel >= (int)ThingDefOf.HerbalMedicine.techLevel)
			{
				thingDef = (from x in ItemCollectionGeneratorUtility.allGeneratableItems
				where x.IsMedicine && (int)x.techLevel <= (int)techLevel
				select x).MaxBy((ThingDef x) => x.GetStatValueAbstract(StatDefOf.MedicalPotency, null));
			}
			else if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsMedicine
			select x).TryRandomElement<ThingDef>(out thingDef))
			{
				throw new Exception();
			}
			if (techLevel.IsNeolithicOrWorse())
			{
				thingDef = ThingDefOf.HerbalMedicine;
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int max = Mathf.Min(thingDef.stackLimit, 20);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomDrugs(TechLevel techLevel)
		{
			ThingDef thingDef = default(ThingDef);
			if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
			where x.IsDrug && (int)x.techLevel <= (int)techLevel
			select x).TryRandomElement<ThingDef>(out thingDef))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int max = Mathf.Min(thingDef.stackLimit, 25);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomResources(TechLevel techLevel)
		{
			ThingDef thingDef = BaseGenUtility.RandomCheapWallStuff(techLevel, false);
			Thing thing = ThingMaker.MakeThing(thingDef, null);
			int num = Mathf.Min(thingDef.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}
	}
}
