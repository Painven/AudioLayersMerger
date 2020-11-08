using AudioLayersMerger.AudioManager;
using AudioLayersMerger.Infrastructure.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AudioLayersMerger.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        string _title = "Заголовок окна из ViewModel";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private ObservableCollection<string> _layers;
        public ObservableCollection<string> Layers { get => _layers; set => Set(ref _layers, value); }

        private string _sourceFilePath;
        public string SourceFilePath { get => _sourceFilePath; set { Set(ref _sourceFilePath, value); RaisePropertyChanged(nameof(SourceFilePathVisibility)); } }

        public Visibility SourceFilePathVisibility => File.Exists(SourceFilePath) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LayersListVisibility => Layers.Any() ? Visibility.Visible : Visibility.Collapsed;

        public ICommand SelectSourceFileCommand { get; }
        public ICommand SelectLayerFilesCommand { get; }
        public ICommand CreateMergedFileCommand { get; }

        private bool _inProgress;
        public bool InProgress { get => _inProgress; set => Set(ref _inProgress, value); }

        MergeManager manager = new MergeManager();

        public MainWindowViewModel()
        {
            SelectSourceFileCommand = new LambdaCommand(OpenSourceFileDialog);
            SelectLayerFilesCommand = new LambdaCommand(OpenLayerFilesDialog);
            //CreateMergedFileCommand = new LambdaCommand(MergeFiles, (p) => !string.IsNullOrEmpty(SourceFilePath) && Layers.Count > 0);
            CreateMergedFileCommand = new LambdaCommand(MergeFiles);

            Layers = new ObservableCollection<string>();
            Layers.CollectionChanged += (o, e) => RaisePropertyChanged(nameof(LayersListVisibility));
        }

        private async void MergeFiles(object obj)
        {
            var sw = new Stopwatch();
            sw.Start();
            InProgress = true;
            string testOutput = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{DateTime.Now.ToString("h-mm")}_merged.wav");
            await Task.Run(() => manager.Merge(SourceFilePath, Layers, 0.5, testOutput));
            InProgress = false;
            MessageBox.Show($"Выполнено за {sw.Elapsed}", "Выполнено", MessageBoxButton.OK, MessageBoxImage.Information);

            //string newFileName = "merged_" + Path.GetFileName(SourceFilePath);
            //var sfd = new SaveFileDialog() { FileName = newFileName, InitialDirectory = Path.GetDirectoryName(SourceFilePath) };
            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    manager.Merge(SourceFilePath, Layers, 0, null);
            //}
        }

        private void OpenSourceFileDialog(object obj)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog() { Multiselect = false };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceFilePath = ofd.FileName;
            }
        }

        private void OpenLayerFilesDialog(object obj)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog() { Multiselect = true };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var file in ofd.FileNames)
                {
                    if (!Layers.Contains(file))
                    {
                        Layers.Add(file);
                    }
                }
            }
        }
    }
}
