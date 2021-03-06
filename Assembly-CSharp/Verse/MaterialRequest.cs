using System;
using UnityEngine;

namespace Verse
{
	public struct MaterialRequest : IEquatable<MaterialRequest>
	{
		public Shader shader;

		public Texture2D mainTex;

		public Color color;

		public Color colorTwo;

		public Texture2D maskTex;

		public int renderQueue;

		public string BaseTexPath
		{
			set
			{
				this.mainTex = ContentFinder<Texture2D>.Get(value, true);
			}
		}

		public MaterialRequest(Texture2D tex)
		{
			this.shader = ShaderDatabase.Cutout;
			this.mainTex = tex;
			this.color = Color.white;
			this.colorTwo = Color.white;
			this.maskTex = null;
			this.renderQueue = 0;
		}

		public MaterialRequest(Texture2D tex, Shader shader)
		{
			this.shader = shader;
			this.mainTex = tex;
			this.color = Color.white;
			this.colorTwo = Color.white;
			this.maskTex = null;
			this.renderQueue = 0;
		}

		public MaterialRequest(Texture2D tex, Shader shader, Color color)
		{
			this.shader = shader;
			this.mainTex = tex;
			this.color = color;
			this.colorTwo = Color.white;
			this.maskTex = null;
			this.renderQueue = 0;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine(seed, this.shader);
			seed = Gen.HashCombineStruct(seed, this.color);
			seed = Gen.HashCombineStruct(seed, this.colorTwo);
			seed = Gen.HashCombine(seed, this.mainTex);
			seed = Gen.HashCombine(seed, this.maskTex);
			return Gen.HashCombineInt(seed, this.renderQueue);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MaterialRequest))
			{
				return false;
			}
			return this.Equals((MaterialRequest)obj);
		}

		public bool Equals(MaterialRequest other)
		{
			return (UnityEngine.Object)other.shader == (UnityEngine.Object)this.shader && (UnityEngine.Object)other.mainTex == (UnityEngine.Object)this.mainTex && other.color == this.color && other.colorTwo == this.colorTwo && (UnityEngine.Object)other.maskTex == (UnityEngine.Object)this.maskTex && other.renderQueue == this.renderQueue;
		}

		public static bool operator ==(MaterialRequest lhs, MaterialRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(MaterialRequest lhs, MaterialRequest rhs)
		{
			return !(lhs == rhs);
		}

		public override string ToString()
		{
			return "MaterialRequest(" + this.shader.name + ", " + this.mainTex.name + ", " + this.color.ToString() + ", " + this.colorTwo.ToString() + ", " + this.maskTex.ToString() + ", " + this.renderQueue.ToString() + ")";
		}
	}
}
