using UnityEngine;

namespace Verse
{
	public class Graphic
	{
		public GraphicData data;

		public string path;

		public Color color = Color.white;

		public Color colorTwo = Color.white;

		public Vector2 drawSize = Vector2.one;

		private Graphic_Shadow cachedShadowGraphicInt;

		private Graphic cachedShadowlessGraphicInt;

		public Shader Shader
		{
			get
			{
				Material matSingle = this.MatSingle;
				if ((Object)matSingle != (Object)null)
				{
					return matSingle.shader;
				}
				return ShaderDatabase.Cutout;
			}
		}

		public Graphic_Shadow ShadowGraphic
		{
			get
			{
				if (this.cachedShadowGraphicInt == null && this.data != null && this.data.shadowData != null)
				{
					this.cachedShadowGraphicInt = new Graphic_Shadow(this.data.shadowData);
				}
				return this.cachedShadowGraphicInt;
			}
		}

		public Color Color
		{
			get
			{
				return this.color;
			}
		}

		public Color ColorTwo
		{
			get
			{
				return this.colorTwo;
			}
		}

		public virtual Material MatSingle
		{
			get
			{
				return BaseContent.BadMat;
			}
		}

		public virtual Material MatFront
		{
			get
			{
				return this.MatSingle;
			}
		}

		public virtual Material MatSide
		{
			get
			{
				return this.MatSingle;
			}
		}

		public virtual Material MatBack
		{
			get
			{
				return this.MatSingle;
			}
		}

		public virtual bool ShouldDrawRotated
		{
			get
			{
				return false;
			}
		}

		public virtual void Init(GraphicRequest req)
		{
			Log.ErrorOnce("Cannot init Graphic of class " + base.GetType().ToString(), 658928);
		}

		public virtual Material MatAt(Rot4 rot, Thing thing = null)
		{
			switch (rot.AsInt)
			{
			case 0:
				return this.MatBack;
			case 1:
				return this.MatSide;
			case 2:
				return this.MatFront;
			case 3:
				return this.MatSide;
			default:
				return BaseContent.BadMat;
			}
		}

		public virtual Mesh MeshAt(Rot4 rot)
		{
			if (this.ShouldDrawRotated)
			{
				return MeshPool.GridPlane(this.drawSize);
			}
			Vector2 vector = this.drawSize;
			if (rot.IsHorizontal)
			{
				vector = vector.Rotated();
			}
			if (rot == Rot4.West && (this.data == null || this.data.allowFlip))
			{
				return MeshPool.GridPlaneFlip(vector);
			}
			return MeshPool.GridPlane(vector);
		}

		public virtual Material MatSingleFor(Thing thing)
		{
			return this.MatSingle;
		}

		public void Draw(Vector3 loc, Rot4 rot, Thing thing, float extraRotation = 0f)
		{
			this.DrawWorker(loc, rot, thing.def, thing, extraRotation);
		}

		public void DrawFromDef(Vector3 loc, Rot4 rot, ThingDef thingDef, float extraRotation = 0f)
		{
			this.DrawWorker(loc, rot, thingDef, null, extraRotation);
		}

		public virtual void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mesh mesh = this.MeshAt(rot);
			Quaternion quaternion = this.QuatFromRot(rot);
			if (extraRotation != 0.0)
			{
				quaternion *= Quaternion.Euler(Vector3.up * extraRotation);
			}
			Material material = this.MatAt(rot, thing);
			Graphics.DrawMesh(mesh, loc, quaternion, material, 0);
			if (this.ShadowGraphic != null)
			{
				this.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
			}
		}

		public virtual void Print(SectionLayer layer, Thing thing)
		{
			Vector2 size;
			bool flag;
			if (this.ShouldDrawRotated)
			{
				size = this.drawSize;
				flag = false;
			}
			else
			{
				size = (thing.Rotation.IsHorizontal ? this.drawSize.Rotated() : this.drawSize);
				flag = (thing.Rotation == Rot4.West);
				if (this.data != null && !this.data.allowFlip)
				{
					flag = false;
				}
			}
			float num = 0f;
			if (this.ShouldDrawRotated)
			{
				num = thing.Rotation.AsAngle;
			}
			if (flag && this.data != null)
			{
				num += this.data.flipExtraRotation;
			}
			Printer_Plane.PrintPlane(layer, thing.TrueCenter(), size, this.MatAt(thing.Rotation, thing), num, flag, null, null, 0.01f);
			if (this.ShadowGraphic != null && thing != null)
			{
				this.ShadowGraphic.Print(layer, thing);
			}
		}

		public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + base.GetType().ToString(), 66300);
			return BaseContent.BadGraphic;
		}

		public virtual Graphic GetCopy(Vector2 newDrawSize)
		{
			return GraphicDatabase.Get(base.GetType(), this.path, this.Shader, newDrawSize, this.color, this.colorTwo);
		}

		public virtual Graphic GetShadowlessGraphic()
		{
			if (this.data != null && this.data.shadowData != null)
			{
				if (this.cachedShadowlessGraphicInt == null)
				{
					GraphicData graphicData = new GraphicData();
					graphicData.CopyFrom(this.data);
					graphicData.shadowData = null;
					this.cachedShadowlessGraphicInt = graphicData.Graphic;
				}
				return this.cachedShadowlessGraphicInt;
			}
			return this;
		}

		protected Quaternion QuatFromRot(Rot4 rot)
		{
			if (this.data != null && !this.data.drawRotated)
			{
				return Quaternion.identity;
			}
			if (this.ShouldDrawRotated)
			{
				return rot.AsQuat;
			}
			return Quaternion.identity;
		}
	}
}
