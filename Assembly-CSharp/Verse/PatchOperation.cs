using System.Xml;

namespace Verse
{
	public class PatchOperation
	{
		private enum Success
		{
			Normal,
			Invert,
			Always,
			Never
		}

		public string sourceFile;

		private bool neverSucceeded = true;

		private Success success;

		public bool Apply(XmlDocument xml)
		{
			bool flag = this.ApplyWorker(xml);
			if (this.success == Success.Always)
			{
				flag = true;
			}
			else if (this.success == Success.Never)
			{
				flag = false;
			}
			else if (this.success == Success.Invert)
			{
				flag = !flag;
			}
			if (flag)
			{
				this.neverSucceeded = false;
			}
			return flag;
		}

		protected virtual bool ApplyWorker(XmlDocument xml)
		{
			Log.Error("Attempted to use PatchOperation directly; patch will always fail");
			return false;
		}

		public virtual void Complete(string modIdentifier)
		{
			if (this.neverSucceeded)
			{
				string text = string.Format("[{0}] Patch operation {1} failed", modIdentifier, this);
				if (!string.IsNullOrEmpty(this.sourceFile))
				{
					text = text + "\nfile: " + this.sourceFile;
				}
				Log.Error(text);
			}
		}
	}
}
