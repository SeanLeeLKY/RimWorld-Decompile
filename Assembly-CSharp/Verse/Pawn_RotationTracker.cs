using UnityEngine;

namespace Verse
{
	public class Pawn_RotationTracker : IExposable
	{
		private Pawn pawn;

		public Pawn_RotationTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void Notify_Spawned()
		{
			this.UpdateRotation();
		}

		public void UpdateRotation()
		{
			if (!this.pawn.Destroyed && !this.pawn.jobs.HandlingFacing)
			{
				if (this.pawn.pather.Moving)
				{
					if (this.pawn.pather.curPath != null && this.pawn.pather.curPath.NodesLeftCount >= 1)
					{
						this.FaceAdjacentCell(this.pawn.pather.nextCell);
					}
				}
				else
				{
					Stance_Busy stance_Busy = this.pawn.stances.curStance as Stance_Busy;
					if (stance_Busy != null && stance_Busy.focusTarg.IsValid)
					{
						if (stance_Busy.focusTarg.HasThing)
						{
							this.Face(stance_Busy.focusTarg.Thing.DrawPos);
						}
						else
						{
							this.FaceCell(stance_Busy.focusTarg.Cell);
						}
					}
					else
					{
						if (this.pawn.jobs.curJob != null)
						{
							LocalTargetInfo target = this.pawn.CurJob.GetTarget(this.pawn.jobs.curDriver.rotateToFace);
							if (target.HasThing)
							{
								bool flag = false;
								IntVec3 c = default(IntVec3);
								CellRect cellRect = target.Thing.OccupiedRect();
								for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
								{
									for (int j = cellRect.minX; j <= cellRect.maxX; j++)
									{
										if (this.pawn.Position == new IntVec3(j, 0, i))
										{
											this.Face(target.Thing.DrawPos);
											return;
										}
									}
								}
								for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
								{
									for (int l = cellRect.minX; l <= cellRect.maxX; l++)
									{
										IntVec3 intVec = new IntVec3(l, 0, k);
										if (intVec.AdjacentToCardinal(this.pawn.Position))
										{
											this.FaceAdjacentCell(intVec);
											return;
										}
										if (intVec.AdjacentTo8Way(this.pawn.Position))
										{
											flag = true;
											c = intVec;
										}
									}
								}
								if (flag)
								{
									if (DebugViewSettings.drawPawnRotatorTarget)
									{
										this.pawn.Map.debugDrawer.FlashCell(this.pawn.Position, 0.6f, "jbthing", 50);
										GenDraw.DrawLineBetween(this.pawn.Position.ToVector3Shifted(), c.ToVector3Shifted());
									}
									this.FaceAdjacentCell(c);
								}
								else
								{
									this.Face(target.Thing.DrawPos);
								}
								return;
							}
							if (this.pawn.Position.AdjacentTo8Way(target.Cell))
							{
								if (DebugViewSettings.drawPawnRotatorTarget)
								{
									this.pawn.Map.debugDrawer.FlashCell(this.pawn.Position, 0.2f, "jbloc", 50);
									GenDraw.DrawLineBetween(this.pawn.Position.ToVector3Shifted(), target.Cell.ToVector3Shifted());
								}
								this.FaceAdjacentCell(target.Cell);
								return;
							}
							if (target.Cell.IsValid && target.Cell != this.pawn.Position)
							{
								this.Face(target.Cell.ToVector3());
								return;
							}
						}
						if (this.pawn.Drafted)
						{
							this.pawn.Rotation = Rot4.South;
						}
					}
				}
			}
		}

		public void RotationTrackerTick()
		{
			this.UpdateRotation();
		}

		private void FaceAdjacentCell(IntVec3 c)
		{
			if (!(c == this.pawn.Position))
			{
				IntVec3 intVec = c - this.pawn.Position;
				if (intVec.x > 0)
				{
					this.pawn.Rotation = Rot4.East;
				}
				else if (intVec.x < 0)
				{
					this.pawn.Rotation = Rot4.West;
				}
				else if (intVec.z > 0)
				{
					this.pawn.Rotation = Rot4.North;
				}
				else
				{
					this.pawn.Rotation = Rot4.South;
				}
			}
		}

		public void FaceCell(IntVec3 c)
		{
			if (!(c == this.pawn.Position))
			{
				float angle = (c - this.pawn.Position).ToVector3().AngleFlat();
				this.pawn.Rotation = Pawn_RotationTracker.RotFromAngleBiased(angle);
			}
		}

		public void Face(Vector3 p)
		{
			if (!(p == this.pawn.DrawPos))
			{
				float angle = (p - this.pawn.DrawPos).AngleFlat();
				this.pawn.Rotation = Pawn_RotationTracker.RotFromAngleBiased(angle);
			}
		}

		public static Rot4 RotFromAngleBiased(float angle)
		{
			if (angle < 30.0)
			{
				return Rot4.North;
			}
			if (angle < 150.0)
			{
				return Rot4.East;
			}
			if (angle < 210.0)
			{
				return Rot4.South;
			}
			if (angle < 330.0)
			{
				return Rot4.West;
			}
			return Rot4.North;
		}

		public void ExposeData()
		{
		}
	}
}
