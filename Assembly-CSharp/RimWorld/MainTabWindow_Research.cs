using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Research : MainTabWindow
	{
		protected ResearchProjectDef selectedProject;

		private bool noBenchWarned;

		private bool requiredByThisFound;

		private Vector2 leftScrollPosition = Vector2.zero;

		private float leftScrollViewHeight;

		private Vector2 rightScrollPosition = default(Vector2);

		private float rightViewWidth;

		private float rightViewHeight;

		private ResearchTabDef curTabInt;

		private const float LeftAreaWidth = 200f;

		private const int ModeSelectButHeight = 40;

		private const float ProjectTitleHeight = 50f;

		private const float ProjectTitleLeftMargin = 0f;

		private const float localPadding = 20f;

		private const int ResearchItemW = 140;

		private const int ResearchItemH = 50;

		private const int ResearchItemPaddingW = 50;

		private const int ResearchItemPaddingH = 50;

		private const float PrereqsLineSpacing = 15f;

		private const int ColumnMaxProjects = 6;

		private static readonly Texture2D ResearchBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

		private static readonly Texture2D ResearchBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

		private static readonly Color FulfilledPrerequisiteColor = Color.green;

		private static readonly Color MissingPrerequisiteColor = Color.red;

		private static readonly Color ProjectWithMissingPrerequisiteLabelColor = Color.gray;

		private static List<Building> tmpAllBuildings = new List<Building>();

		private ResearchTabDef CurTab
		{
			get
			{
				return this.curTabInt;
			}
			set
			{
				if (value != this.curTabInt)
				{
					this.curTabInt = value;
					Vector2 vector = this.ViewSize(this.CurTab);
					this.rightViewWidth = vector.x;
					this.rightViewHeight = vector.y;
					this.rightScrollPosition = Vector2.zero;
				}
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 initialSize = base.InitialSize;
				float b = (float)(UI.screenHeight - 35);
				float b2 = (float)(this.Margin + 10.0 + 32.0 + 10.0 + DefDatabase<ResearchTabDef>.AllDefs.Max(delegate(ResearchTabDef tab)
				{
					Vector2 vector = this.ViewSize(tab);
					return vector.y;
				}) + 10.0 + 10.0 + this.Margin);
				float a = Mathf.Max(initialSize.y, b2);
				initialSize.y = Mathf.Min(a, b);
				return initialSize;
			}
		}

		private Vector2 ViewSize(ResearchTabDef tab)
		{
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			float num = 0f;
			float num2 = 0f;
			Text.Font = GameFont.Small;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ResearchProjectDef researchProjectDef = allDefsListForReading[i];
				if (researchProjectDef.tab == tab)
				{
					Rect rect = new Rect(0f, 0f, 140f, 0f);
					Widgets.LabelCacheHeight(ref rect, this.GetLabel(researchProjectDef), false, false);
					num = Mathf.Max(num, (float)(this.PosX(researchProjectDef) + 140.0));
					num2 = Mathf.Max(num2, this.PosY(researchProjectDef) + rect.height);
				}
			}
			return new Vector2((float)(num + 20.0), (float)(num2 + 20.0));
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.selectedProject = Find.ResearchManager.currentProj;
			if (this.CurTab == null)
			{
				if (this.selectedProject != null)
				{
					this.CurTab = this.selectedProject.tab;
				}
				else
				{
					this.CurTab = ResearchTabDefOf.Main;
				}
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			base.windowRect.width = (float)UI.screenWidth;
			if (!this.noBenchWarned)
			{
				bool flag = false;
				List<Map> maps = Find.Maps;
				int num = 0;
				while (num < maps.Count)
				{
					if (!maps[num].listerBuildings.ColonistsHaveResearchBench())
					{
						num++;
						continue;
					}
					flag = true;
					break;
				}
				if (!flag)
				{
					Find.WindowStack.Add(new Dialog_MessageBox("ResearchMenuWithoutBench".Translate(), null, null, null, null, null, false));
				}
				this.noBenchWarned = true;
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Rect leftOutRect = new Rect(0f, 0f, 200f, inRect.height);
			Rect rightOutRect = new Rect((float)(leftOutRect.xMax + 10.0), 0f, (float)(inRect.width - leftOutRect.width - 10.0), inRect.height);
			this.DrawLeftRect(leftOutRect);
			this.DrawRightRect(rightOutRect);
		}

		private void DrawLeftRect(Rect leftOutRect)
		{
			Rect position = leftOutRect;
			GUI.BeginGroup(position);
			if (this.selectedProject != null)
			{
				Rect outRect = new Rect(0f, 0f, position.width, 500f);
				Rect viewRect = new Rect(0f, 0f, (float)(outRect.width - 16.0), this.leftScrollViewHeight);
				Widgets.BeginScrollView(outRect, ref this.leftScrollPosition, viewRect, true);
				float num = 0f;
				Text.Font = GameFont.Medium;
				GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
				Rect rect = new Rect(0f, num, viewRect.width, 50f);
				Widgets.LabelCacheHeight(ref rect, this.selectedProject.LabelCap, true, false);
				GenUI.ResetLabelAlign();
				Text.Font = GameFont.Small;
				num += rect.height;
				Rect rect2 = new Rect(0f, num, viewRect.width, 0f);
				Widgets.LabelCacheHeight(ref rect2, this.selectedProject.description, true, false);
				num = (float)(num + (rect2.height + 10.0));
				string text = "ProjectTechLevel".Translate().CapitalizeFirst() + ": " + this.selectedProject.techLevel.ToStringHuman().CapitalizeFirst() + "\n" + "YourTechLevel".Translate().CapitalizeFirst() + ": " + Faction.OfPlayer.def.techLevel.ToStringHuman().CapitalizeFirst();
				float num2 = this.selectedProject.CostFactor(Faction.OfPlayer.def.techLevel);
				if (num2 != 1.0)
				{
					string text2 = text;
					text = text2 + "\n\n" + "ResearchCostMultiplier".Translate().CapitalizeFirst() + ": " + num2.ToStringPercent() + "\n" + "ResearchCostComparison".Translate(this.selectedProject.baseCost.ToString("F0"), this.selectedProject.CostApparent.ToString("F0"));
				}
				Rect rect3 = new Rect(0f, num, viewRect.width, 0f);
				Widgets.LabelCacheHeight(ref rect3, text, true, false);
				num = (float)(rect3.yMax + 10.0);
				Rect rect4 = new Rect(0f, num, viewRect.width, 500f);
				float num3 = this.DrawResearchPrereqs(this.selectedProject, rect4);
				if (num3 > 0.0)
				{
					num = (float)(num + (num3 + 15.0));
				}
				Rect rect5 = new Rect(0f, num, viewRect.width, 500f);
				num += this.DrawResearchBenchRequirements(this.selectedProject, rect5);
				num = (this.leftScrollViewHeight = (float)(num + 3.0));
				Widgets.EndScrollView();
				bool flag = Prefs.DevMode && this.selectedProject.PrerequisitesCompleted && this.selectedProject != Find.ResearchManager.currentProj && !this.selectedProject.IsFinished;
				Rect rect6 = new Rect(0f, 0f, 90f, 50f);
				if (flag)
				{
					rect6.x = (float)((outRect.width - (rect6.width * 2.0 + 20.0)) / 2.0);
				}
				else
				{
					rect6.x = (float)((outRect.width - rect6.width) / 2.0);
				}
				rect6.y = (float)(outRect.y + outRect.height + 20.0);
				if (this.selectedProject.IsFinished)
				{
					Widgets.DrawMenuSection(rect6);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect6, "Finished".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
				else if (this.selectedProject == Find.ResearchManager.currentProj)
				{
					Widgets.DrawMenuSection(rect6);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect6, "InProgress".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
				else if (!this.selectedProject.CanStartNow)
				{
					Widgets.DrawMenuSection(rect6);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect6, "Locked".Translate());
					Text.Anchor = TextAnchor.UpperLeft;
				}
				else if (Widgets.ButtonText(rect6, "Research".Translate(), true, false, true))
				{
					SoundDef.Named("ResearchStart").PlayOneShotOnCamera(null);
					Find.ResearchManager.currentProj = this.selectedProject;
					TutorSystem.Notify_Event("StartResearchProject");
				}
				if (flag)
				{
					Rect rect7 = rect6;
					rect7.x += (float)(rect7.width + 20.0);
					if (Widgets.ButtonText(rect7, "Debug Insta-finish", true, false, true))
					{
						Find.ResearchManager.currentProj = this.selectedProject;
						Find.ResearchManager.InstantFinish(this.selectedProject, false);
					}
				}
				Rect rect8 = new Rect(15f, (float)(rect6.y + rect6.height + 20.0), (float)(position.width - 30.0), 35f);
				Widgets.FillableBar(rect8, this.selectedProject.ProgressPercent, MainTabWindow_Research.ResearchBarFillTex, MainTabWindow_Research.ResearchBarBGTex, true);
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect8, this.selectedProject.ProgressApparent.ToString("F0") + " / " + this.selectedProject.CostApparent.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
			}
			GUI.EndGroup();
		}

		private float CoordToPixelsX(float x)
		{
			return (float)(x * 190.0);
		}

		private float CoordToPixelsY(float y)
		{
			return (float)(y * 100.0);
		}

		private float PosX(ResearchProjectDef d)
		{
			return this.CoordToPixelsX(d.ResearchViewX);
		}

		private float PosY(ResearchProjectDef d)
		{
			return this.CoordToPixelsY(d.ResearchViewY);
		}

		private void DrawRightRect(Rect rightOutRect)
		{
			rightOutRect.yMin += 32f;
			Widgets.DrawMenuSection(rightOutRect);
			List<TabRecord> list = new List<TabRecord>();
			foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				ResearchTabDef localTabDef = allDef;
				list.Add(new TabRecord(localTabDef.LabelCap, delegate
				{
					this.CurTab = localTabDef;
				}, this.CurTab == localTabDef));
			}
			TabDrawer.DrawTabs(rightOutRect, list);
			Rect outRect = rightOutRect.ContractedBy(10f);
			Rect rect = new Rect(0f, 0f, this.rightViewWidth, this.rightViewHeight);
			Rect rect2 = rect.ContractedBy(10f);
			rect.width = this.rightViewWidth;
			rect2 = rect.ContractedBy(10f);
			Vector2 start = default(Vector2);
			Vector2 end = default(Vector2);
			Widgets.ScrollHorizontal(outRect, ref this.rightScrollPosition, rect, 20f);
			Widgets.BeginScrollView(outRect, ref this.rightScrollPosition, rect, true);
			GUI.BeginGroup(rect2);
			List<ResearchProjectDef> allDefsListForReading = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					ResearchProjectDef researchProjectDef = allDefsListForReading[j];
					if (researchProjectDef.tab == this.CurTab)
					{
						start.x = this.PosX(researchProjectDef);
						start.y = (float)(this.PosY(researchProjectDef) + 25.0);
						for (int k = 0; k < researchProjectDef.prerequisites.CountAllowNull(); k++)
						{
							ResearchProjectDef researchProjectDef2 = researchProjectDef.prerequisites[k];
							if (researchProjectDef2 != null && researchProjectDef2.tab == this.CurTab)
							{
								end.x = (float)(this.PosX(researchProjectDef2) + 140.0);
								end.y = (float)(this.PosY(researchProjectDef2) + 25.0);
								if (this.selectedProject == researchProjectDef || this.selectedProject == researchProjectDef2)
								{
									if (i == 1)
									{
										Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
									}
								}
								else if (i == 0)
								{
									Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
								}
							}
						}
					}
				}
			}
			for (int l = 0; l < allDefsListForReading.Count; l++)
			{
				ResearchProjectDef researchProjectDef3 = allDefsListForReading[l];
				if (researchProjectDef3.tab == this.CurTab)
				{
					Rect source = new Rect(this.PosX(researchProjectDef3), this.PosY(researchProjectDef3), 140f, 50f);
					string label = this.GetLabel(researchProjectDef3);
					Rect rect3 = new Rect(source);
					Color textColor = Widgets.NormalOptionColor;
					Color color = default(Color);
					Color color2 = default(Color);
					bool flag = !researchProjectDef3.IsFinished && !researchProjectDef3.CanStartNow;
					if (researchProjectDef3 == Find.ResearchManager.currentProj)
					{
						color = TexUI.ActiveResearchColor;
					}
					else if (researchProjectDef3.IsFinished)
					{
						color = TexUI.FinishedResearchColor;
					}
					else if (flag)
					{
						color = TexUI.LockedResearchColor;
					}
					else if (researchProjectDef3.CanStartNow)
					{
						color = TexUI.AvailResearchColor;
					}
					if (this.selectedProject == researchProjectDef3)
					{
						color += TexUI.HighlightBgResearchColor;
						color2 = TexUI.HighlightBorderResearchColor;
					}
					else
					{
						color2 = TexUI.DefaultBorderResearchColor;
					}
					if (flag)
					{
						textColor = MainTabWindow_Research.ProjectWithMissingPrerequisiteLabelColor;
					}
					for (int m = 0; m < researchProjectDef3.prerequisites.CountAllowNull(); m++)
					{
						ResearchProjectDef researchProjectDef4 = researchProjectDef3.prerequisites[m];
						if (researchProjectDef4 != null && this.selectedProject == researchProjectDef4)
						{
							color2 = TexUI.HighlightLineResearchColor;
						}
					}
					if (this.requiredByThisFound)
					{
						for (int n = 0; n < researchProjectDef3.requiredByThis.CountAllowNull(); n++)
						{
							ResearchProjectDef researchProjectDef5 = researchProjectDef3.requiredByThis[n];
							if (this.selectedProject == researchProjectDef5)
							{
								color2 = TexUI.HighlightLineResearchColor;
							}
						}
					}
					if (Widgets.CustomButtonText(ref rect3, label, color, textColor, color2, true, 1, true, true))
					{
						SoundDefOf.Click.PlayOneShotOnCamera(null);
						this.selectedProject = researchProjectDef3;
					}
				}
			}
			GUI.EndGroup();
			Widgets.EndScrollView();
		}

		private float DrawResearchPrereqs(ResearchProjectDef project, Rect rect)
		{
			if (project.prerequisites.NullOrEmpty())
			{
				return 0f;
			}
			float yMin = rect.yMin;
			Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":", true, false);
			rect.yMin += rect.height;
			for (int i = 0; i < project.prerequisites.Count; i++)
			{
				this.SetPrerequisiteStatusColor(project.prerequisites[i].IsFinished, project);
				Widgets.LabelCacheHeight(ref rect, "  " + project.prerequisites[i].LabelCap, true, false);
				rect.yMin += rect.height;
			}
			GUI.color = Color.white;
			return rect.yMin - yMin;
		}

		private float DrawResearchBenchRequirements(ResearchProjectDef project, Rect rect)
		{
			float yMin = rect.yMin;
			if (project.requiredResearchBuilding != null)
			{
				bool present = false;
				List<Map> maps = Find.Maps;
				int num = 0;
				while (num < maps.Count)
				{
					if (maps[num].listerBuildings.allBuildingsColonist.Find((Building x) => x.def == project.requiredResearchBuilding) == null)
					{
						num++;
						continue;
					}
					present = true;
					break;
				}
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBench".Translate() + ":", true, false);
				rect.yMin += rect.height;
				this.SetPrerequisiteStatusColor(present, project);
				Widgets.LabelCacheHeight(ref rect, "  " + project.requiredResearchBuilding.LabelCap, true, false);
				rect.yMin += rect.height;
				GUI.color = Color.white;
			}
			if (!project.requiredResearchFacilities.NullOrEmpty())
			{
				Widgets.LabelCacheHeight(ref rect, "RequiredResearchBenchFacilities".Translate() + ":", true, false);
				rect.yMin += rect.height;
				Building_ResearchBench building_ResearchBench = this.FindBenchFulfillingMostRequirements(project.requiredResearchBuilding, project.requiredResearchFacilities);
				CompAffectedByFacilities bestMatchingBench = null;
				if (building_ResearchBench != null)
				{
					bestMatchingBench = building_ResearchBench.TryGetComp<CompAffectedByFacilities>();
				}
				for (int i = 0; i < project.requiredResearchFacilities.Count; i++)
				{
					this.DrawResearchBenchFacilityRequirement(project.requiredResearchFacilities[i], bestMatchingBench, project, ref rect);
					rect.yMin += 15f;
				}
			}
			GUI.color = Color.white;
			return rect.yMin - yMin;
		}

		private string GetLabel(ResearchProjectDef r)
		{
			return r.LabelCap + "\n(" + r.CostApparent.ToString("F0") + ")";
		}

		private void SetPrerequisiteStatusColor(bool present, ResearchProjectDef project)
		{
			if (!project.IsFinished)
			{
				if (present)
				{
					GUI.color = MainTabWindow_Research.FulfilledPrerequisiteColor;
				}
				else
				{
					GUI.color = MainTabWindow_Research.MissingPrerequisiteColor;
				}
			}
		}

		private void DrawResearchBenchFacilityRequirement(ThingDef requiredFacility, CompAffectedByFacilities bestMatchingBench, ResearchProjectDef project, ref Rect rect)
		{
			Thing thing = null;
			Thing thing2 = null;
			if (bestMatchingBench != null)
			{
				thing = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility);
				thing2 = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility && bestMatchingBench.IsFacilityActive(x));
			}
			this.SetPrerequisiteStatusColor(thing2 != null, project);
			string text = requiredFacility.LabelCap;
			if (thing != null && thing2 == null)
			{
				text = text + " (" + "InactiveFacility".Translate() + ")";
			}
			Widgets.LabelCacheHeight(ref rect, "  " + text, true, false);
		}

		private Building_ResearchBench FindBenchFulfillingMostRequirements(ThingDef requiredResearchBench, List<ThingDef> requiredFacilities)
		{
			MainTabWindow_Research.tmpAllBuildings.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				MainTabWindow_Research.tmpAllBuildings.AddRange(maps[i].listerBuildings.allBuildingsColonist);
			}
			float num = 0f;
			Building_ResearchBench building_ResearchBench = null;
			for (int j = 0; j < MainTabWindow_Research.tmpAllBuildings.Count; j++)
			{
				Building_ResearchBench building_ResearchBench2 = MainTabWindow_Research.tmpAllBuildings[j] as Building_ResearchBench;
				if (building_ResearchBench2 != null && (requiredResearchBench == null || building_ResearchBench2.def == requiredResearchBench))
				{
					float researchBenchRequirementsScore = this.GetResearchBenchRequirementsScore(building_ResearchBench2, requiredFacilities);
					if (building_ResearchBench == null || researchBenchRequirementsScore > num)
					{
						num = researchBenchRequirementsScore;
						building_ResearchBench = building_ResearchBench2;
					}
				}
			}
			MainTabWindow_Research.tmpAllBuildings.Clear();
			return building_ResearchBench;
		}

		private float GetResearchBenchRequirementsScore(Building_ResearchBench bench, List<ThingDef> requiredFacilities)
		{
			float num = 0f;
			int i;
			for (i = 0; i < requiredFacilities.Count; i++)
			{
				CompAffectedByFacilities benchComp = bench.GetComp<CompAffectedByFacilities>();
				if (benchComp != null)
				{
					List<Thing> linkedFacilitiesListForReading = benchComp.LinkedFacilitiesListForReading;
					if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i] && benchComp.IsFacilityActive(x)) != null)
					{
						num = (float)(num + 1.0);
					}
					else if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i]) != null)
					{
						num = (float)(num + 0.60000002384185791);
					}
				}
			}
			return num;
		}
	}
}
