using System.Collections.Generic;

namespace Ultra_FlexEd_Reloaded
{
	/// <summary>
	/// I hope it is my first proper Command Pattern usage...
	/// </summary>
	internal class ToolManager
	{
		public ToolCommand CurrentToolCommand { get; private set; }
		private ToolCommand previousToolCommand;
		private readonly Dictionary<string, ToolCommand> toolCommands;

		public ToolManager(MainWindow mainWindow)
		{
			toolCommands = new Dictionary<string, ToolCommand> {
				{"Single Brick", new SingleBrickTool(mainWindow) },
				{"Fill", new FillTool(mainWindow) },
				{"Line", new LineTool(mainWindow) },
				{"Rectangle", new RectangleTool(mainWindow) },
				{"Select", new SelectTool(mainWindow) },
				{"Pick Brick", new PickBrickTool(mainWindow) }
			};
		}

		public void SwitchCommand(string commandKey)
		{
			previousToolCommand = CurrentToolCommand;
			if (CurrentToolCommand != null)
			{
				CurrentToolCommand.OnSwitch();
				CurrentToolCommand.AssociatedToggleButton.LockToggle = false;
			}
			CurrentToolCommand = toolCommands[commandKey];
			CurrentToolCommand.AssociatedToggleButton.LockToggle = true;
		}

		public void SwitchToPreviousCommand() => previousToolCommand.AssociatedToggleButton.LockToggle = true;
	}
}
