using NAudio.Wave;
using System;
using System.IO;

namespace AudioLayersMerger.ViewModels
{
    public class BackgroundFileViewModel : ViewModelBase
    {
        private double _volume;
        public double Volume { get => _volume; set => Set(ref _volume, value); }

        private string _filePath;
        public string FilePath { get => _filePath; set => Set(ref _filePath, value); }

        public string FileName => Path.GetFileName(FilePath);
        public string StartTime { get; set; }
        public string EndTime { get; set; }     
        public bool IsOutOfRange { get; set; }
   

        public BackgroundFileViewModel(string filePath)
        {
            FilePath = filePath;
        }

        public BackgroundFileViewModel()
        {

        }
    }
}
