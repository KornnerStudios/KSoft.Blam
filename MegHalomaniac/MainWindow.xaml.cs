using System;
using System.Windows;
using System.Windows.Input;

namespace MgloGui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool mInitialGameBuildRefreshCompleted;
		private MainWindowViewModel ViewModel
			=> (MainWindowViewModel)(DataContext
				?? throw new InvalidOperationException("MainWindow requires a MainWindowViewModel DataContext."));

		public MainWindow()
		{
			InitializeComponent();
		}

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			if (mInitialGameBuildRefreshCompleted)
				return;

			mInitialGameBuildRefreshCompleted = true;
			ViewModel.RefreshForCurrentlySelectedGameBuild();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			Properties.Settings.Default.Save();
			base.OnClosing(e);
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (ViewModel.IsProcessing)
			{
				return;
			}

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				ViewModel.ProcessFiles(files);
			}
		}

		private void OnPreviewDragOver(object sender, DragEventArgs e)
		{
			if (ViewModel.IsProcessing)
			{
				return;
			}

			e.Handled = true;
		}

		private void OnPreviewDragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;
			if (ViewModel.IsProcessing)
			{
				return;
			}

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (ViewModel.AcceptsFiles(files))
				{
					e.Effects = DragDropEffects.Move;
				}
			}
		}

		private void OnPreviewDragLeave(object sender, DragEventArgs e)
		{
			if (ViewModel.IsProcessing)
			{
				return;
			}

			ViewModel.ClearProcessFilesHelpText();
		}

		private void OnMessagesBlockMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ViewModel.IsProcessing)
			{
				return;
			}

			if (!string.IsNullOrWhiteSpace(ViewModel.MessagesText))
			{
				Clipboard.SetText(ViewModel.MessagesText);
			}
		}

		private void OnSelectedGameGameBuildChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// Settings-backed bindings can raise this while InitializeComponent is constructing the window.
			if (!IsLoaded || ViewModel.IsProcessing)
			{
				return;
			}

			ViewModel.RefreshForCurrentlySelectedGameBuild();
		}
	};
}
