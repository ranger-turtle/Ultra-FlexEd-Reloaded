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
		//TODO Add copying section to clipboard
		internal class BrickGroupSelection
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

			public void SetLastBrickCoordinates(int x, int y)
			{
				LastX = x;
				LastY = y;
			}

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

			public void SelectRectangle(BrickView brick)
			{
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

			public void ConfirmSelection()
			{
				BeginX = SwapBeginX;
				BeginY = SwapBeginY;
				SelectedBricksInEditor = new BrickInEditorPrimaryData[Height, Width];
				for (int i = 0; i < LevelSet.ROWS; i++)
				{
					for (int j = 0; j < LevelSet.COLUMNS; j++)
					{
						if (i >= BeginY && i < BeginY + Height && j >= BeginX && j < BeginX + Width && window.bricksInEditor[i, j].BrickId != 0)
						{
							BrickView brickView = window.bricksInEditor[i, j];
							SelectedBricksInEditor[i - BeginY, j - BeginX] = new BrickInEditorPrimaryData(brickView.BrickId, brickView.Image.Source, brickView.Hidden);
							window.LevelSetManager.UpdateBrickInLevel(j, i, new BrickInLevel());
						}
					}
				}
			}

			public void CopyToClipboard()
			{
				int firstLen = SelectedBricksInEditor.GetLength(0);
				int secondLen = SelectedBricksInEditor.GetLength(1);
				ClipboardData[,] dataForClipboard = new ClipboardData[firstLen, secondLen];
				for (int i = 0; i < firstLen; i++)
				{
					for (int j = 0; j < secondLen; j++)
					{
						dataForClipboard[i, j] = SelectedBricksInEditor[i, j] != null
							? new ClipboardData(SelectedBricksInEditor[i, j].BrickId, SelectedBricksInEditor[i, j].Hidden)
							: null;
					}
				}
				window.ClipboardManager.CopyBrickIndicesToClipboard(dataForClipboard);
			}

			public void CutToClipboard(BrickView brickView)
			{
				CopyToClipboard();
				window.RefreshBoard();
			}

			public void Move(BrickView brickView)
			{
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

			public void Paste(BrickView brickView)
			{
				AnchorSelection();
				ClipboardData[,] clipboardData = window.ClipboardManager.GetBrickIndicesFromClipboard();
				BeginX = Grid.GetColumn(brickView);
				BeginY = Grid.GetRow(brickView);
				Height = clipboardData.GetLength(0);
				Width = clipboardData.GetLength(1);
				SelectedBricksInEditor = new BrickInEditorPrimaryData[Height, Width];
				for (int y = Math.Max(BeginY, 0), i = 0; y < Math.Min(BeginY + Height, LevelSet.ROWS); y++, i++)
				{
					for (int x = Math.Max(BeginX, 0), j = 0; x < Math.Min(BeginX + Width, LevelSet.COLUMNS); x++, j++)
					{
						if (clipboardData[i, j] != null)
						{
							int brickId = clipboardData[i, j].BrickId;
							SelectedBricksInEditor[i, j] = new BrickInEditorPrimaryData(brickId, window.BrickListBoxItems[brickId - 1].Image.Source, clipboardData[i, j].Hidden);
							window.bricksInEditor[y, x].BrickId = SelectedBricksInEditor[i, j].BrickId;
							window.bricksInEditor[y, x].Image.Source = SelectedBricksInEditor[i, j].Image;
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

		public static readonly RoutedCommand CutCommand = new RoutedCommand("CutCommand", typeof(MenuItem));
		public static readonly RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(MenuItem));
		public static readonly RoutedCommand PasteCommand = new RoutedCommand("PasteCommand", typeof(MenuItem));

		public BrickView ContextMenuBrickView { get; set; }

		internal BrickGroupSelection CurrentBrickGroupSelection { get; private set; }

		private bool SelectionExists => CurrentBrickGroupSelection != null;

		internal void EraseSelection()
		{
			if (SelectionExists)
			{
				CurrentBrickGroupSelection.AnchorSelection();
				CurrentBrickGroupSelection = null;
			}
		}

		internal void BeginSelectRectangle(BrickView brickView)
		{
			CurrentBrickGroupSelection = new BrickGroupSelection(this, Grid.GetColumn(brickView), Grid.GetRow(brickView));
			CurrentBrickGroupSelection.SelectRectangle(brickView);
		}

		public bool HandleSelection(BrickView brickView)
		{
			if (CurrentBrickGroupSelection.IsBrickInSelection(Grid.GetColumn(brickView), Grid.GetRow(brickView)))
			{
				//Clicked selection
				CurrentBrickGroupSelection.LastX = Grid.GetColumn(brickView);
				CurrentBrickGroupSelection.LastY = Grid.GetRow(brickView);
				return true;
			}
			else
			{
				//Clicked outside selection
				EraseSelection();
				return false;
			}
		}

		internal void SwitchContextMenuOnBoard(bool active)
		{
			ContextMenu brickContextMenu = FindResource("BrickContextMenu") as ContextMenu;
			foreach (BrickView brickView in bricksInEditor)
				brickView.ContextMenu = active ? brickContextMenu : null;
		}

		private void CanCut(object sender, CanExecuteRoutedEventArgs e) => CanCopy(sender, e);

		internal void Cut_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			CurrentBrickGroupSelection.CutToClipboard(ContextMenuBrickView);
		}

		private void CanCopy(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = CurrentBrickGroupSelection?.IsBrickInSelection(Grid.GetColumn(ContextMenuBrickView), Grid.GetRow(ContextMenuBrickView)) == true;

		internal void Copy_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			CurrentBrickGroupSelection.CopyToClipboard();
		}

		private void CanPaste(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ClipboardManager.ProperDataPresent();

		internal void Paste_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			CurrentBrickGroupSelection = new BrickGroupSelection(this, Grid.GetColumn(ContextMenuBrickView), Grid.GetRow(ContextMenuBrickView));
			CurrentBrickGroupSelection.Paste(ContextMenuBrickView);
		}
	}
}
