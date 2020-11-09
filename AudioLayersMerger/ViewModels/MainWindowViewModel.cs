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
            set  => Set(ref _sourceFilePath, value);
        }

        private bool _inProgress;
        public bool InProgress { get => _inProgress; set => Set(ref _inProgress, value); }

        private double _volume = 0.5;
        public double Volume 
        { 
            get => _volume; 
            set
            {
                double diff = _volume - value;
                Set(ref _volume, value);
                
                foreach(var layer in Layers)
                {
                    layer.Volume -= diff;
                }
            }
        }

        public ICommand SelectSourceFileCommand { get; }
        public ICommand SelectLayerFilesCommand { get; }
        public ICommand CreateMergedFileCommand { get; }

        IAudioMerger manager = new SlowSmallAudioMerger();

        public MainWindowViewModel()
        {
            SelectSourceFileCommand = new LambdaCommand(OpenSourceFileDialog);
            SelectLayerFilesCommand = new LambdaCommand(OpenLayerFilesDialog, (p) => File.Exists(SourceFilePath));
            CreateMergedFileCommand = new LambdaCommand(MergeFiles, (p) => !string.IsNullOrEmpty(SourceFilePath) && Layers.Count > 0);

            Layers = new ObservableCollection<BackgroundFileViewModel>();
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

                var backgroundFilesData = Layers.Select(l => Tuple.Create(l.FilePath, l.Volume)).ToList();
                await manager.MergeAsync(SourceFilePath, backgroundFilesData, sfd.FileName);

                InProgress = false;
                MessageBox.Show($"Выполнено за {sw.Elapsed}", "Выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void OpenSourceFileDialog(object obj)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog() { Multiselect = false };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceFilePath = ofd.FileName;
                mainFileDuration = await Task.Run(() => new Mp3FileReader(SourceFilePath).TotalTime);
            }
        }

        private async void OpenLayerFilesDialog(object obj)
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
                    item.Duaration = await Task.Run(() => new Mp3FileReader(item.FilePath).TotalTime);
                }

                RefreshItems();
            }
        }
        
        private void Item_OnRemove(object sender, EventArgs e)
        {
            Layers.Remove(sender as BackgroundFileViewModel);
            RefreshItems();
        }

        private void RefreshItems()
        {
            currentPosition = TimeSpan.Zero;
            foreach (BackgroundFileViewModel item in Layers)
            {                
                item.IsOutOfRange = (currentPosition + item.Duaration >= mainFileDuration);
                currentPosition += item.Duaration;
            }
        }
    }
}
