using UnityEngine;

namespace Verse
{
	public static class Printer_Shadow
	{
		private static readonly Color32 LowVertexColor = new Color32(0, 0, 0, 0);

		public static void PrintShadow(SectionLayer layer, Vector3 center, ShadowData shadow, Rot4 rotation)
		{
			Printer_Shadow.PrintShadow(layer, center, shadow.volume, rotation);
		}

		public static void PrintShadow(SectionLayer layer, Vector3 center, Vector3 volume, Rot4 rotation)
		{
			if (DebugViewSettings.drawShadows)
			{
				LayerSubMesh subMesh = layer.GetSubMesh(MatBases.SunShadowFade);
				Color32 item = new Color32(255, 0, 0, (byte)Mathf.Min((float)(255.0 * volume.y), 255f));
				Vector3 vector = volume.RotatedBy(rotation) / 2f;
				float x = center.x;
				float z = center.z;
				int count = subMesh.verts.Count;
				subMesh.verts.Add(new Vector3(x - vector.x, 0f, z - vector.z));
				subMesh.verts.Add(new Vector3(x - vector.x, 0f, z + vector.z));
				subMesh.verts.Add(new Vector3(x + vector.x, 0f, z + vector.z));
				subMesh.verts.Add(new Vector3(x + vector.x, 0f, z - vector.z));
				subMesh.colors.Add(Printer_Shadow.LowVertexColor);
				subMesh.colors.Add(Printer_Shadow.LowVertexColor);
				subMesh.colors.Add(Printer_Shadow.LowVertexColor);
				subMesh.colors.Add(Printer_Shadow.LowVertexColor);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count + 3);
				int count2 = subMesh.verts.Count;
				subMesh.verts.Add(new Vector3(x - vector.x, 0f, z - vector.z));
				subMesh.verts.Add(new Vector3(x - vector.x, 0f, z + vector.z));
				subMesh.colors.Add(item);
				subMesh.colors.Add(item);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count2);
				subMesh.tris.Add(count2 + 1);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count2 + 1);
				subMesh.tris.Add(count + 1);
				int count3 = subMesh.verts.Count;
				subMesh.verts.Add(new Vector3(x + vector.x, 0f, z + vector.z));
				subMesh.verts.Add(new Vector3(x + vector.x, 0f, z - vector.z));
				subMesh.colors.Add(item);
				subMesh.colors.Add(item);
				subMesh.tris.Add(count + 2);
				subMesh.tris.Add(count3);
				subMesh.tris.Add(count3 + 1);
				subMesh.tris.Add(count3 + 1);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count + 2);
				int count4 = subMesh.verts.Count;
				subMesh.verts.Add(new Vector3(x - vector.x, 0f, z - vector.z));
				subMesh.verts.Add(new Vector3(x + vector.x, 0f, z - vector.z));
				subMesh.colors.Add(item);
				subMesh.colors.Add(item);
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count4);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count4 + 1);
				subMesh.tris.Add(count4);
			}
		}
	}
}
