using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Lame;
using NAudio.Wave;
using NLayer.NAudioSupport;
using NAudio.Wave.SampleProviders;
using System.Threading.Tasks;
using NAudio.MediaFoundation;

namespace AudioLayersMerger.AudioManager
{
    public class SlowSmallAudioMerger : IAudioMerger
    {
        public async Task MergeAsync(string sourceFilePath,  List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName)
        {
            var mixer = new WaveMixerStream32() { AutoStop = true };

            TimeSpan totalDuration = AddMainLayer(sourceFilePath, mixer);
            AddLayers(backgroundFilesAndVolume, mixer, totalDuration);
            await CompressAndSave(outputFileName, mixer);
        }

        private async Task CompressAndSave(string outputFileName, WaveMixerStream32 mixer)
        {
            var wave32 = new Wave32To16Stream(mixer);
            var mp3Writer = new LameMP3FileWriter(outputFileName, wave32.WaveFormat, 128);
            await wave32.CopyToAsync(mp3Writer, 8 * 1000 * 1000);

            mp3Writer.Dispose();
            wave32.Dispose();
            mixer.Dispose();
        }
        
        private TimeSpan AddMainLayer(string sourceFilePath, WaveMixerStream32 mixer)
        {
            var mainReader = new Mp3FileReader(sourceFilePath);
            var mp3Channel = WaveFormatConversionStream.CreatePcmStream(mainReader);
            var waveChannel = new WaveChannel32(mp3Channel);
            mixer.AddInputStream(waveChannel);

            return waveChannel.TotalTime;
        }

        private void AddLayers(List<Tuple<string, double>> backgroundFilesAndVolume, WaveMixerStream32 mixer, TimeSpan totalDuration)
        {
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
        }

        public async Task ConvertM4aToMp3(string input)
        {
            if (Path.GetExtension(input) != ".m4a") { throw new InvalidOperationException(); }
            string output = input.Replace(".m4a", ".mp3");

            var tempFile = Path.Combine(Path.GetDirectoryName(input), Guid.NewGuid() + ".m4a");
            await Task.Run(() => File.Copy(input, tempFile));

            try
            {
                using (var reader = new MediaFoundationReader(tempFile)) //this reader supports: MP3, AAC and WAV
                {
                    Guid AACtype = AudioSubtypes.MFAudioFormat_AAC;
                    int[] bitrates = MediaFoundationEncoder.GetEncodeBitrates(AACtype, reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);
                    await Task.Run(() => MediaFoundationEncoder.EncodeToMp3(reader, output, bitrates[bitrates.GetUpperBound(0)]));            
                }
            }
            catch
            {
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

    }
}
