using System.Windows.Input;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	internal class LineTool : ToolCommand
	{
		public LineTool(MainWindow mainWindow) : base(mainWindow) { }
		private bool executing;
		private bool erase;
		public override void OnHover(BrickView brickView)
		{
			if (executing)
			{
				if (Mouse.RightButton == MouseButtonState.Pressed || Mouse.LeftButton == MouseButtonState.Pressed)
					mainWindow.DrawLine(brickView, erase);
				else
					OnRelease();
			}
		}

		public override void OnLeftClick(BrickView brickView)
		{
			executing = true;
			erase = false;
			mainWindow.BeginBrickLine(brickView);
		}

		public override void OnRelease()
		{
			mainWindow.UpdateLevelSet();
			executing = false;
		}

		public override void OnRightClick(BrickView brickView)
		{
			executing = true;
			erase = true;
			mainWindow.BeginEraseLine(brickView);
		}

		public override void OnSwitch() { }

		protected override void AssignToggleButton()
		{
			AssociatedToggleButton = mainWindow.LineButton;
		}
	}
}
