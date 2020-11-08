using AudioLayersMerger.Infrastructure.Commands;
using NAudio.Wave;
using System;
using System.IO;
using System.Windows.Input;

namespace AudioLayersMerger.ViewModels
{
    public class BackgroundFileViewModel : ViewModelBase
    {
        public event EventHandler OnRemove;

        private double _volume;
        public double Volume { get => _volume; set => Set(ref _volume, value); }

        private string _filePath;
        public string FilePath { get => _filePath; set => Set(ref _filePath, value); }

        public double ItemOpacity => IsOutOfRange ? 0.4 : 1;
        public string FileName => Path.GetFileName(FilePath);

        private string _startTime;
        public string StartTime { get => _startTime; set => Set(ref _startTime, value); }

        private string _endTime;
        public string EndTime { get => _endTime; set => Set(ref _endTime, value); }

        private bool _isOutOfRange;
        public bool IsOutOfRange { get => _isOutOfRange; set => Set(ref _isOutOfRange, value); }

        public ICommand RemoveCommand { get; } 

        public BackgroundFileViewModel(string filePath) : this()
        {
            FilePath = filePath;
        }

        public BackgroundFileViewModel()
        {
            RemoveCommand = new LambdaCommand((p) => OnRemove?.Invoke(this, EventArgs.Empty));
        }
    }
}
