using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class CollectionsMassCalculator
	{
		private static List<ThingStackPart> tmpThingStackParts = new List<ThingStackPart>();

		private static List<Thing> thingsInReverse = new List<Thing>();

		public static float Capacity(List<ThingStackPart> stackParts)
		{
			float num = 0f;
			for (int i = 0; i < stackParts.Count; i++)
			{
				if (stackParts[i].Count > 0)
				{
					Pawn pawn = stackParts[i].Thing as Pawn;
					if (pawn != null)
					{
						num += MassUtility.Capacity(pawn) * (float)stackParts[i].Count;
					}
				}
			}
			return Mathf.Max(num, 0f);
		}

		public static float MassUsage(List<ThingStackPart> stackParts, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreCorpsesGearAndInventory = false)
		{
			float num = 0f;
			for (int i = 0; i < stackParts.Count; i++)
			{
				int count = stackParts[i].Count;
				if (count > 0)
				{
					Thing thing = stackParts[i].Thing;
					Pawn pawn = thing as Pawn;
					if (pawn != null)
					{
						num = ((!includePawnsMass) ? (num + MassUtility.GearAndInventoryMass(pawn) * (float)count) : (num + pawn.GetStatValue(StatDefOf.Mass, true) * (float)count));
						if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
						{
							num -= MassUtility.InventoryMass(pawn) * (float)count;
						}
					}
					else
					{
						num += thing.GetStatValue(StatDefOf.Mass, true) * (float)count;
						if (ignoreCorpsesGearAndInventory)
						{
							Corpse corpse = thing as Corpse;
							if (corpse != null)
							{
								num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn) * (float)count;
							}
						}
					}
				}
			}
			return Mathf.Max(num, 0f);
		}

		public static float CapacityTransferables(List<TransferableOneWay> transferables)
		{
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].HasAnyThing && transferables[i].AnyThing is Pawn)
				{
					TransferableUtility.TransferNoSplit(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
					{
						CollectionsMassCalculator.tmpThingStackParts.Add(new ThingStackPart(originalThing, toTake));
					}, false, false);
				}
			}
			float result = CollectionsMassCalculator.Capacity(CollectionsMassCalculator.tmpThingStackParts);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float CapacityLeftAfterTransfer(List<TransferableOneWay> transferables)
		{
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].HasAnyThing && transferables[i].AnyThing is Pawn)
				{
					CollectionsMassCalculator.thingsInReverse.Clear();
					CollectionsMassCalculator.thingsInReverse.AddRange(transferables[i].things);
					CollectionsMassCalculator.thingsInReverse.Reverse();
					TransferableUtility.TransferNoSplit(CollectionsMassCalculator.thingsInReverse, transferables[i].MaxCount - transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
					{
						CollectionsMassCalculator.tmpThingStackParts.Add(new ThingStackPart(originalThing, toTake));
					}, false, false);
				}
			}
			CollectionsMassCalculator.thingsInReverse.Clear();
			float result = CollectionsMassCalculator.Capacity(CollectionsMassCalculator.tmpThingStackParts);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float CapacityLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables)
		{
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, CollectionsMassCalculator.tmpThingStackParts);
			float result = CollectionsMassCalculator.Capacity(CollectionsMassCalculator.tmpThingStackParts);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float Capacity<T>(List<T> things) where T : Thing
		{
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			for (int i = 0; i < things.Count; i++)
			{
				CollectionsMassCalculator.tmpThingStackParts.Add(new ThingStackPart((Thing)(object)things[i], ((Thing)(object)things[i]).stackCount));
			}
			float result = CollectionsMassCalculator.Capacity(CollectionsMassCalculator.tmpThingStackParts);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float MassUsageTransferables(List<TransferableOneWay> transferables, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreCorpsesGearAndInventory = false)
		{
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableUtility.TransferNoSplit(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
				{
					CollectionsMassCalculator.tmpThingStackParts.Add(new ThingStackPart(originalThing, toTake));
				}, false, false);
			}
			float result = CollectionsMassCalculator.MassUsage(CollectionsMassCalculator.tmpThingStackParts, ignoreInventory, includePawnsMass, ignoreCorpsesGearAndInventory);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float MassUsageLeftAfterTransfer(List<TransferableOneWay> transferables, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreCorpsesGearAndInventory = false)
		{
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				CollectionsMassCalculator.thingsInReverse.Clear();
				CollectionsMassCalculator.thingsInReverse.AddRange(transferables[i].things);
				CollectionsMassCalculator.thingsInReverse.Reverse();
				TransferableUtility.TransferNoSplit(CollectionsMassCalculator.thingsInReverse, transferables[i].MaxCount - transferables[i].CountToTransfer, delegate(Thing originalThing, int toTake)
				{
					CollectionsMassCalculator.tmpThingStackParts.Add(new ThingStackPart(originalThing, toTake));
				}, false, false);
			}
			float result = CollectionsMassCalculator.MassUsage(CollectionsMassCalculator.tmpThingStackParts, ignoreInventory, includePawnsMass, ignoreCorpsesGearAndInventory);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float MassUsage<T>(List<T> things, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreCorpsesGearAndInventory = false) where T : Thing
		{
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			for (int i = 0; i < things.Count; i++)
			{
				CollectionsMassCalculator.tmpThingStackParts.Add(new ThingStackPart((Thing)(object)things[i], ((Thing)(object)things[i]).stackCount));
			}
			float result = CollectionsMassCalculator.MassUsage(CollectionsMassCalculator.tmpThingStackParts, ignoreInventory, includePawnsMass, ignoreCorpsesGearAndInventory);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}

		public static float MassUsageLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, IgnorePawnsInventoryMode ignoreInventory, bool includePawnsMass = false, bool ignoreCorpsesGearAndInventory = false)
		{
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, CollectionsMassCalculator.tmpThingStackParts);
			float result = CollectionsMassCalculator.MassUsage(CollectionsMassCalculator.tmpThingStackParts, ignoreInventory, includePawnsMass, ignoreCorpsesGearAndInventory);
			CollectionsMassCalculator.tmpThingStackParts.Clear();
			return result;
		}
	}
}
