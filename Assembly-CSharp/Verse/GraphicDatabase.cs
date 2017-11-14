using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class GraphicDatabase
	{
		private static Dictionary<GraphicRequest, Graphic> allGraphics = new Dictionary<GraphicRequest, Graphic>();

		public static Graphic Get<T>(string path) where T : Graphic, new()
		{
			GraphicRequest req = new GraphicRequest(typeof(T), path, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white, null, 0);
			return (Graphic)(object)GraphicDatabase.GetInner<T>(req);
		}

		public static Graphic Get<T>(string path, Shader shader) where T : Graphic, new()
		{
			GraphicRequest req = new GraphicRequest(typeof(T), path, shader, Vector2.one, Color.white, Color.white, null, 0);
			return (Graphic)(object)GraphicDatabase.GetInner<T>(req);
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color) where T : Graphic, new()
		{
			GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null, 0);
			return (Graphic)(object)GraphicDatabase.GetInner<T>(req);
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, int renderQueue) where T : Graphic, new()
		{
			GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, Color.white, null, renderQueue);
			return (Graphic)(object)GraphicDatabase.GetInner<T>(req);
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo) where T : Graphic, new()
		{
			GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, null, 0);
			return (Graphic)(object)GraphicDatabase.GetInner<T>(req);
		}

		public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data) where T : Graphic, new()
		{
			GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, colorTwo, data, 0);
			return (Graphic)(object)GraphicDatabase.GetInner<T>(req);
		}

		public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo)
		{
			return GraphicDatabase.Get(graphicClass, path, shader, drawSize, color, colorTwo, null);
		}

		public static Graphic Get(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData data)
		{
			GraphicRequest graphicRequest = new GraphicRequest(graphicClass, path, shader, drawSize, color, colorTwo, data, 0);
			if (graphicRequest.graphicClass == typeof(Graphic_Single))
			{
				return GraphicDatabase.GetInner<Graphic_Single>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Terrain))
			{
				return GraphicDatabase.GetInner<Graphic_Terrain>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Multi))
			{
				return GraphicDatabase.GetInner<Graphic_Multi>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Mote))
			{
				return GraphicDatabase.GetInner<Graphic_Mote>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Random))
			{
				return GraphicDatabase.GetInner<Graphic_Random>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Flicker))
			{
				return GraphicDatabase.GetInner<Graphic_Flicker>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_Appearances))
			{
				return GraphicDatabase.GetInner<Graphic_Appearances>(graphicRequest);
			}
			if (graphicRequest.graphicClass == typeof(Graphic_StackCount))
			{
				return GraphicDatabase.GetInner<Graphic_StackCount>(graphicRequest);
			}
			try
			{
				return (Graphic)GenGeneric.InvokeStaticGenericMethod(typeof(GraphicDatabase), graphicRequest.graphicClass, "GetInner", graphicRequest);
			}
			catch (Exception ex)
			{
				Log.Error("Exception getting " + graphicClass + " at " + path + ": " + ex.ToString());
			}
			return BaseContent.BadGraphic;
		}

		private static T GetInner<T>(GraphicRequest req) where T : Graphic, new()
		{
			Graphic graphic = default(Graphic);
			if (!GraphicDatabase.allGraphics.TryGetValue(req, out graphic))
			{
				graphic = (Graphic)(object)new T();
				graphic.Init(req);
				GraphicDatabase.allGraphics.Add(req, graphic);
			}
			return (T)graphic;
		}

		public static void Clear()
		{
			GraphicDatabase.allGraphics.Clear();
		}

		public static void DebugLogAllGraphics()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("There are " + GraphicDatabase.allGraphics.Count + " graphics loaded.");
			int num = 0;
			foreach (Graphic value in GraphicDatabase.allGraphics.Values)
			{
				stringBuilder.AppendLine(num + " - " + value.ToString());
				if (num % 50 == 49)
				{
					Log.Message(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
				}
				num++;
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
