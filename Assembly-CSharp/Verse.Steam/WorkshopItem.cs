using Steamworks;
using System.IO;

namespace Verse.Steam
{
	public class WorkshopItem
	{
		protected DirectoryInfo directoryInt;

		private PublishedFileId_t pfidInt;

		public DirectoryInfo Directory
		{
			get
			{
				return this.directoryInt;
			}
		}

		public virtual PublishedFileId_t PublishedFileId
		{
			get
			{
				return this.pfidInt;
			}
			set
			{
				this.pfidInt = value;
			}
		}

		public static WorkshopItem MakeFrom(PublishedFileId_t pfid)
		{
			ulong num = default(ulong);
			string path = default(string);
			uint num2 = default(uint);
			bool itemInstallInfo = SteamUGC.GetItemInstallInfo(pfid, out num, out path, 257u, out num2);
			WorkshopItem workshopItem = null;
			if (!itemInstallInfo)
			{
				workshopItem = new WorkshopItem_NotInstalled();
			}
			else
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				if (!directoryInfo.Exists)
				{
					Log.Error("Created WorkshopItem for " + pfid + " but there is no folder for it.");
				}
				FileInfo[] files = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in files)
				{
					if (fileInfo.Extension == ".rsc")
					{
						workshopItem = new WorkshopItem_Scenario();
						break;
					}
				}
				if (workshopItem == null)
				{
					workshopItem = new WorkshopItem_Mod();
				}
				workshopItem.directoryInt = directoryInfo;
			}
			workshopItem.PublishedFileId = pfid;
			return workshopItem;
		}

		public override string ToString()
		{
			return base.GetType().ToString() + "-" + this.PublishedFileId;
		}
	}
}
