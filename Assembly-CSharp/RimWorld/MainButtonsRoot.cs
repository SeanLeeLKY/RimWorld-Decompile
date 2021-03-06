using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainButtonsRoot
	{
		public MainTabsRoot tabs = new MainTabsRoot();

		private List<MainButtonDef> allButtonsInOrder;

		private int VisibleButtonsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.allButtonsInOrder.Count; i++)
				{
					if (this.allButtonsInOrder[i].buttonVisible)
					{
						num++;
					}
				}
				return num;
			}
		}

		public MainButtonsRoot()
		{
			this.allButtonsInOrder = (from x in DefDatabase<MainButtonDef>.AllDefs
			orderby x.order
			select x).ToList();
		}

		public void MainButtonsOnGUI()
		{
			if (Event.current.type != EventType.Layout)
			{
				this.DoButtons();
				int num = 0;
				while (true)
				{
					if (num < this.allButtonsInOrder.Count)
					{
						if ((this.allButtonsInOrder[num].validWithoutMap || Find.VisibleMap != null) && this.allButtonsInOrder[num].hotKey != null && this.allButtonsInOrder[num].hotKey.KeyDownEvent)
							break;
						num++;
						continue;
					}
					return;
				}
				Event.current.Use();
				this.allButtonsInOrder[num].Worker.InterfaceTryActivate();
			}
		}

		public void HandleLowPriorityShortcuts()
		{
			this.tabs.HandleLowPriorityShortcuts();
			if (WorldRendererUtility.WorldRenderedNow && Current.ProgramState == ProgramState.Playing && Find.VisibleMap != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				Event.current.Use();
				Find.World.renderer.wantedMode = WorldRenderMode.None;
			}
		}

		private void DoButtons()
		{
			GUI.color = Color.white;
			int visibleButtonsCount = this.VisibleButtonsCount;
			int num = (int)((float)UI.screenWidth / (float)visibleButtonsCount);
			int num2 = this.allButtonsInOrder.FindLastIndex((MainButtonDef x) => x.buttonVisible);
			int num3 = 0;
			for (int i = 0; i < this.allButtonsInOrder.Count; i++)
			{
				if (this.allButtonsInOrder[i].buttonVisible)
				{
					int num4 = num;
					if (i == num2)
					{
						num4 = UI.screenWidth - num3;
					}
					Rect rect = new Rect((float)num3, (float)(UI.screenHeight - 35), (float)num4, 35f);
					this.allButtonsInOrder[i].Worker.DoButton(rect);
					num3 += num;
				}
			}
		}
	}
}
