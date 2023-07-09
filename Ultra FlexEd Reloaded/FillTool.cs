using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	internal class FillTool : ToolCommand
	{
		public FillTool(MainWindow mainWindow) : base(mainWindow) { }
		public override void OnHover(BrickView brickView) { }

		public override void OnLeftClick(BrickView brickView)
		{
			mainWindow.BrushFill(brickView);
		}

		public override void OnRelease() { }

		public override void OnRightClick(BrickView brickView)
		{
			mainWindow.EraseFill(brickView);
		}

		public override void OnSwitch() { }

		protected override void AssignToggleButton()
		{
			AssociatedToggleButton = mainWindow.FillButton;
		}
	}
}
