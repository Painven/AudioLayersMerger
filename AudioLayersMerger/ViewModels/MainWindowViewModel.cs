using AudioLayersMerger.AudioManager;
using AudioLayersMerger.Infrastructure.Commands;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
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

        Manager manager = new Manager();

        public MainWindowViewModel()
        {
            SelectSourceFileCommand = new LambdaCommand(OpenSourceFileDialog);
            SelectLayerFilesCommand = new LambdaCommand(OpenLayerFilesDialog);
            CreateMergedFileCommand = new LambdaCommand(MergeFiles, (p) => !string.IsNullOrEmpty(SourceFilePath) && Layers.Count > 0);

            Layers = new ObservableCollection<string>();
            Layers.CollectionChanged += (o, e) => RaisePropertyChanged(nameof(LayersListVisibility));
        }

        private void MergeFiles(object obj)
        {
            string newFileName ="merged_" + Path.GetFileName(SourceFilePath);
            var sfd = new SaveFileDialog() { FileName = newFileName, InitialDirectory = Path.GetDirectoryName(SourceFilePath) };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                manager.Merge(SourceFilePath, Layers, sfd.FileName);
            }
        }

        private void OpenSourceFileDialog(object obj)
        {
            var ofd = new OpenFileDialog() { Multiselect = false };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SourceFilePath = ofd.FileName;
            }
        }

        private void OpenLayerFilesDialog(object obj)
        {
            var ofd = new OpenFileDialog() { Multiselect = true };
            if (ofd.ShowDialog() == DialogResult.OK)
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
