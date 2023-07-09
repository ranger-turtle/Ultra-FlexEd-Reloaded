using System.Windows.Input;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	internal class SingleBrickTool : ToolCommand
	{
		public SingleBrickTool(MainWindow mainWindow) : base(mainWindow) { }
		private bool executing;
		private bool erase;

		public override void OnHover(BrickView brickView)
		{
			if (executing)
			{
				if (Mouse.RightButton == MouseButtonState.Pressed || Mouse.LeftButton == MouseButtonState.Pressed)
					mainWindow.PutSingleBrick(brickView, erase);
				else
					OnRelease();
			}
		}

		public override void OnLeftClick(BrickView brickView)
		{
			erase = false;
			executing = true;
			mainWindow.PutSingleBrick(brickView, erase);
		}

		public override void OnRelease()
		{
			executing = false;
		}

		public override void OnRightClick(BrickView brickView)
		{
			erase = true;
			executing = true;
			mainWindow.PutSingleBrick(brickView, erase);
		}

		public override void OnSwitch() { }

		protected override void AssignToggleButton()
		{
			AssociatedToggleButton = mainWindow.SingleBrickButton;
		}
	}
}
