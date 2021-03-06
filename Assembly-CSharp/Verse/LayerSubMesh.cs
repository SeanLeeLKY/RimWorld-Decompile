using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class LayerSubMesh
	{
		public bool finalized;

		public bool disabled;

		public Material material;

		public Mesh mesh;

		public List<Vector3> verts = new List<Vector3>();

		public List<int> tris = new List<int>();

		public List<Color32> colors = new List<Color32>();

		public List<Vector3> uvs = new List<Vector3>();

		public LayerSubMesh(Mesh mesh, Material material)
		{
			this.mesh = mesh;
			this.material = material;
		}

		public void Clear(MeshParts parts)
		{
			if ((parts & MeshParts.Verts) != 0)
			{
				this.verts.Clear();
			}
			if ((parts & MeshParts.Tris) != 0)
			{
				this.tris.Clear();
			}
			if ((parts & MeshParts.Colors) != 0)
			{
				this.colors.Clear();
			}
			if ((parts & MeshParts.UVs) != 0)
			{
				this.uvs.Clear();
			}
			this.finalized = false;
		}

		public void FinalizeMesh(MeshParts parts)
		{
			if (this.finalized)
			{
				Log.Warning("Finalizing mesh which is already finalized. Did you forget to call Clear()?");
			}
			if ((parts & MeshParts.Verts) != 0 || (parts & MeshParts.Tris) != 0)
			{
				this.mesh.Clear();
			}
			if ((parts & MeshParts.Verts) != 0)
			{
				if (this.verts.Count > 0)
				{
					this.mesh.SetVertices(this.verts);
				}
				else
				{
					Log.Error("Cannot cook Verts for " + this.material.ToString() + ": no ingredients data. If you want to not render this submesh, disable it.");
				}
			}
			if ((parts & MeshParts.Tris) != 0)
			{
				if (this.tris.Count > 0)
				{
					this.mesh.SetTriangles(this.tris, 0);
				}
				else
				{
					Log.Error("Cannot cook Tris for " + this.material.ToString() + ": no ingredients data.");
				}
			}
			if ((parts & MeshParts.Colors) != 0 && this.colors.Count > 0)
			{
				this.mesh.SetColors(this.colors);
			}
			if ((parts & MeshParts.UVs) != 0 && this.uvs.Count > 0)
			{
				this.mesh.SetUVs(0, this.uvs);
			}
			this.finalized = true;
		}

		public override string ToString()
		{
			return "LayerSubMesh(" + this.material.ToString() + ")";
		}
	}
}
