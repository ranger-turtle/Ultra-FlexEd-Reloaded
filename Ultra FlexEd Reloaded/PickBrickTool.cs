using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	internal class PickBrickTool : ToolCommand
	{
		public PickBrickTool(MainWindow mainWindow) : base(mainWindow) { }
		private bool executing;
		public override void OnHover(BrickView brickView)
		{
			if (executing)
				mainWindow.PickBrick(brickView.BrickId);
		}

		public override void OnLeftClick(BrickView brickView)
		{
			mainWindow.PickBrick(brickView.BrickId);
			executing = true;
		}

		public override void OnRelease()
		{
			mainWindow.SwitchToPreviousCommand();
		}

		public override void OnRightClick(BrickView brickView) { }

		public override void OnSwitch()
		{
			executing = false;
		}

		protected override void AssignToggleButton()
		{
			AssociatedToggleButton = mainWindow.PickButton;
		}
	}
}
