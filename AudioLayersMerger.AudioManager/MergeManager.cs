using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace AudioLayersMerger.AudioManager
{
    public class MergeManager
    {
        private double _backgroundVolumeLevel = 0.75;
        public double BackgroundVolumeLevel
        {
            get => _backgroundVolumeLevel;
            set
            {
                if(value >= 0 && value <= 1)
                {
                    if (value != _backgroundVolumeLevel)
                    {
                        _backgroundVolumeLevel = value;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(BackgroundVolumeLevel) + " должен быть в диапазоне от [0.00] до [1.00]");
                }
            }
        }

        public void Merge(string sourceFilePath, IEnumerable<string> layers, string outputFileName)
        {
            var mixer = new WaveMixerStream32 { AutoStop = true };

            var mainChannel = new Mp3FileReader(sourceFilePath);            
            mixer.AddInputStream(new WaveChannel32(mainChannel));
            TimeSpan mainTrackLength = mainChannel.TotalTime;


            List<Mp3FileReader> backgroundLayers = layers.Select(path => new Mp3FileReader(path)).ToList();

            TimeSpan currentBackgroundTime = TimeSpan.Zero;
            int backgroundIndex = 0;
            do
            {
                int index = backgroundIndex % backgroundLayers.Count;
                var reader = backgroundLayers[index];

                mixer.AddInputStream(new WaveChannel32(reader));

                currentBackgroundTime += reader.TotalTime;

                backgroundIndex++;

            } while (currentBackgroundTime < mainTrackLength);

            WaveFileWriter.CreateWaveFile(outputFileName, new Wave32To16Stream(mixer));
        }
    }
}
