using System.Collections.Generic;
using UnityEngine;

namespace RimWorld.Planet
{
	public static class IcosahedronGenerator
	{
		private static readonly TriangleIndices[] IcosahedronTris = new TriangleIndices[20]
		{
			new TriangleIndices(0, 11, 5),
			new TriangleIndices(0, 5, 1),
			new TriangleIndices(0, 1, 7),
			new TriangleIndices(0, 7, 10),
			new TriangleIndices(0, 10, 11),
			new TriangleIndices(1, 5, 9),
			new TriangleIndices(5, 11, 4),
			new TriangleIndices(11, 10, 2),
			new TriangleIndices(10, 7, 6),
			new TriangleIndices(7, 1, 8),
			new TriangleIndices(3, 9, 4),
			new TriangleIndices(3, 4, 2),
			new TriangleIndices(3, 2, 6),
			new TriangleIndices(3, 6, 8),
			new TriangleIndices(3, 8, 9),
			new TriangleIndices(4, 9, 5),
			new TriangleIndices(2, 4, 11),
			new TriangleIndices(6, 2, 10),
			new TriangleIndices(8, 6, 7),
			new TriangleIndices(9, 8, 1)
		};

		public static void GenerateIcosahedron(List<Vector3> outVerts, List<TriangleIndices> outTris, float radius, Vector3 viewCenter, float viewAngle)
		{
			float num = (float)((1.0 + Mathf.Sqrt(5f)) / 2.0);
			outVerts.Clear();
			outVerts.Add(new Vector3(-1f, num, 0f).normalized * radius);
			outVerts.Add(new Vector3(1f, num, 0f).normalized * radius);
			outVerts.Add(new Vector3(-1f, (float)(0.0 - num), 0f).normalized * radius);
			outVerts.Add(new Vector3(1f, (float)(0.0 - num), 0f).normalized * radius);
			outVerts.Add(new Vector3(0f, -1f, num).normalized * radius);
			outVerts.Add(new Vector3(0f, 1f, num).normalized * radius);
			outVerts.Add(new Vector3(0f, -1f, (float)(0.0 - num)).normalized * radius);
			outVerts.Add(new Vector3(0f, 1f, (float)(0.0 - num)).normalized * radius);
			outVerts.Add(new Vector3(num, 0f, -1f).normalized * radius);
			outVerts.Add(new Vector3(num, 0f, 1f).normalized * radius);
			outVerts.Add(new Vector3((float)(0.0 - num), 0f, -1f).normalized * radius);
			outVerts.Add(new Vector3((float)(0.0 - num), 0f, 1f).normalized * radius);
			outTris.Clear();
			int i = 0;
			int num2 = IcosahedronGenerator.IcosahedronTris.Length;
			for (; i < num2; i++)
			{
				TriangleIndices item = IcosahedronGenerator.IcosahedronTris[i];
				if (IcosahedronGenerator.IcosahedronFaceNeeded(item.v1, item.v2, item.v3, outVerts, radius, viewCenter, viewAngle))
				{
					outTris.Add(item);
				}
			}
			MeshUtility.RemoveUnusedVertices(outVerts, outTris);
		}

		private static bool IcosahedronFaceNeeded(int v1, int v2, int v3, List<Vector3> verts, float radius, Vector3 viewCenter, float viewAngle)
		{
			viewAngle = (float)(viewAngle + 18.0);
			return MeshUtility.Visible(verts[v1], radius, viewCenter, viewAngle) || MeshUtility.Visible(verts[v2], radius, viewCenter, viewAngle) || MeshUtility.Visible(verts[v3], radius, viewCenter, viewAngle);
		}
	}
}
