using LevelSetData;
using LevelSetManagement;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Ultra_FlexEd_Reloaded.DialogWindows;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for SoundList.xaml
	/// </summary>
	public partial class SoundList : UserControl
	{
		private class SoundLibraryPropertyAccessor
		{
			public Action<string> Setter { get; set; }
			public Func<string> Getter { get; set; }
		}

		public SoundLibrary SoundLibrary
		{
			get { return (SoundLibrary)GetValue(SoundLibraryProperty); }
			set { SetValue(SoundLibraryProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SoundLibrary.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SoundLibraryProperty =
			DependencyProperty.Register("SoundLibrary", typeof(SoundLibrary), typeof(SoundList));

		public string Caption
		{
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register("Caption", typeof(string), typeof(SoundList), new PropertyMetadata("Sound"));

		public Window Owner
		{
			get { return (Window)GetValue(OwnerProperty); }
			set { SetValue(OwnerProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Owner.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OwnerProperty =
			DependencyProperty.Register("Owner", typeof(Window), typeof(SoundList));

		public bool IncludeLevelSetDefault
		{
			get { return (bool)GetValue(IncludeLevelSetDefaultProperty); }
			set { SetValue(IncludeLevelSetDefaultProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IncludeLevelSetDefault.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IncludeLevelSetDefaultProperty =
			DependencyProperty.Register("IncludeLevelSetDefault", typeof(bool), typeof(SoundList));


		private Dictionary<string, SoundLibraryPropertyAccessor> changeSoundFileActions;

		public SoundList()
		{
			InitializeComponent();
			changeSoundFileActions = new Dictionary<string, SoundLibraryPropertyAccessor>()
			{
				{ "Normal Ball Bounce", new SoundLibraryPropertyAccessor{ Setter = name => SoundLibrary.NormalBallBounceSoundName = name, Getter = () => SoundLibrary.NormalBallBounceSoundName } },
				{ "Bang", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.BangSoundName = name, Getter = () => SoundLibrary.BangSoundName } },
				{ "Explosion", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.ExplosionSoundName = name, Getter = () => SoundLibrary.ExplosionSoundName } },
				{ "Special Hit", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.SpecialHitSoundName = name, Getter = () => SoundLibrary.SpecialHitSoundName } },
				{ "Power Up Yield", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.PowerUpYieldSoundName = name, Getter = () => SoundLibrary.PowerUpYieldSoundName } },
				{ "Hit Wall", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.HitWallSoundName = name, Getter = () => SoundLibrary.HitWallSoundName } },
				{ "Ball Fall", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.BallFallSoundName = name, Getter = () => SoundLibrary.BallFallSoundName } },
				{ "Space Djoel Fall", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.SpaceDjoelFallSoundName = name, Getter = () => SoundLibrary.SpaceDjoelFallSoundName } },
				{ "Power Up Fall", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.PowerUpFallSoundName = name, Getter = () => SoundLibrary.PowerUpFallSoundName } },
				{ "Magnet Stick", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.MagnetStickSoundName = name, Getter = () => SoundLibrary.MagnetStickSoundName } },
				{ "Ball Size Change", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.BallSizeChangeSoundName = name, Getter = () => SoundLibrary.BallSizeChangeSoundName } },
				{ "Brick Descend", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.BrickDescendSoundName = name, Getter = () => SoundLibrary.BrickDescendSoundName } },
				{ "Lose Paddle", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.LosePaddleSoundName = name, Getter = () => SoundLibrary.LosePaddleSoundName } },
				{ "Bullet Shoot", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.BulletShootSoundName = name, Getter = () => SoundLibrary.BulletShootSoundName } },
				{ "Ball Push", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.BallPushSoundName = name, Getter = () => SoundLibrary.BallPushSoundName } },
				{ "Teleport", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.TeleportSoundName = name, Getter = () => SoundLibrary.TeleportSoundName } },
				{ "Protective Barrier Hit", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.ProtectiveBarrierHitSoundName = name, Getter = () => SoundLibrary.ProtectiveBarrierHitSoundName } },
				{ "Win", new SoundLibraryPropertyAccessor{ Setter = name=>SoundLibrary.WinSoundName = name, Getter = () => SoundLibrary.WinSoundName } }
			};
			foreach (var key in changeSoundFileActions.Keys)
				ComboBox.Items.Add(key);
		}

		private void SetHitSound()
		{
			FileChooseAndImportWindow soundChooseWindow = new FileChooseAndImportWindow("Sounds", ".wav", $"{ComboBox.SelectedItem.ToString()} sound file", includeGameDefault: true, IncludeLevelSetDefault)
			{
				Owner = Owner
			};
			if (soundChooseWindow.ShowDialog() == true)
			{
				changeSoundFileActions[ComboBox.SelectedItem.ToString()].Setter(soundChooseWindow.ChosenFileName);
				HitSound.Text = soundChooseWindow.ChosenFileName;
			}
		}

		private void ChangeButton_Click(object sender, RoutedEventArgs e)
		{
			SetHitSound();
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HitSound.Text = changeSoundFileActions[e.AddedItems[0].ToString()].Getter();
			ChangeButton.IsEnabled = true;
		}
	}
}
