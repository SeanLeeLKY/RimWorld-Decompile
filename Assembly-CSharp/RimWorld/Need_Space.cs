using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Need_Space : Need_Seeker
	{
		private static List<Room> tempScanRooms = new List<Room>();

		private const float MinCramped = 0.01f;

		private const float MinNormal = 0.3f;

		private const float MinSpacious = 0.7f;

		public static readonly int SampleNumCells = GenRadial.NumCellsInRadius(7.9f);

		private static readonly SimpleCurve RoomCellCountSpaceCurve = new SimpleCurve
		{
			{
				new CurvePoint(3f, 0f),
				true
			},
			{
				new CurvePoint(9f, 0.25f),
				true
			},
			{
				new CurvePoint(16f, 0.5f),
				true
			},
			{
				new CurvePoint(42f, 0.71f),
				true
			},
			{
				new CurvePoint(100f, 1f),
				true
			}
		};

		public override float CurInstantLevel
		{
			get
			{
				return this.SpacePerceptibleNow();
			}
		}

		public SpaceCategory CurCategory
		{
			get
			{
				if (this.CurLevel < 0.0099999997764825821)
				{
					return SpaceCategory.VeryCramped;
				}
				if (this.CurLevel < 0.30000001192092896)
				{
					return SpaceCategory.Cramped;
				}
				if (this.CurLevel < 0.699999988079071)
				{
					return SpaceCategory.Normal;
				}
				return SpaceCategory.Spacious;
			}
		}

		public Need_Space(Pawn pawn)
			: base(pawn)
		{
			base.threshPercents = new List<float>();
			base.threshPercents.Add(0.3f);
			base.threshPercents.Add(0.7f);
		}

		public float SpacePerceptibleNow()
		{
			if (!base.pawn.Spawned)
			{
				return 1f;
			}
			IntVec3 position = base.pawn.Position;
			Need_Space.tempScanRooms.Clear();
			for (int i = 0; i < 5; i++)
			{
				IntVec3 loc = position + GenRadial.RadialPattern[i];
				Room room = loc.GetRoom(base.pawn.Map, RegionType.Set_Passable);
				if (room != null)
				{
					if (i == 0 && room.PsychologicallyOutdoors)
					{
						return 1f;
					}
					if ((i == 0 || room.RegionType != RegionType.Portal) && !Need_Space.tempScanRooms.Contains(room))
					{
						Need_Space.tempScanRooms.Add(room);
					}
				}
			}
			float num = 0f;
			for (int j = 0; j < Need_Space.SampleNumCells; j++)
			{
				IntVec3 loc2 = position + GenRadial.RadialPattern[j];
				if (Need_Space.tempScanRooms.Contains(loc2.GetRoom(base.pawn.Map, RegionType.Set_Passable)))
				{
					num = (float)(num + 1.0);
				}
			}
			Need_Space.tempScanRooms.Clear();
			return Need_Space.RoomCellCountSpaceCurve.Evaluate(num);
		}
	}
}
