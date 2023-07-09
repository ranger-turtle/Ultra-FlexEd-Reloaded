using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	internal abstract class ToolCommand
	{
		public MainWindow mainWindow;
		public LockableToggleButton AssociatedToggleButton { get; protected set; }

		public ToolCommand(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			AssignToggleButton();
		}

		protected abstract void AssignToggleButton();

		public abstract void OnLeftClick(BrickView brickView);
		public abstract void OnRightClick(BrickView brickView);
		public abstract void OnHover(BrickView brickView);
		public abstract void OnRelease();
		public abstract void OnSwitch();
	}
}
