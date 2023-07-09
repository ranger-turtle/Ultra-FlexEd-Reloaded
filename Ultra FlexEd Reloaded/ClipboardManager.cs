using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ultra_FlexEd_Reloaded;

namespace Ultra_FlexEd_Reloaded
{

	[Serializable]
	internal class ClipboardData
	{
		internal int BrickId { get; set; }
		internal bool Hidden { get; set; }

		public ClipboardData(int brickId, bool hidden)
		{
			BrickId = brickId;
			Hidden = hidden;
		}
	}

	internal class ClipboardManager
	{
		private const string FormatString = "BrickInEditorData";

		public void CopyBrickIndicesToClipboard(ClipboardData[,] brickIndices)
		{
			Clipboard.SetData(FormatString, brickIndices);
		}

		public ClipboardData[,] GetBrickIndicesFromClipboard()
		{
			if (Clipboard.ContainsData(FormatString))
			{
				object clipboardData = Clipboard.GetData(FormatString);
				return clipboardData as ClipboardData[,];
			}
			else
				return null;
		}

		public bool ProperDataPresent()
		{
			return Clipboard.ContainsData(FormatString);
		}
	}
}
