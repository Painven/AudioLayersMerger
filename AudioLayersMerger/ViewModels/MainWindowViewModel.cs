using AudioLayersMerger.AudioManager;
using AudioLayersMerger.Infrastructure.Commands;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
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

        private double _progressPercent;
        public double ProgressPercent { get => _progressPercent; set => Set(ref _progressPercent, value); }

        private bool _isWorking;
        public bool IsWorking { get => _isWorking; set => Set(ref _isWorking, value); }

        private double _volume = 0.25;
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
        public ICommand SelectLayerRandomFilesCommand { get; }
        public ICommand CreateMergedFileCommand { get; }
        public ICommand ConvertFileFormat { get; }

        IAudioMerger manager = new SlowSmallAudioMerger();

        public MainWindowViewModel()
        {
            SelectSourceFileCommand = new LambdaCommand(OpenSourceFileDialog);
            SelectLayerFilesCommand = new LambdaCommand(OpenLayerFilesDialog, (p) => File.Exists(SourceFilePath));
            SelectLayerRandomFilesCommand = new LambdaCommand(AddRandomFiles, (p) => File.Exists(SourceFilePath));
            CreateMergedFileCommand = new LambdaCommand(MergeFiles, (p) => !string.IsNullOrEmpty(SourceFilePath) && Layers.Count > 0);
            ConvertFileFormat = new LambdaCommand(ConvertFormat);

            Layers = new ObservableCollection<BackgroundFileViewModel>();
        }

        private async void ConvertFormat(object obj)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog() { Multiselect = false, Filter = "M4A файлы |*.m4a" };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsWorking = true;
                await manager.ConvertM4aToMp3(ofd.FileName);
                IsWorking = false;
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


                var backgroundFilesData = Layers.Select(l => Tuple.Create(l.FilePath, l.Volume)).ToList();

                IsWorking = true;
                try
                {
                    await manager.MergeAsync(SourceFilePath, backgroundFilesData, sfd.FileName);
                    MessageBox.Show($"Выполнено за {sw.Elapsed}", "Выполнено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsWorking = false;
                }
                


                
            }
        }

        private async void OpenSourceFileDialog(object obj)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog() { Multiselect = false, Filter = "MP3 файлы|*.mp3" };
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
                    await AddItemFromFileName(file);
                }

                RefreshItems();
            }
        }

        private async Task AddItemFromFileName(string file)
        {
            var item = new BackgroundFileViewModel(file);
            item.Volume = Volume;
            item.OnRemove += Item_OnRemove;
            Layers.Add(item);
            item.Duaration = await Task.Run(() => new Mp3FileReader(item.FilePath).TotalTime);
        }

        private async void AddRandomFiles(object obj)
        {
            var ofd = new System.Windows.Forms.FolderBrowserDialog()
            {
                SelectedPath = ConfigurationManager.AppSettings["default_random_folder_path"]
            };

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) { return; }        
            var dirFiles = Directory.GetFiles(ofd.SelectedPath, "*.mp3", SearchOption.AllDirectories);
            if (dirFiles.Length == 0) { return; }
            
            do
            {
                var nextRandomFile = dirFiles[new Random().Next(0, dirFiles.Length)];

                await AddItemFromFileName(nextRandomFile);
                await Task.Delay(TimeSpan.FromMilliseconds(50));

            } while ((Layers.Sum(l => l.Duaration.TotalSeconds)) < mainFileDuration.TotalSeconds);

            Layers.Remove(Layers.Last());
            RefreshItems();
            
            
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
