using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class WorldFloodFiller
	{
		private bool working;

		private Queue<int> openSet = new Queue<int>();

		private List<int> traversalDistance = new List<int>();

		private List<int> visited = new List<int>();

		public void FloodFill(int rootTile, Predicate<int> passCheck, Action<int> processor, int maxTilesToProcess = 2147483647, IEnumerable<int> extraRootTiles = null)
		{
			this.FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile);
				return false;
			}, maxTilesToProcess, extraRootTiles);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Action<int, int> processor, int maxTilesToProcess = 2147483647, IEnumerable<int> extraRootTiles = null)
		{
			this.FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile, traversalDistance);
				return false;
			}, maxTilesToProcess, extraRootTiles);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Predicate<int> processor, int maxTilesToProcess = 2147483647, IEnumerable<int> extraRootTiles = null)
		{
			this.FloodFill(rootTile, passCheck, (int tile, int traversalDistance) => processor(tile), maxTilesToProcess, extraRootTiles);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Func<int, int, bool> processor, int maxTilesToProcess = 2147483647, IEnumerable<int> extraRootTiles = null)
		{
			if (this.working)
			{
				Log.Error("Nested FloodFill calls are not allowed. This will cause bugs.");
			}
			this.working = true;
			this.ClearVisited();
			if (rootTile != -1 && extraRootTiles == null && !passCheck(rootTile))
			{
				this.working = false;
			}
			else
			{
				int tilesCount = Find.WorldGrid.TilesCount;
				int num = tilesCount;
				if (this.traversalDistance.Count != tilesCount)
				{
					this.traversalDistance.Clear();
					for (int i = 0; i < tilesCount; i++)
					{
						this.traversalDistance.Add(-1);
					}
				}
				WorldGrid worldGrid = Find.WorldGrid;
				List<int> tileIDToNeighbors_offsets = worldGrid.tileIDToNeighbors_offsets;
				List<int> tileIDToNeighbors_values = worldGrid.tileIDToNeighbors_values;
				int num2 = 0;
				this.openSet.Clear();
				if (rootTile != -1)
				{
					this.visited.Add(rootTile);
					this.traversalDistance[rootTile] = 0;
					this.openSet.Enqueue(rootTile);
				}
				if (extraRootTiles != null)
				{
					this.visited.AddRange(extraRootTiles);
					IList<int> list = extraRootTiles as IList<int>;
					if (list != null)
					{
						for (int j = 0; j < list.Count; j++)
						{
							int num3 = list[j];
							this.traversalDistance[num3] = 0;
							this.openSet.Enqueue(num3);
						}
					}
					else
					{
						foreach (int extraRootTile in extraRootTiles)
						{
							this.traversalDistance[extraRootTile] = 0;
							this.openSet.Enqueue(extraRootTile);
						}
					}
				}
				while (this.openSet.Count > 0)
				{
					int num4 = this.openSet.Dequeue();
					int num5 = this.traversalDistance[num4];
					if (!processor(num4, num5))
					{
						num2++;
						if (num2 != maxTilesToProcess)
						{
							int num6 = (num4 + 1 >= tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_values.Count : tileIDToNeighbors_offsets[num4 + 1];
							for (int k = tileIDToNeighbors_offsets[num4]; k < num6; k++)
							{
								int num7 = tileIDToNeighbors_values[k];
								if (this.traversalDistance[num7] == -1 && passCheck(num7))
								{
									this.visited.Add(num7);
									this.openSet.Enqueue(num7);
									this.traversalDistance[num7] = num5 + 1;
								}
							}
							if (this.openSet.Count > num)
							{
								Log.Error("Overflow on world flood fill (>" + num + " cells). Make sure we're not flooding over the same area after we check it.");
								this.working = false;
								return;
							}
							continue;
						}
					}
					break;
				}
				this.working = false;
			}
		}

		private void ClearVisited()
		{
			int i = 0;
			int count = this.visited.Count;
			for (; i < count; i++)
			{
				this.traversalDistance[this.visited[i]] = -1;
			}
			this.visited.Clear();
			this.openSet.Clear();
		}
	}
}
