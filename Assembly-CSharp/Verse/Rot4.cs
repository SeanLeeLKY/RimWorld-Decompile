using System;
using UnityEngine;

namespace Verse
{
	public struct Rot4 : IEquatable<Rot4>
	{
		private byte rotInt;

		public bool IsValid
		{
			get
			{
				return this.rotInt < 100;
			}
		}

		public byte AsByte
		{
			get
			{
				return this.rotInt;
			}
			set
			{
				this.rotInt = (byte)((int)value % 4);
			}
		}

		public int AsInt
		{
			get
			{
				return this.rotInt;
			}
			set
			{
				if (value < 0)
				{
					value += 4000;
				}
				this.rotInt = (byte)(value % 4);
			}
		}

		public float AsAngle
		{
			get
			{
				switch (this.AsInt)
				{
				case 0:
					return 0f;
				case 1:
					return 90f;
				case 2:
					return 180f;
				case 3:
					return 270f;
				default:
					return 0f;
				}
			}
		}

		public Quaternion AsQuat
		{
			get
			{
				switch (this.rotInt)
				{
				case 0:
					return Quaternion.identity;
				case 1:
					return Quaternion.LookRotation(Vector3.right);
				case 2:
					return Quaternion.LookRotation(Vector3.back);
				case 3:
					return Quaternion.LookRotation(Vector3.left);
				default:
					Log.Error("ToQuat with Rot = " + this.AsInt);
					return Quaternion.identity;
				}
			}
		}

		public bool IsHorizontal
		{
			get
			{
				return this.rotInt == 1 || this.rotInt == 3;
			}
		}

		public static Rot4 North
		{
			get
			{
				return new Rot4(0);
			}
		}

		public static Rot4 East
		{
			get
			{
				return new Rot4(1);
			}
		}

		public static Rot4 South
		{
			get
			{
				return new Rot4(2);
			}
		}

		public static Rot4 West
		{
			get
			{
				return new Rot4(3);
			}
		}

		public static Rot4 Random
		{
			get
			{
				return new Rot4(Rand.RangeInclusive(0, 3));
			}
		}

		public static Rot4 Invalid
		{
			get
			{
				Rot4 result = default(Rot4);
				result.rotInt = 200;
				return result;
			}
		}

		public IntVec3 FacingCell
		{
			get
			{
				switch (this.AsInt)
				{
				case 0:
					return new IntVec3(0, 0, 1);
				case 1:
					return new IntVec3(1, 0, 0);
				case 2:
					return new IntVec3(0, 0, -1);
				case 3:
					return new IntVec3(-1, 0, 0);
				default:
					return default(IntVec3);
				}
			}
		}

		public Rot4 Opposite
		{
			get
			{
				switch (this.AsInt)
				{
				case 0:
					return new Rot4(2);
				case 1:
					return new Rot4(3);
				case 2:
					return new Rot4(0);
				case 3:
					return new Rot4(1);
				default:
					return default(Rot4);
				}
			}
		}

		public Rot4(byte newRot)
		{
			this.rotInt = newRot;
		}

		public Rot4(int newRot)
		{
			this.rotInt = (byte)(newRot % 4);
		}

		public void Rotate(RotationDirection RotDir)
		{
			if (RotDir == RotationDirection.Clockwise)
			{
				this.AsInt++;
			}
			if (RotDir == RotationDirection.Counterclockwise)
			{
				this.AsInt--;
			}
		}

		public static Rot4 FromAngleFlat(float angle)
		{
			angle = GenMath.PositiveMod(angle, 360f);
			if (angle < 45.0)
			{
				return Rot4.North;
			}
			if (angle < 135.0)
			{
				return Rot4.East;
			}
			if (angle < 225.0)
			{
				return Rot4.South;
			}
			if (angle < 315.0)
			{
				return Rot4.West;
			}
			return Rot4.North;
		}

		public static Rot4 FromIntVec3(IntVec3 offset)
		{
			if (offset.x == 1)
			{
				return Rot4.East;
			}
			if (offset.x == -1)
			{
				return Rot4.West;
			}
			if (offset.z == 1)
			{
				return Rot4.North;
			}
			if (offset.z == -1)
			{
				return Rot4.South;
			}
			Log.Error("FromIntVec3 with bad offset " + offset);
			return Rot4.North;
		}

		public static Rot4 FromIntVec2(IntVec2 offset)
		{
			return Rot4.FromIntVec3(offset.ToIntVec3);
		}

		public static bool operator ==(Rot4 a, Rot4 b)
		{
			return a.AsInt == b.AsInt;
		}

		public static bool operator !=(Rot4 a, Rot4 b)
		{
			return a.AsInt != b.AsInt;
		}

		public override int GetHashCode()
		{
			switch (this.rotInt)
			{
			case 0:
				return 235515;
			case 1:
				return 5612938;
			case 2:
				return 1215650;
			case 3:
				return 9231792;
			default:
				throw new InvalidOperationException("IntRot out of range.");
			}
		}

		public override string ToString()
		{
			return this.rotInt.ToString();
		}

		public string ToStringHuman()
		{
			switch (this.rotInt)
			{
			case 0:
				return "North".Translate();
			case 1:
				return "East".Translate();
			case 2:
				return "South".Translate();
			case 3:
				return "West".Translate();
			default:
				return "error";
			}
		}

		public static Rot4 FromString(string str)
		{
			int num = default(int);
			byte newRot;
			if (int.TryParse(str, out num))
			{
				newRot = (byte)num;
				goto IL_0093;
			}
			if (str != null)
			{
				if (!(str == "North"))
				{
					if (!(str == "East"))
					{
						if (!(str == "South"))
						{
							if (str == "West")
							{
								newRot = 3;
								goto IL_0093;
							}
							goto IL_007c;
						}
						newRot = 2;
					}
					else
					{
						newRot = 1;
					}
				}
				else
				{
					newRot = 0;
				}
				goto IL_0093;
			}
			goto IL_007c;
			IL_0093:
			return new Rot4(newRot);
			IL_007c:
			newRot = 0;
			Log.Error("Invalid rotation: " + str);
			goto IL_0093;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Rot4))
			{
				return false;
			}
			return this.Equals((Rot4)obj);
		}

		public bool Equals(Rot4 other)
		{
			return this.rotInt == other.rotInt;
		}
	}
}
