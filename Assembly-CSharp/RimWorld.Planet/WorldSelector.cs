using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	public class WorldSelector
	{
		public WorldDragBox dragBox = new WorldDragBox();

		private List<WorldObject> selected = new List<WorldObject>();

		public int selectedTile = -1;

		private const int MaxNumSelected = 80;

		private const float MaxDragBoxDiagonalToSelectTile = 30f;

		private bool ShiftIsHeld
		{
			get
			{
				return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			}
		}

		public List<WorldObject> SelectedObjects
		{
			get
			{
				return this.selected;
			}
		}

		public WorldObject SingleSelectedObject
		{
			get
			{
				if (this.selected.Count != 1)
				{
					return null;
				}
				return this.selected[0];
			}
		}

		public WorldObject FirstSelectedObject
		{
			get
			{
				if (this.selected.Count == 0)
				{
					return null;
				}
				return this.selected[0];
			}
		}

		public int NumSelectedObjects
		{
			get
			{
				return this.selected.Count;
			}
		}

		public bool AnyObjectOrTileSelected
		{
			get
			{
				return this.NumSelectedObjects != 0 || this.selectedTile >= 0;
			}
		}

		public void WorldSelectorOnGUI()
		{
			this.HandleWorldClicks();
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && this.selected.Count > 0)
			{
				this.ClearSelection();
				Event.current.Use();
			}
		}

		private void HandleWorldClicks()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
				{
					if (Event.current.clickCount == 1)
					{
						this.dragBox.active = true;
						this.dragBox.start = UI.MousePositionOnUIInverted;
					}
					if (Event.current.clickCount == 2)
					{
						this.SelectAllMatchingObjectUnderMouseOnScreen();
					}
					Event.current.Use();
				}
				if (Event.current.button == 1 && this.selected.Count > 0)
				{
					if (this.selected.Count == 1 && this.selected[0] is Caravan)
					{
						Caravan caravan = (Caravan)this.selected[0];
						if (caravan.IsPlayerControlled && !FloatMenuMakerWorld.TryMakeFloatMenu(caravan))
						{
							this.AutoOrderToTile(caravan, GenWorld.MouseTile(false));
						}
					}
					else
					{
						for (int i = 0; i < this.selected.Count; i++)
						{
							Caravan caravan2 = this.selected[i] as Caravan;
							if (caravan2 != null && caravan2.IsPlayerControlled)
							{
								this.AutoOrderToTile(caravan2, GenWorld.MouseTile(false));
							}
						}
					}
					Event.current.Use();
				}
			}
			if (Event.current.rawType == EventType.MouseUp)
			{
				if (Event.current.button == 0 && this.dragBox.active)
				{
					this.dragBox.active = false;
					if (!this.dragBox.IsValid)
					{
						this.SelectUnderMouse(true);
					}
					else
					{
						this.SelectInsideDragBox();
					}
				}
				Event.current.Use();
			}
		}

		public bool IsSelected(WorldObject obj)
		{
			return this.selected.Contains(obj);
		}

		public void ClearSelection()
		{
			WorldSelectionDrawer.Clear();
			this.selected.Clear();
			this.selectedTile = -1;
		}

		public void Deselect(WorldObject obj)
		{
			if (this.selected.Contains(obj))
			{
				this.selected.Remove(obj);
			}
		}

		public void Select(WorldObject obj, bool playSound = true)
		{
			if (obj == null)
			{
				Log.Error("Cannot select null.");
			}
			else
			{
				this.selectedTile = -1;
				if (this.selected.Count < 80 && !this.IsSelected(obj))
				{
					if (playSound)
					{
						this.PlaySelectionSoundFor(obj);
					}
					this.selected.Add(obj);
					WorldSelectionDrawer.Notify_Selected(obj);
				}
			}
		}

		public void Notify_DialogOpened()
		{
			this.dragBox.active = false;
		}

		private void PlaySelectionSoundFor(WorldObject obj)
		{
			SoundDefOf.ThingSelected.PlayOneShotOnCamera(null);
		}

		private void SelectInsideDragBox()
		{
			if (!this.ShiftIsHeld)
			{
				this.ClearSelection();
			}
			bool flag = false;
			if (Current.ProgramState == ProgramState.Playing)
			{
				List<Caravan> list = Find.ColonistBar.CaravanMembersCaravansInScreenRect(this.dragBox.ScreenRect);
				for (int i = 0; i < list.Count; i++)
				{
					flag = true;
					this.Select(list[i], true);
				}
			}
			if (!flag && Current.ProgramState == ProgramState.Playing)
			{
				List<Thing> list2 = Find.ColonistBar.MapColonistsOrCorpsesInScreenRect(this.dragBox.ScreenRect);
				for (int j = 0; j < list2.Count; j++)
				{
					if (!flag)
					{
						CameraJumper.TryJumpAndSelect(list2[j]);
						flag = true;
					}
					else
					{
						Find.Selector.Select(list2[j], true, true);
					}
				}
			}
			if (!flag)
			{
				List<WorldObject> list3 = WorldObjectSelectionUtility.MultiSelectableWorldObjectsInScreenRectDistinct(this.dragBox.ScreenRect).ToList();
				if (list3.Any((WorldObject x) => x is Caravan))
				{
					list3.RemoveAll((WorldObject x) => !(x is Caravan));
					if (list3.Any((WorldObject x) => x.Faction == Faction.OfPlayer))
					{
						list3.RemoveAll((WorldObject x) => x.Faction != Faction.OfPlayer);
					}
				}
				for (int k = 0; k < list3.Count; k++)
				{
					flag = true;
					this.Select(list3[k], true);
				}
			}
			if (!flag)
			{
				bool canSelectTile = this.dragBox.Diagonal < 30.0;
				this.SelectUnderMouse(canSelectTile);
			}
		}

		public IEnumerable<WorldObject> SelectableObjectsUnderMouse()
		{
			bool flag = default(bool);
			return this.SelectableObjectsUnderMouse(out flag, out flag);
		}

		public IEnumerable<WorldObject> SelectableObjectsUnderMouse(out bool clickedDirectlyOnCaravan, out bool usedColonistBar)
		{
			Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
			if (Current.ProgramState == ProgramState.Playing)
			{
				Caravan caravan = Find.ColonistBar.CaravanMemberCaravanAt(mousePositionOnUIInverted);
				if (caravan != null)
				{
					clickedDirectlyOnCaravan = true;
					usedColonistBar = true;
					return Gen.YieldSingle((WorldObject)caravan);
				}
			}
			List<WorldObject> list = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
			clickedDirectlyOnCaravan = false;
			if (list.Count > 0 && list[0] is Caravan && list[0].DistanceToMouse(UI.MousePositionOnUI) < GenWorldUI.CaravanDirectClickRadius)
			{
				clickedDirectlyOnCaravan = true;
				for (int num = list.Count - 1; num >= 0; num--)
				{
					WorldObject worldObject = list[num];
					if (worldObject is Caravan && worldObject.DistanceToMouse(UI.MousePositionOnUI) > GenWorldUI.CaravanDirectClickRadius)
					{
						list.Remove(worldObject);
					}
				}
			}
			usedColonistBar = false;
			return list;
		}

		public static IEnumerable<WorldObject> SelectableObjectsAt(int tileID)
		{
			foreach (WorldObject item in Find.WorldObjects.ObjectsAt(tileID))
			{
				if (item.SelectableNow)
				{
					yield return item;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00ce:
			/*Error near IL_00cf: Unexpected return in MoveNext()*/;
		}

		private void SelectUnderMouse(bool canSelectTile = true)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Thing thing = Find.ColonistBar.ColonistOrCorpseAt(UI.MousePositionOnUIInverted);
				Pawn pawn = thing as Pawn;
				if (thing != null && (pawn == null || !pawn.IsCaravanMember()))
				{
					if (thing.Spawned)
					{
						CameraJumper.TryJumpAndSelect(thing);
					}
					else
					{
						CameraJumper.TryJump(thing);
					}
					return;
				}
			}
			bool flag = default(bool);
			bool flag2 = default(bool);
			List<WorldObject> list = this.SelectableObjectsUnderMouse(out flag, out flag2).ToList();
			if (flag2 || (flag && list.Count >= 2))
			{
				canSelectTile = false;
			}
			if (list.Count == 0)
			{
				if (!this.ShiftIsHeld)
				{
					this.ClearSelection();
					if (canSelectTile)
					{
						this.selectedTile = GenWorld.MouseTile(false);
					}
				}
			}
			else
			{
				WorldObject worldObject = (from obj in list
				where this.selected.Contains(obj)
				select obj).FirstOrDefault();
				if (worldObject != null)
				{
					if (!this.ShiftIsHeld)
					{
						int tile = (!canSelectTile) ? (-1) : GenWorld.MouseTile(false);
						this.SelectFirstOrNextFrom(list, tile);
					}
					else
					{
						foreach (WorldObject item in list)
						{
							if (this.selected.Contains(item))
							{
								this.Deselect(item);
							}
						}
					}
				}
				else
				{
					if (!this.ShiftIsHeld)
					{
						this.ClearSelection();
					}
					this.Select(list[0], true);
				}
			}
		}

		public void SelectFirstOrNextAt(int tileID)
		{
			this.SelectFirstOrNextFrom(WorldSelector.SelectableObjectsAt(tileID).ToList(), tileID);
		}

		private void SelectAllMatchingObjectUnderMouseOnScreen()
		{
			List<WorldObject> list = this.SelectableObjectsUnderMouse().ToList();
			if (list.Count != 0)
			{
				Type type = list[0].GetType();
				List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
				for (int i = 0; i < allWorldObjects.Count; i++)
				{
					if (type == allWorldObjects[i].GetType() && (allWorldObjects[i] == list[0] || allWorldObjects[i].AllMatchingObjectsOnScreenMatchesWith(list[0])) && allWorldObjects[i].VisibleToCameraNow())
					{
						this.Select(allWorldObjects[i], true);
					}
				}
			}
		}

		private void AutoOrderToTile(Caravan c, int tile)
		{
			if (tile >= 0)
			{
				if (c.autoJoinable && CaravanExitMapUtility.IsTheOnlyJoinableCaravanForAnyPrisonerOrAnimal(c))
				{
					CaravanExitMapUtility.OpenTheOnlyJoinableCaravanForPrisonerOrAnimalDialog(c, delegate
					{
						this.AutoOrderToTileNow(c, tile);
					});
				}
				else
				{
					this.AutoOrderToTileNow(c, tile);
				}
			}
		}

		private void AutoOrderToTileNow(Caravan c, int tile)
		{
			if (tile >= 0)
			{
				if (tile == c.Tile && !c.pather.Moving)
					return;
				int num = CaravanUtility.BestGotoDestNear(tile, c);
				if (num >= 0)
				{
					c.pather.StartPath(num, null, true);
					c.gotoMote.OrderedToTile(num);
					SoundDefOf.ColonistOrdered.PlayOneShotOnCamera(null);
				}
			}
		}

		private void SelectFirstOrNextFrom(List<WorldObject> objects, int tile)
		{
			int num = objects.FindIndex((WorldObject x) => this.selected.Contains(x));
			int num2 = -1;
			int num3 = -1;
			if (num != -1)
			{
				if (num == objects.Count - 1 || this.selected.Count >= 2)
				{
					if (this.selected.Count >= 2)
					{
						num3 = 0;
					}
					else if (tile >= 0)
					{
						num2 = tile;
					}
					else
					{
						num3 = 0;
					}
				}
				else
				{
					num3 = num + 1;
				}
			}
			else if (objects.Count == 0)
			{
				num2 = tile;
			}
			else
			{
				num3 = 0;
			}
			this.ClearSelection();
			if (num3 >= 0)
			{
				this.Select(objects[num3], true);
			}
			this.selectedTile = num2;
		}
	}
}
