using System.Windows.Input;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	internal class SelectTool : ToolCommand
	{
		private enum SelectionMode { None, Selecting, Active, Moving }
		private SelectionMode selectionMode = SelectionMode.None;

		public SelectTool(MainWindow mainWindow) : base(mainWindow) { }
		public override void OnHover(BrickView brickView)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
			{
				if (selectionMode == SelectionMode.Selecting)
					mainWindow.CurrentBrickGroupSelection.SelectRectangle(brickView);
				else if (selectionMode == SelectionMode.Moving)
					mainWindow.CurrentBrickGroupSelection.Move(brickView);
			}
			else
				OnRelease();
		}

		public override void OnLeftClick(BrickView brickView)
		{
			if (mainWindow.CurrentBrickGroupSelection != null)
				selectionMode = SelectionMode.Active;
			if (selectionMode == SelectionMode.None)
			{
				mainWindow.BeginSelectRectangle(brickView);
				selectionMode = SelectionMode.Selecting;
			}
			else if (selectionMode == SelectionMode.Active)
			{
				selectionMode = mainWindow.HandleSelection(brickView) ? SelectionMode.Moving : SelectionMode.None;
			}
		}

		public override void OnRelease()
		{
			if (selectionMode != SelectionMode.None)
			{
				if (selectionMode == SelectionMode.Selecting)
				{
					mainWindow.CurrentBrickGroupSelection.ConfirmSelection();
				}
				selectionMode = SelectionMode.Active;
			}
		}

		public override void OnRightClick(BrickView brickView)
		{
			mainWindow.ContextMenuBrickView = brickView;
		}

		public override void OnSwitch()
		{
			mainWindow.SwitchContextMenuOnBoard(false);
			mainWindow.EraseSelection();
			selectionMode = SelectionMode.None;
		}

		protected override void AssignToggleButton()
		{
			AssociatedToggleButton = mainWindow.SelectButton;
		}
	}
}
