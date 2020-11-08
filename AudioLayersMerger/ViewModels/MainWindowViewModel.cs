using AudioLayersMerger.AudioManager;
using AudioLayersMerger.Infrastructure.Commands;
using NAudio.Wave;
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
        TimeSpan currentPosition;
        TimeSpan mainFileDuration;

        string _title = "Audio Merger";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private ObservableCollection<BackgroundFileViewModel> _layers;
        public ObservableCollection<BackgroundFileViewModel> Layers { get => _layers; set => Set(ref _layers, value); }

        private string _sourceFilePath;
        public string SourceFilePath 
        { 
            get => _sourceFilePath; 
            set 
            { 
                Set(ref _sourceFilePath, value);
                mainFileDuration = new Mp3FileReader(SourceFilePath).TotalTime;
            } 
        }

        private bool _inProgress;
        public bool InProgress { get => _inProgress; set => Set(ref _inProgress, value); }

        private double _volume = 0.5;
        public double Volume 
        { 
            get => _volume; 
            set
            {
                Set(ref _volume, value);
                foreach(var layer in Layers)
                {
                    layer.Volume = value;
                }
            }
        }

        public ICommand SelectSourceFileCommand { get; }
        public ICommand SelectLayerFilesCommand { get; }
        public ICommand CreateMergedFileCommand { get; }

        IAudioMerger manager = new SimpleAudioMerger();

        public MainWindowViewModel()
        {
            SelectSourceFileCommand = new LambdaCommand(OpenSourceFileDialog);
            SelectLayerFilesCommand = new LambdaCommand(OpenLayerFilesDialog, (p) => File.Exists(SourceFilePath));
            CreateMergedFileCommand = new LambdaCommand(MergeFiles, (p) => !string.IsNullOrEmpty(SourceFilePath) && Layers.Count > 0);

            Layers = new ObservableCollection<BackgroundFileViewModel>();
            Layers.CollectionChanged += Layers_CollectionChanged;
        }

        private void Layers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            currentPosition = TimeSpan.Zero;

            foreach (var item in Layers)
            {
                if (e.OldItems != null && e.OldItems.Contains(item)) { continue; }

                var duration = new Mp3FileReader(item.FilePath).TotalTime;

                item.IsOutOfRange = (currentPosition + duration >= mainFileDuration);

                currentPosition += duration;
            }
        }

        private async void MergeFiles(object obj)
        {
            string newFileName = "merged_" + Path.GetFileName(SourceFilePath);
            var sfd = new System.Windows.Forms.SaveFileDialog() { FileName = newFileName, InitialDirectory = Path.GetDirectoryName(SourceFilePath) };

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var sw = new Stopwatch();
                sw.Start();
                InProgress = true;

                await Task.Run(() => manager.Merge(SourceFilePath, Layers.Select(l => Tuple.Create(l.FilePath, l.Volume)).ToList(), sfd.FileName));

                InProgress = false;
                MessageBox.Show($"Выполнено за {sw.Elapsed}", "Выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                    var item = new BackgroundFileViewModel(file);
                    item.Volume = Volume;
                    item.OnRemove += Item_OnRemove;
                    Layers.Add(item);
                }
            }
        }
        
        private void Item_OnRemove(object sender, EventArgs e)
        {
            Layers.Remove(sender as BackgroundFileViewModel);
        }
    }
}
