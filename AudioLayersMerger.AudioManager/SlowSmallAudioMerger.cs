using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Lame;
using NAudio.Wave;
using NLayer.NAudioSupport;
using NAudio.Wave.SampleProviders;
using System.Threading.Tasks;

namespace AudioLayersMerger.AudioManager
{
    public class SlowSmallAudioMerger : IAudioMerger
    {
        //TODO: тут получается слишком большой выходной файл. Возможно из-за отго что создается большое количество каналов (лесенкой) а нужно всего один
        public async Task MergeAsync(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName)
        {
            var mixer = new WaveMixerStream32() { AutoStop = true };

            var mainReader = new Mp3FileReader(sourceFilePath);
            var mp3Channel = WaveFormatConversionStream.CreatePcmStream(mainReader);
            var waveChannel = new WaveChannel32(mp3Channel);

            mixer.AddInputStream(waveChannel);


            TimeSpan totalDuration = waveChannel.TotalTime;
            TimeSpan currentBackgroundTime = TimeSpan.Zero;

            int backgroundIndex = 0;
            do
            {
                int index = backgroundIndex % backgroundFilesAndVolume.Count;

                var backgroundReader = new Mp3FileReader(backgroundFilesAndVolume[index].Item1);
                var backgroundChannel = WaveFormatConversionStream.CreatePcmStream(backgroundReader);
                var offsetStream = new WaveOffsetStream(backgroundChannel, currentBackgroundTime, TimeSpan.Zero, totalDuration - currentBackgroundTime + backgroundReader.TotalTime);
                var channel = new WaveChannel32(offsetStream);
                channel.PadWithZeroes = false;
                channel.Volume = (float)backgroundFilesAndVolume[index].Item2;


                mixer.AddInputStream(channel);

                currentBackgroundTime += backgroundReader.TotalTime;
                backgroundIndex++;

            } while (currentBackgroundTime < totalDuration);


            var wave32 = new Wave32To16Stream(mixer);
            var mp3Writer = new LameMP3FileWriter(outputFileName, wave32.WaveFormat, 128);
            await wave32.CopyToAsync(mp3Writer, 16 * 1000 * 1000);

            mp3Writer.Dispose();
            wave32.Dispose();
            mixer.Dispose();
            mainReader.Dispose();
            mp3Channel.Dispose();
            waveChannel.Dispose();

        }

        public void Merge (string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName)
        {
            throw new NotImplementedException();
        }

    }
}
