﻿using System;
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
    public class LargeFastAudioMerger : IAudioMerger
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

        }

        public async Task MergeAsync(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName)
        {
            await Task.Run(() => Merge(sourceFilePath, backgroundFilesAndVolume, outputFileName));
        }

        private void Save(string outputFileName, WaveMixerStream32 mixer)
        {
            var outputStream = new Wave32To16Stream(mixer);
            WaveFileWriter.CreateWaveFile(outputFileName, outputStream);
        }
    }
}