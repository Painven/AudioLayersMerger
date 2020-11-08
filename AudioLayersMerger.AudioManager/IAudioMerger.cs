using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioLayersMerger.AudioManager
{
    public interface IAudioMerger
    {
        Task MergeAsync(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName);
        void Merge(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName);
    }
}