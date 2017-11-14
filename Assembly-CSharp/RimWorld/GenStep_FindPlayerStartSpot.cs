using Verse;

namespace RimWorld
{
	public class GenStep_FindPlayerStartSpot : GenStep
	{
		private const int MinRoomCellCount = 10;

		public override void Generate(Map map)
		{
			DeepProfiler.Start("RebuildAllRegions");
			map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
			DeepProfiler.End();
			MapGenerator.PlayerStartSpot = CellFinderLoose.TryFindCentralCell(map, 7, 10, (IntVec3 x) => !x.Roofed(map));
		}
	}
}
