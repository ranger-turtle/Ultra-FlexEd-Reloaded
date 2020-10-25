using System.Windows.Media;

namespace Ultra_FlexEd_Reloaded
{
	internal class BrickInEditorPrimaryData
	{
		internal int BrickId { get; set; }
		internal ImageSource Image { get; set; }
		internal bool Hidden { get; set; }

		public BrickInEditorPrimaryData(int brickId, ImageSource image, bool hidden)
		{
			BrickId = brickId;
			Image = image;
			Hidden = hidden;
		}
	}
}
