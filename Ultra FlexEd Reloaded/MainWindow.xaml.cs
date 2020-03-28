using LevelSetData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ultra_FlexEd_Reloaded.DialogWindows;
using Ultra_FlexEd_Reloaded.LevelManagement;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Brick[,] _bricks = new Brick[LevelSet.ROWS, LevelSet.COLUMNS];
		private LevelManager _levelManager = new LevelManager();
		private MouseEventHandler _currentBrickMouseHoverHandler;
		private MouseButtonEventHandler _currentBrickMouseLeftHandler, _currentBrickMouseRightHandler;
		private LockableToggleButton _currentPaintModeButton;
		public List<ImageListBoxItem> BrickTypes { get; set; } = new List<ImageListBoxItem>();

		private bool _ready;

		public MainWindow()
		{
			InitializeComponent();
			//ListItem.Image.Source = new BitmapImage(new Uri("/Default Assets/bricks/brick1.png", UriKind.Relative));
			//ListItem.Label.Content = "Blue Marble p";
			//ListItem.IsSelected = true;
			//BitmapImage img = new BitmapImage(new Uri("/Default Assets/Bricks/brick3.png", UriKind.Relative));
			//img.SourceRect = new Int32Rect(0, 0, 30, 15);
			for (int i = 0; i < LevelSet.ROWS; i++)
			{
				for (int j = 0; j < LevelSet.COLUMNS; j++)
				{
					Brick brick = new Brick();
					_bricks[i, j] = brick;
					brick.Width = 30;
					brick.Height = 15;
					//brick.Image.Source = img;
					Bricks.Children.Add(brick);
					Grid.SetRow(brick, i);
					Grid.SetColumn(brick, j);
					brick.MouseLeftButtonDown += _currentBrickMouseLeftHandler = PutSingleBrick;
					brick.MouseRightButtonDown += _currentBrickMouseRightHandler = RemoveSingleBrick;
				}
			}
			string[] names = {"Red FlexBall Cream", "Green FlexBall Cream", "Blue FlexBall Cream", "Yellow Bumps",
				"Orange-Beige Bumps", "Cyan Bumps", "Turquoise FlexBall Cream", "Orange-Beige Stripes", "Lila Stripes",
				"Sky Blue Stripes", "Magenta Scales", "Green Scales"};
			for (int i = 0; i < names.Length; i++)
			{
				ImageListBoxItem imageListBoxItem = new ImageListBoxItem();
				imageListBoxItem.Image.Source = new BitmapImage(new Uri($"/Default Assets/Bricks/brick{i + 1}.png", UriKind.Relative));
				imageListBoxItem.Label.Content = names[i];
				BrickTypes.Add(imageListBoxItem);
			}
			BrickTypes[0].IsSelected = true;
			SingleBrickButton.IsChecked = true;
			_currentPaintModeButton = SingleBrickButton;
			_currentPaintModeButton.LockToggle = true;
			SwitchHoverEraseOn();
			_ready = true;
		}

		private void SwitchHoverEraseOn()
		{
			Bricks.MouseLeftButtonUp += EraseHoverEvent;
			Bricks.MouseRightButtonUp += EraseHoverEvent;
		}

		private void SwitchHoverEraseOff()
		{
			if (SingleBrickButton.IsChecked == true)
			{
				Bricks.MouseLeftButtonUp -= EraseHoverEvent;
				Bricks.MouseRightButtonUp -= EraseHoverEvent;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MinWidth = Width;
			MinHeight = Height;
		}

		private void Button_Call_Window(object sender, RoutedEventArgs e)// TODO Delete it soon
		{
			BrickWindow brickWindow = new BrickWindow
			{
				Owner = this
			};
			brickWindow.ShowDialog();
		}

		private void PutBrick(Brick brick, BitmapImage image)
		{
			int columnIndex = Grid.GetColumn(brick);
			int rowIndex = Grid.GetRow(brick);
			_bricks[rowIndex, columnIndex].Image.Source = image;
		}

		private void PutBrickAndUpdateLevel(Brick brick, BitmapImage image, uint brickTypeId, bool hidden)
		{
			PutBrick(brick, image);
			int columnIndex = Grid.GetColumn(brick);
			int rowIndex = Grid.GetRow(brick);
			BrickInLevel brickInLevel = _levelManager.LevelSet.Levels[_levelManager.CurrentLevelIndex].Bricks[rowIndex, columnIndex];
			brickInLevel.BrickId = brickTypeId;
			brickInLevel.Hidden = _bricks[rowIndex, columnIndex].Hidden = brickTypeId > 0 ? hidden : false;
		}

		//private void HandleLeftClick_Down(object sender, MouseButtonEventArgs e)
		//{
		//	_brickLeftPressed = true;
		//	if (_currentBrickMouseHoverHandler == RemoveSingleBrick)
		//		AssignEvents(PutSingleBrick);
		//}

		//private void HandleLeftClick_Up(object sender, MouseButtonEventArgs e)
		//{
		//	_brickLeftPressed = false;
		//	if (_currentBrickMouseHoverHandler == RemoveSingleBrick)
		//		AssignEvents(PutSingleBrick);
		//}

		//private void HandleRightClick_Down(object sender, MouseButtonEventArgs e)
		//{
		//	_brickRightPressed = true;
		//}

		//private void HandleRightClick_Up(object sender, MouseButtonEventArgs e)
		//{
		//	_brickRightPressed = false;
		//}

		private void EraseHoverEvent(object sender, MouseButtonEventArgs e)
		{
			foreach (Brick brick in _bricks)
			{
				brick.MouseEnter -= _currentBrickMouseHoverHandler;
			}
			_currentBrickMouseHoverHandler = null;
		}

		private void PutSingleBrick(object sender, RoutedEventArgs e)
		{
			PutBrickAndUpdateLevel(sender as Brick, new BitmapImage(new Uri("/Default Assets/Bricks/brick1.png", UriKind.Relative)), _levelManager.CurrentBrickId, false);
			if (_currentBrickMouseHoverHandler == null)
				SwitchEvents(PutSingleBrick);
		}

		private void RemoveSingleBrick(object sender, RoutedEventArgs e)
		{
			PutBrickAndUpdateLevel(sender as Brick, null, 0, false);
			if (_currentBrickMouseHoverHandler == null)
				SwitchEvents(RemoveSingleBrick);
		}

		private void FillWithBricks(Brick brick, BitmapImage image, bool erase)
		{
			uint brickIdForFill = erase ? 0 : _levelManager.CurrentBrickId;
			int columnIndex = Grid.GetColumn(brick);
			int rowIndex = Grid.GetRow(brick);
			BrickInLevel[,] bricksInLevel = _levelManager.LevelSet.Levels[_levelManager.CurrentLevelIndex].Bricks;
			uint clickedBrickId = bricksInLevel[rowIndex, columnIndex].BrickId;
			PutBrickAndUpdateLevel(brick, image, brickIdForFill, _levelManager.Hidden);
			if (rowIndex > 0 && bricksInLevel[rowIndex - 1, columnIndex].BrickId == clickedBrickId)
				FillWithBricks(_bricks[rowIndex - 1, columnIndex], image, erase);
			if (columnIndex > 0 && bricksInLevel[rowIndex, columnIndex - 1].BrickId == clickedBrickId)
				FillWithBricks(_bricks[rowIndex, columnIndex - 1], image, erase);
			if (rowIndex < LevelSet.ROWS - 1 && bricksInLevel[rowIndex + 1, columnIndex].BrickId == clickedBrickId)
				FillWithBricks(_bricks[rowIndex + 1, columnIndex], image, erase);
			if (columnIndex < LevelSet.COLUMNS - 1 && bricksInLevel[rowIndex, columnIndex + 1].BrickId == clickedBrickId)
				FillWithBricks(_bricks[rowIndex, columnIndex + 1], image, erase);
		}

		private void BrushFill(object sender, RoutedEventArgs e)
		{
			FillWithBricks(sender as Brick, new BitmapImage(new Uri("/Default Assets/Bricks/brick1.png", UriKind.Relative)), false);
		}

		private void EraseFill(object sender, RoutedEventArgs e)
		{
			FillWithBricks(sender as Brick, null, true);
		}

		private void SwitchEvents(MouseEventHandler brickHoverHandler, MouseButtonEventHandler brickMouseLeftHandler, MouseButtonEventHandler brickMouseRightHandler)
		{
			MouseEventHandler oldMouseHoverHandler = _currentBrickMouseHoverHandler;
			_currentBrickMouseHoverHandler = brickHoverHandler;
			MouseButtonEventHandler oldMouseLeftHandler = _currentBrickMouseLeftHandler;
			_currentBrickMouseLeftHandler = brickMouseLeftHandler;
			MouseButtonEventHandler oldMouseRightHandler = _currentBrickMouseRightHandler;
			_currentBrickMouseRightHandler = brickMouseRightHandler;
			foreach (Brick brick in _bricks)
			{
				if (_currentBrickMouseHoverHandler != null)
					brick.MouseEnter -= _currentBrickMouseHoverHandler;
				if (brickHoverHandler != null)
					brick.MouseEnter += _currentBrickMouseHoverHandler;
				brick.MouseLeftButtonDown -= oldMouseLeftHandler;
				brick.MouseLeftButtonDown += _currentBrickMouseLeftHandler;
				brick.MouseRightButtonDown -= oldMouseRightHandler;
				brick.MouseRightButtonDown += _currentBrickMouseRightHandler;
			}
		}

		private void SwitchEvents(MouseEventHandler brickHoverHandler)
		{
			foreach (Brick brick in _bricks)
			{
				if (_currentBrickMouseHoverHandler != null)
					brick.MouseEnter -= _currentBrickMouseHoverHandler;
				brick.MouseEnter += _currentBrickMouseHoverHandler = brickHoverHandler;
			}
		}

		private void SwitchPaintModeButton(LockableToggleButton paintModeButton)
		{
			_currentPaintModeButton.LockToggle = false;
			_currentPaintModeButton = paintModeButton;
		}

		private void SingleBrickButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_ready)
			{
				SwitchPaintModeButton(SingleBrickButton);
				FillButton.IsChecked = false;
				LineButton.IsChecked = false;
				RectangleButton.IsChecked = false;
				SwitchEvents(null, PutSingleBrick, RemoveSingleBrick);
				SwitchHoverEraseOn();
			}
		}

		private void FillButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchHoverEraseOff();
			SwitchPaintModeButton(FillButton);
			SingleBrickButton.IsChecked = false;
			LineButton.IsChecked = false;
			RectangleButton.IsChecked = false;
			SwitchEvents(null, BrushFill, EraseFill);
		}

		private void LineButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchHoverEraseOff();
			SwitchPaintModeButton(LineButton);
			FillButton.IsChecked = false;
			SingleBrickButton.IsChecked = false;
			RectangleButton.IsChecked = false;
			//AssignEvents(PutSingleBrick);
		}

		private void RectangleButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchHoverEraseOff();
			SwitchPaintModeButton(RectangleButton);
			FillButton.IsChecked = false;
			LineButton.IsChecked = false;
			SingleBrickButton.IsChecked = false;
			//AssignEvents(PutSingleBrick);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			for (int i = 0; i < LevelSet.ROWS; ++i)
			{
				for (int j = 0; j < LevelSet.COLUMNS; ++j)
				{
					_bricks[i, j].Width = Bricks.ColumnDefinitions[j].ActualWidth;
					_bricks[i, j].Height = Bricks.RowDefinitions[i].ActualHeight;
				}
			}
		}
	}
}
