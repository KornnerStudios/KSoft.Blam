using System.Windows;
using System.Windows.Input;

namespace MgloGui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel mViewModel = new MainWindowViewModel();

		public MainWindow()
		{
			InitializeComponent();

			base.DataContext = mViewModel;
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			Properties.Settings.Default.Save();
			base.OnClosing(e);
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				mViewModel.ProcessFiles(files);
			}
		}

		private void OnPreviewDragOver(object sender, DragEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			e.Handled = true;
		}

		private void OnPreviewDragEnter(object sender, DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;
			if (mViewModel.IsProcessing)
				return;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (mViewModel.AcceptsFiles(files))
				{
					e.Effects = DragDropEffects.Move;
				}
			}
		}

		private void OnPreviewDragLeave(object sender, DragEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			mViewModel.ClearProcessFilesHelpText();
		}

		private void OnMessagesBlockMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			if (!string.IsNullOrWhiteSpace(mViewModel.MessagesText))
				Clipboard.SetText(mViewModel.MessagesText);
		}

		private void OnSelectedGameGameBuildChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (mViewModel.IsProcessing)
				return;

			mViewModel.RefreshForCurrentlySelectedGameBuild();
		}
	};
}
