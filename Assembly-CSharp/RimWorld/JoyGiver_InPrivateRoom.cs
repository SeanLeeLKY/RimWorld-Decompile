using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InPrivateRoom : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.ownership == null)
			{
				return null;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return null;
			}
			IntVec3 c2 = default(IntVec3);
			if (!(from c in ownedRoom.Cells
			where c.Standable(pawn.Map) && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1, -1, null, false)
			select c).TryRandomElement<IntVec3>(out c2))
			{
				return null;
			}
			return new Job(base.def.jobDef, c2);
		}
	}
}
