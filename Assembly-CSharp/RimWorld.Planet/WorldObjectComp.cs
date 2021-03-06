using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldObjectComp
	{
		public WorldObject parent;

		public WorldObjectCompProperties props;

		public IThingHolder ParentHolder
		{
			get
			{
				return this.parent.ParentHolder;
			}
		}

		public bool ParentHasMap
		{
			get
			{
				MapParent mapParent = this.parent as MapParent;
				return mapParent != null && mapParent.HasMap;
			}
		}

		public virtual void Initialize(WorldObjectCompProperties props)
		{
			this.props = props;
		}

		public virtual void CompTick()
		{
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			yield break;
		}

		public virtual IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			yield break;
		}

		public virtual IEnumerable<IncidentTargetTypeDef> AcceptedTypes()
		{
			yield break;
		}

		public virtual string CompInspectStringExtra()
		{
			return null;
		}

		public virtual string GetDescriptionPart()
		{
			return null;
		}

		public virtual void PostPostRemove()
		{
		}

		public virtual void PostExposeData()
		{
		}

		public override string ToString()
		{
			return base.GetType().Name + "(parent=" + this.parent + " at=" + ((this.parent == null) ? (-1) : this.parent.Tile) + ")";
		}
	}
}
