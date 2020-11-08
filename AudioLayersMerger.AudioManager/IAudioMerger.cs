using System;
using System.Collections.Generic;

namespace AudioLayersMerger.AudioManager
{
    public interface IAudioMerger
    {
        void Merge(string sourceFilePath, List<Tuple<string, double>> backgroundFilesAndVolume, string outputFileName);
    }
}