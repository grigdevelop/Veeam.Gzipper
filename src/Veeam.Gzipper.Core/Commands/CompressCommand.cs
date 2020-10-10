using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Veeam.Gzipper.Core.Configuration.Abstraction;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.IO;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;
using Veeam.Gzipper.Core.Streams.Types;
using Veeam.Gzipper.Core.Threads;

namespace Veeam.Gzipper.Core.Commands
{
    public class CompressCommand : ICommand
    {
        private readonly IStreamFactory _streamFactory;
        private readonly ILogger _logger;
        private readonly ICompressorSettings _settings;

        public CompressCommand(IStreamFactory streamFactory, ILogger logger, ICompressorSettings settings)
        {
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void StartSync(UserInputData input)
        {
            // initial values
            var length = new FileInfo(input.SourceFilePath).Length;
            var partialSizeList = FileInfoHelpers.GetPartialSizeList(length, _settings.Cores, _settings.BufferSize);
            
            // initialize streams
            using var targetFileStream = _streamFactory.CreateTargetFileStream(input.TargetFilePath);
            using var mmf = MemoryMappedFile.CreateFromFile(input.SourceFilePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

            var compressors = new CompressorPartial[partialSizeList.Length];
            for (var i = 0; i < partialSizeList.Length; i++)
            {
                compressors[i] = new CompressorPartial(mmf, Path.GetTempFileName(), partialSizeList[i], _settings.BufferSize);
            }
            
            foreach (var compressor in compressors)
            {
                compressor.Wait();
                compressor.CopyTo(targetFileStream);
                compressor.DeleteSource();
            }
        }

    }

}
