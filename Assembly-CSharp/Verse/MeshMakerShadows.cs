using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	internal static class MeshMakerShadows
	{
		private static List<Vector3> vertsList = new List<Vector3>();

		private static List<Color32> colorsList = new List<Color32>();

		private static List<int> trianglesList = new List<int>();

		private static readonly Color32 LowVertexColor = new Color32(0, 0, 0, 0);

		public static Mesh NewShadowMesh(float baseWidth, float baseHeight, float tallness)
		{
			Color32 item = new Color32(255, 0, 0, (byte)(255.0 * tallness));
			float num = (float)(baseWidth / 2.0);
			float num2 = (float)(baseHeight / 2.0);
			MeshMakerShadows.vertsList.Clear();
			MeshMakerShadows.colorsList.Clear();
			MeshMakerShadows.trianglesList.Clear();
			MeshMakerShadows.vertsList.Add(new Vector3((float)(0.0 - num), 0f, (float)(0.0 - num2)));
			MeshMakerShadows.vertsList.Add(new Vector3((float)(0.0 - num), 0f, num2));
			MeshMakerShadows.vertsList.Add(new Vector3(num, 0f, num2));
			MeshMakerShadows.vertsList.Add(new Vector3(num, 0f, (float)(0.0 - num2)));
			MeshMakerShadows.colorsList.Add(MeshMakerShadows.LowVertexColor);
			MeshMakerShadows.colorsList.Add(MeshMakerShadows.LowVertexColor);
			MeshMakerShadows.colorsList.Add(MeshMakerShadows.LowVertexColor);
			MeshMakerShadows.colorsList.Add(MeshMakerShadows.LowVertexColor);
			MeshMakerShadows.trianglesList.Add(0);
			MeshMakerShadows.trianglesList.Add(1);
			MeshMakerShadows.trianglesList.Add(2);
			MeshMakerShadows.trianglesList.Add(0);
			MeshMakerShadows.trianglesList.Add(2);
			MeshMakerShadows.trianglesList.Add(3);
			int count = MeshMakerShadows.vertsList.Count;
			MeshMakerShadows.vertsList.Add(new Vector3((float)(0.0 - num), 0f, (float)(0.0 - num2)));
			MeshMakerShadows.colorsList.Add(item);
			MeshMakerShadows.vertsList.Add(new Vector3((float)(0.0 - num), 0f, num2));
			MeshMakerShadows.colorsList.Add(item);
			MeshMakerShadows.trianglesList.Add(0);
			MeshMakerShadows.trianglesList.Add(count);
			MeshMakerShadows.trianglesList.Add(count + 1);
			MeshMakerShadows.trianglesList.Add(0);
			MeshMakerShadows.trianglesList.Add(count + 1);
			MeshMakerShadows.trianglesList.Add(1);
			int count2 = MeshMakerShadows.vertsList.Count;
			MeshMakerShadows.vertsList.Add(new Vector3(num, 0f, num2));
			MeshMakerShadows.colorsList.Add(item);
			MeshMakerShadows.vertsList.Add(new Vector3(num, 0f, (float)(0.0 - num2)));
			MeshMakerShadows.colorsList.Add(item);
			MeshMakerShadows.trianglesList.Add(2);
			MeshMakerShadows.trianglesList.Add(count2);
			MeshMakerShadows.trianglesList.Add(count2 + 1);
			MeshMakerShadows.trianglesList.Add(count2 + 1);
			MeshMakerShadows.trianglesList.Add(3);
			MeshMakerShadows.trianglesList.Add(2);
			int count3 = MeshMakerShadows.vertsList.Count;
			MeshMakerShadows.vertsList.Add(new Vector3((float)(0.0 - num), 0f, (float)(0.0 - num2)));
			MeshMakerShadows.colorsList.Add(item);
			MeshMakerShadows.vertsList.Add(new Vector3(num, 0f, (float)(0.0 - num2)));
			MeshMakerShadows.colorsList.Add(item);
			MeshMakerShadows.trianglesList.Add(0);
			MeshMakerShadows.trianglesList.Add(3);
			MeshMakerShadows.trianglesList.Add(count3);
			MeshMakerShadows.trianglesList.Add(3);
			MeshMakerShadows.trianglesList.Add(count3 + 1);
			MeshMakerShadows.trianglesList.Add(count3);
			Mesh mesh = new Mesh();
			mesh.name = "NewShadowMesh()";
			mesh.vertices = MeshMakerShadows.vertsList.ToArray();
			mesh.colors32 = MeshMakerShadows.colorsList.ToArray();
			mesh.triangles = MeshMakerShadows.trianglesList.ToArray();
			return mesh;
		}
	}
}
