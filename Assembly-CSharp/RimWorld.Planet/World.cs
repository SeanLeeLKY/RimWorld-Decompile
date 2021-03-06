using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public sealed class World : IThingHolder, IExposable, IIncidentTarget, ILoadReferenceable
	{
		public WorldInfo info = new WorldInfo();

		public List<WorldComponent> components = new List<WorldComponent>();

		public FactionManager factionManager;

		public UniqueIDsManager uniqueIDsManager;

		public WorldPawns worldPawns;

		public WorldObjectsHolder worldObjects;

		public WorldSettings settings;

		public GameConditionManager gameConditionManager;

		public StoryState storyState;

		public WorldFeatures features;

		public WorldGrid grid;

		public WorldPathGrid pathGrid;

		public WorldRenderer renderer;

		public WorldInterface UI;

		public WorldDebugDrawer debugDrawer;

		public WorldDynamicDrawManager dynamicDrawManager;

		public WorldPathFinder pathFinder;

		public WorldPathPool pathPool;

		public WorldReachability reachability;

		public WorldFloodFiller floodFiller;

		public ConfiguredTicksAbsAtGameStartCache ticksAbsCache;

		public TileTemperaturesComp tileTemperatures;

		public WorldGenData genData;

		private static List<int> tmpNeighbors = new List<int>();

		private static List<Rot4> tmpOceanDirs = new List<Rot4>();

		public float PlanetCoverage
		{
			get
			{
				return this.info.planetCoverage;
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return null;
			}
		}

		public int Tile
		{
			get
			{
				return -1;
			}
		}

		public StoryState StoryState
		{
			get
			{
				return this.storyState;
			}
		}

		public GameConditionManager GameConditionManager
		{
			get
			{
				return this.gameConditionManager;
			}
		}

		public float PlayerWealthForStoryteller
		{
			get
			{
				float num = 0f;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					num += maps[i].PlayerWealthForStoryteller;
				}
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					num += caravans[j].PlayerWealthForStoryteller;
				}
				return num;
			}
		}

		public IEnumerable<Pawn> FreeColonistsForStoryteller
		{
			get
			{
				return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists;
			}
		}

		public FloatRange IncidentPointsRandomFactorRange
		{
			get
			{
				return FloatRange.One;
			}
		}

		public IEnumerable<IncidentTargetTypeDef> AcceptedTypes()
		{
			yield return IncidentTargetTypeDefOf.World;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<WorldInfo>(ref this.info, "info", new object[0]);
			Scribe_Deep.Look<WorldGrid>(ref this.grid, "grid", new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				foreach (WorldGenStepDef item in from gs in DefDatabase<WorldGenStepDef>.AllDefs
				orderby gs.order
				select gs)
				{
					if (this.grid == null || !this.grid.HasWorldData)
					{
						item.worldGenStep.GenerateWithoutWorldData(this.info.seedString);
					}
					else
					{
						item.worldGenStep.GenerateFromScribe(this.info.seedString);
					}
				}
			}
			else
			{
				this.ExposeComponents();
			}
		}

		public void ExposeComponents()
		{
			Scribe_Deep.Look<UniqueIDsManager>(ref this.uniqueIDsManager, "uniqueIDsManager", new object[0]);
			Scribe_Deep.Look<FactionManager>(ref this.factionManager, "factionManager", new object[0]);
			Scribe_Deep.Look<WorldPawns>(ref this.worldPawns, "worldPawns", new object[0]);
			Scribe_Deep.Look<WorldObjectsHolder>(ref this.worldObjects, "worldObjects", new object[0]);
			Scribe_Deep.Look<WorldSettings>(ref this.settings, "settings", new object[0]);
			Scribe_Deep.Look<GameConditionManager>(ref this.gameConditionManager, "gameConditionManager", new object[1]);
			Scribe_Deep.Look<StoryState>(ref this.storyState, "storyState", new object[1]
			{
				this
			});
			Scribe_Deep.Look<WorldFeatures>(ref this.features, "features", new object[0]);
			Scribe_Collections.Look<WorldComponent>(ref this.components, "components", LookMode.Deep, new object[1]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				BackCompatibility.WorldLoadingVars(this);
			}
			this.FillComponents();
		}

		public void ConstructComponents()
		{
			this.worldObjects = new WorldObjectsHolder();
			this.factionManager = new FactionManager();
			this.uniqueIDsManager = new UniqueIDsManager();
			this.worldPawns = new WorldPawns();
			this.settings = new WorldSettings();
			this.gameConditionManager = new GameConditionManager(null);
			this.storyState = new StoryState(this);
			this.renderer = new WorldRenderer();
			this.UI = new WorldInterface();
			this.debugDrawer = new WorldDebugDrawer();
			this.dynamicDrawManager = new WorldDynamicDrawManager();
			this.pathFinder = new WorldPathFinder();
			this.pathPool = new WorldPathPool();
			this.reachability = new WorldReachability();
			this.floodFiller = new WorldFloodFiller();
			this.ticksAbsCache = new ConfiguredTicksAbsAtGameStartCache();
			this.components.Clear();
			this.FillComponents();
		}

		private void FillComponents()
		{
			this.components.RemoveAll((WorldComponent component) => component == null);
			foreach (Type item2 in typeof(WorldComponent).AllSubclassesNonAbstract())
			{
				if (this.GetComponent(item2) == null)
				{
					WorldComponent item = (WorldComponent)Activator.CreateInstance(item2, this);
					this.components.Add(item);
				}
			}
			this.tileTemperatures = this.GetComponent<TileTemperaturesComp>();
			this.genData = this.GetComponent<WorldGenData>();
		}

		public void FinalizeInit()
		{
			this.pathGrid.RecalculateAllPerceivedPathCosts(-1f);
			AmbientSoundManager.EnsureWorldAmbientSoundCreated();
			WorldComponentUtility.FinalizeInit(this);
		}

		public void WorldTick()
		{
			this.worldPawns.WorldPawnsTick();
			this.factionManager.FactionManagerTick();
			this.worldObjects.WorldObjectsHolderTick();
			this.debugDrawer.WorldDebugDrawerTick();
			this.pathGrid.WorldPathGridTick();
			WorldComponentUtility.WorldComponentTick(this);
		}

		public void WorldPostTick()
		{
			try
			{
				this.gameConditionManager.GameConditionManagerTick();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		public void WorldUpdate()
		{
			bool worldRenderedNow = WorldRendererUtility.WorldRenderedNow;
			this.renderer.CheckActivateWorldCamera();
			if (worldRenderedNow)
			{
				ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
				this.renderer.DrawWorldLayers();
				this.dynamicDrawManager.DrawDynamicWorldObjects();
				this.features.UpdateFeatures();
				NoiseDebugUI.RenderPlanetNoise();
			}
			WorldComponentUtility.WorldComponentUpdate(this);
		}

		public T GetComponent<T>() where T : WorldComponent
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				T val = (T)(this.components[i] as T);
				if (val != null)
				{
					return val;
				}
			}
			return (T)null;
		}

		public WorldComponent GetComponent(Type type)
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				if (type.IsAssignableFrom(this.components[i].GetType()))
				{
					return this.components[i];
				}
			}
			return null;
		}

		public Rot4 CoastDirectionAt(int tileID)
		{
			Tile tile = this.grid[tileID];
			if (!tile.biome.canBuildBase)
			{
				return Rot4.Invalid;
			}
			World.tmpOceanDirs.Clear();
			this.grid.GetTileNeighbors(tileID, World.tmpNeighbors);
			int i = 0;
			int count = World.tmpNeighbors.Count;
			for (; i < count; i++)
			{
				Tile tile2 = this.grid[World.tmpNeighbors[i]];
				if (tile2.biome == BiomeDefOf.Ocean)
				{
					Rot4 rotFromTo = this.grid.GetRotFromTo(tileID, World.tmpNeighbors[i]);
					if (!World.tmpOceanDirs.Contains(rotFromTo))
					{
						World.tmpOceanDirs.Add(rotFromTo);
					}
				}
			}
			if (World.tmpOceanDirs.Count == 0)
			{
				return Rot4.Invalid;
			}
			Rand.PushState();
			Rand.Seed = tileID;
			int index = Rand.Range(0, World.tmpOceanDirs.Count);
			Rand.PopState();
			return World.tmpOceanDirs[index];
		}

		public bool HasCaves(int tile)
		{
			Tile tile2 = this.grid[tile];
			float chance;
			if ((int)tile2.hilliness >= 4)
			{
				chance = 0.5f;
				goto IL_003d;
			}
			if ((int)tile2.hilliness >= 3)
			{
				chance = 0.25f;
				goto IL_003d;
			}
			return false;
			IL_003d:
			return Rand.ChanceSeeded(chance, Gen.HashCombineInt(Find.World.info.Seed, tile));
		}

		public IEnumerable<ThingDef> NaturalRockTypesIn(int tile)
		{
			Rand.PushState();
			Rand.Seed = tile;
			List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock
			select d).ToList();
			int num = Rand.RangeInclusive(2, 3);
			if (num > list.Count)
			{
				num = list.Count;
			}
			List<ThingDef> list2 = new List<ThingDef>();
			for (int i = 0; i < num; i++)
			{
				ThingDef item = list.RandomElement();
				list.Remove(item);
				list2.Add(item);
			}
			Rand.PopState();
			return list2;
		}

		public bool Impassable(int tileID)
		{
			return !this.pathGrid.Passable(tileID);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			List<WorldObject> allWorldObjects = this.worldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				IThingHolder thingHolder = allWorldObjects[i] as IThingHolder;
				if (thingHolder != null)
				{
					outChildren.Add(thingHolder);
				}
				List<WorldObjectComp> allComps = allWorldObjects[i].AllComps;
				for (int j = 0; j < allComps.Count; j++)
				{
					IThingHolder thingHolder2 = allComps[j] as IThingHolder;
					if (thingHolder2 != null)
					{
						outChildren.Add(thingHolder2);
					}
				}
			}
			for (int k = 0; k < this.components.Count; k++)
			{
				IThingHolder thingHolder3 = this.components[k] as IThingHolder;
				if (thingHolder3 != null)
				{
					outChildren.Add(thingHolder3);
				}
			}
		}

		public string GetUniqueLoadID()
		{
			return "World";
		}

		public override string ToString()
		{
			return "(World-" + this.info.name + ")";
		}
	}
}
