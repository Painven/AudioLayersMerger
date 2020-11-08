using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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

        public void Merge(string sourceFilePath, List<string> backgroundFiles, string outputFileName)
        {
            var mixer = new WaveMixerStream32 { AutoStop = true };

            var mainChannel = new Mp3FileReader(sourceFilePath);
            mixer.AddInputStream(new WaveChannel32(mainChannel));
            TimeSpan totalDuration = mainChannel.TotalTime;


            TimeSpan currentBackgroundTime = TimeSpan.Zero;
            int backgroundIndex = 0;
            do
            {
                int index = backgroundIndex % backgroundFiles.Count;

                var layerStream = new Mp3FileReader(backgroundFiles[index]);
                var offsetStream = new WaveOffsetStream(layerStream, currentBackgroundTime, TimeSpan.Zero, totalDuration - currentBackgroundTime + layerStream.TotalTime);
                var channel = new WaveChannel32(offsetStream);
                channel.PadWithZeroes = false;
                channel.Volume = 0.4f;


                mixer.AddInputStream(channel);

                currentBackgroundTime += layerStream.TotalTime;

                backgroundIndex++;

            } while (currentBackgroundTime < totalDuration);

            WaveFileWriter.CreateWaveFile(outputFileName, new Wave32To16Stream(mixer));
        }
    }
}
