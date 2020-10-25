using LevelSetData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	public partial class MainWindow : Window
	{
		//FIXME remove selection capturing
		//TODO Add copying section to clipboard
		private class BrickGroupSelection
		{
			public int Width { get; set; }
			public int Height { get; set; }
			public int BeginX { get; set; }
			public int BeginY { get; set; }
			public int SwapBeginX { get; set; }
			public int SwapBeginY { get; set; }
			public int LastX { get; set; }
			public int LastY { get; set; }
			public BrickInEditorPrimaryData[,] SelectedBricksInEditor;
			private readonly MainWindow window;

			public BrickGroupSelection(MainWindow window, int BeginX, int BeginY)
			{
				this.window = window;
				this.BeginX = BeginX;
				this.BeginY = BeginY;
			}

			public bool IsBrickInSelection(int x, int y) =>
				x >= BeginX && x < BeginX + Width && y >= BeginY && y < BeginY + Height;

			public void AnchorSelection()
			{
				if (BeginX >= 0)
					for (int y = Math.Max(BeginY, 0); y < Math.Min(BeginY + Height, LevelSet.ROWS); y++)
						window.bricksInEditor[y, BeginX].SelectionBorder = false;
				if (BeginY >= 0)
					for (int x = Math.Max(BeginX + 1, 0); x < Math.Min(BeginX + Width, LevelSet.COLUMNS); x++)
						window.bricksInEditor[BeginY, x].SelectionBorder = false;
				if (BeginY + Height - 1 < LevelSet.ROWS)
					for (int x = Math.Max(BeginX + 1, 0); x < Math.Min(BeginX + Width, LevelSet.COLUMNS); x++)
						window.bricksInEditor[BeginY + Height - 1, x].SelectionBorder = false;
				if (BeginX + Width - 1 < LevelSet.COLUMNS)
					for (int y = Math.Max(BeginY + 1, 0); y < Math.Min(BeginY + Height - 1, LevelSet.ROWS); y++)
						window.bricksInEditor[y, BeginX + Width - 1].SelectionBorder = false;
				window.UpdateLevelSet();
			}

			public void SelectRectangle(object sender, RoutedEventArgs e)
			{
				BrickView brick = sender as BrickView;
				window.RefreshBoard();
				int endX = Grid.GetColumn(brick);
				int endY = Grid.GetRow(brick);
				SwapBeginX = BeginX;
				SwapBeginY = BeginY;
				Width = Math.Abs(BeginX - endX) + 1;
				Height = Math.Abs(BeginY - endY) + 1;
				if (Math.Sign(endX - BeginX) == -1)
				{
					int tmp = endX;
					endX = SwapBeginX;
					SwapBeginX = tmp;
				}
				if (Math.Sign(endY - BeginY) == -1)
				{
					int tmp = endY;
					endY = SwapBeginY;
					SwapBeginY = tmp;
				}
				for (int y = SwapBeginY; y <= endY; y++)
				{
					window.bricksInEditor[y, SwapBeginX].SelectionBorder = true;
					window.bricksInEditor[y, endX].SelectionBorder = true;
				}
				for (int x = SwapBeginX + 1; x <= endX - 1; x++)
				{
					window.bricksInEditor[SwapBeginY, x].SelectionBorder = true;
					window.bricksInEditor[endY, x].SelectionBorder = true;
				}
			}

			public void ConfirmSelection(object sender, RoutedEventArgs e)
			{
				BeginX = SwapBeginX;
				BeginY = SwapBeginY;
				window.EraseHoverEvent(null, null);
				Mouse.Capture(null);
				window.SwitchMouseEvents(window.HandleSelection, window.HandleSelection);
				SelectedBricksInEditor = new BrickInEditorPrimaryData[Height, Width];
				for (int i = 0; i < LevelSet.ROWS; i++)
				{
					for (int j = 0; j < LevelSet.COLUMNS; j++)
					{
						if (i >= BeginY && i < BeginY + Height && j >= BeginX && j < BeginX + Width && window.bricksInEditor[i, j].BrickId != 0)
						{
							BrickView brickView = window.bricksInEditor[i, j];
							SelectedBricksInEditor[i - BeginY, j - BeginX] = new BrickInEditorPrimaryData(brickView.BrickId, brickView.Image.Source, brickView.Hidden);
							window.levelSetManager.UpdateBrickInLevel(j, i, new BrickInLevel());
						}
					}
				}
			}

			public void Move(object sender, MouseEventArgs e)
			{
				BrickView brickView = sender as BrickView;
				BeginX += Grid.GetColumn(brickView) - LastX;
				BeginY += Grid.GetRow(brickView) - LastY;
				window.RefreshBoard();
				for (int y = Math.Max(BeginY, 0), i = Math.Max(-BeginY, 0); y < Math.Min(BeginY + Height, LevelSet.ROWS); y++, i++)
				{
					for (int x = Math.Max(BeginX, 0), j = Math.Max(-BeginX, 0); x < Math.Min(BeginX + Width, LevelSet.COLUMNS); x++, j++)
					{
						if (SelectedBricksInEditor[i, j] != null)
						{
							window.bricksInEditor[y, x].Image.Source = SelectedBricksInEditor[i, j].Image;
							window.bricksInEditor[y, x].BrickId = SelectedBricksInEditor[i, j].BrickId;
							window.bricksInEditor[y, x].Hidden = SelectedBricksInEditor[i, j].Hidden;
						}
						if (x == BeginX || x == BeginX + Width - 1 || y == BeginY || y == BeginY + Height - 1)
							window.bricksInEditor[y, x].SelectionBorder = true;
					}
				}
				LastX = Grid.GetColumn(brickView);
				LastY = Grid.GetRow(brickView);
			}
		}

		private BrickGroupSelection brickGroupSelection;

		private bool SelectionExists => brickGroupSelection != null;

		private void EraseSelection()
		{
			if (SelectionExists)
			{
				brickGroupSelection.AnchorSelection();
				brickGroupSelection = null;
			}
		}

		private void PrepareForNewSelection()
		{
			SwitchMouseEvents(BeginSelectRectangle, BeginSelectRectangle);
			SwitchReleaseEvent(null);
		}

		private void BeginSelectRectangle(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			brickGroupSelection = new BrickGroupSelection(this, Grid.GetColumn(brickView), Grid.GetRow(brickView));
			brickGroupSelection.SelectRectangle(sender, e);
			Mouse.Capture(Bricks, CaptureMode.SubTree);
			SwitchHoverEvent(brickGroupSelection.SelectRectangle);
			SwitchReleaseEvent(brickGroupSelection.ConfirmSelection);
		}

		public void HandleSelection(object sender, MouseButtonEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			if (brickGroupSelection.IsBrickInSelection(Grid.GetColumn(brickView), Grid.GetRow(brickView)))
			{
				//Clicked selection
				brickGroupSelection.LastX = Grid.GetColumn(sender as BrickView);
				brickGroupSelection.LastY = Grid.GetRow(sender as BrickView);
				SwitchHoverEvent(brickGroupSelection.Move);
				SwitchReleaseEvent(EraseHoverEvent);
			}
			else
			{
				//Clicked outside selection
				EraseSelection();
				PrepareForNewSelection();
			}
		}
	}
}
