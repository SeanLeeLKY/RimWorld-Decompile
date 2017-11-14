using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_Allowed : Area
	{
		private string labelInt;

		private Color colorInt = Color.red;

		public AllowedAreaMode mode = AllowedAreaMode.Humanlike;

		public override string Label
		{
			get
			{
				return this.labelInt;
			}
		}

		public override Color Color
		{
			get
			{
				return this.colorInt;
			}
		}

		public override bool Mutable
		{
			get
			{
				return true;
			}
		}

		public override int ListPriority
		{
			get
			{
				if (this.mode == AllowedAreaMode.Any)
				{
					return 1000;
				}
				if (this.mode == AllowedAreaMode.Humanlike)
				{
					return 900;
				}
				if (this.mode == AllowedAreaMode.Animal)
				{
					return 800;
				}
				return 500;
			}
		}

		public Area_Allowed()
		{
		}

		public Area_Allowed(AreaManager areaManager, AllowedAreaMode mode, string label = null)
			: base(areaManager)
		{
			base.areaManager = areaManager;
			this.mode = mode;
			if (!label.NullOrEmpty())
			{
				this.labelInt = label;
			}
			else
			{
				int num = 1;
				while (true)
				{
					if (mode == AllowedAreaMode.Animal)
					{
						this.labelInt = "AreaAnimalDefaultLabel".Translate(num);
					}
					else
					{
						this.labelInt = "AreaDefaultLabel".Translate(num);
					}
					if (areaManager.GetLabeled(this.labelInt) != null)
					{
						num++;
						continue;
					}
					break;
				}
			}
			this.colorInt = new Color(Rand.Value, Rand.Value, Rand.Value);
			this.colorInt = Color.Lerp(this.colorInt, Color.gray, 0.5f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.labelInt, "label", (string)null, false);
			Scribe_Values.Look<Color>(ref this.colorInt, "color", default(Color), false);
			Scribe_Values.Look<AllowedAreaMode>(ref this.mode, "mode", (AllowedAreaMode)0, false);
		}

		public override bool AssignableAsAllowed(AllowedAreaMode mode)
		{
			return (mode & this.mode) != (AllowedAreaMode)0;
		}

		public override void SetLabel(string label)
		{
			this.labelInt = label;
		}

		public override string GetUniqueLoadID()
		{
			return "Area_" + base.ID + "_Named_" + this.labelInt;
		}

		public override string ToString()
		{
			return this.labelInt;
		}
	}
}
