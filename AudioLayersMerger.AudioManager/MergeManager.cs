using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Lame;
using NAudio.Wave;
using NLayer.NAudioSupport;
using NAudio.Wave.SampleProviders;

namespace AudioLayersMerger.AudioManager
{
    public class MergeManager
    {
        //TODO: тут получается слишком большой выходной файл. Возможно из-за отго что создается большое количество каналов (лесенкой) а нужно всего один
        public void Merge(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName)
        {
            var mixer = new WaveMixerStream32 { AutoStop = true };
            var mainChannel = new Mp3FileReader(sourceFilePath);
            mixer.AddInputStream(new WaveChannel32(mainChannel));
            TimeSpan totalDuration = mainChannel.TotalTime;


            TimeSpan currentBackgroundTime = TimeSpan.Zero;
            int backgroundIndex = 0;
            do
            {
                int index = backgroundIndex % backgroundFilesAndVolume.Count;

                var layerStream = new Mp3FileReader(backgroundFilesAndVolume[index].Item1);
                var offsetStream = new WaveOffsetStream(layerStream, currentBackgroundTime, TimeSpan.Zero, totalDuration - currentBackgroundTime + layerStream.TotalTime);
                var channel = new WaveChannel32(offsetStream);
                channel.PadWithZeroes = false;
                channel.Volume = (float)backgroundFilesAndVolume[index].Item2;
                mixer.AddInputStream(channel);

                currentBackgroundTime += layerStream.TotalTime;

                backgroundIndex++;

            } while (currentBackgroundTime < totalDuration);

            Save(outputFileName, mixer);

            CompressMp3(outputFileName);

        }

        private void Save(string outputFileName, WaveMixerStream32 mixer)
        {
            var outputStream = new Wave32To16Stream(mixer);
            WaveFileWriter.CreateWaveFile(outputFileName, outputStream);
        }

        private static void CompressMp3(string file)
        {
            var output = Path.Combine(Path.GetDirectoryName(file), "compressed_" + Path.GetFileName(file));

            using (Mp3FileReader reader = new Mp3FileReader(file, wf => new Mp3FrameDecompressor(wf)))
            {
                WaveFileWriter.CreateWaveFile(output, reader);
            }
        }
    }
}
