using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioLayersMerger.AudioManager
{
    public interface IAudioMerger
    {
        Task MergeAsync(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName);
        Task ConvertM4aToMp3(string sourceFilePath);
    }
}