﻿using System;
using Veeam.Gzipper.Core.Commands;
using Veeam.Gzipper.Core.Configuration.Types;
using Veeam.Gzipper.Core.IO;
using Veeam.Gzipper.Core.Logging.Abstraction;
using Veeam.Gzipper.Core.Streams;
using Veeam.Gzipper.Core.Streams.Factory.Abstractions;

namespace Veeam.Gzipper.Core
{
    public class CompressorApplication
    {
        private readonly IInputOutput _io;
        private readonly ILogger _logger;
        private readonly IStreamFactory _streamFactory;
        private readonly GzipperUserInputReader _inputReader;

        public CompressorApplication(IInputOutput io, ILogger logger, IStreamFactory streamFactory)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _streamFactory = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));

            _inputReader = new GzipperUserInputReader(_io);
        }

        public void Execute(string[] args)
        {
            _logger.Info("Starting Gzipper application");

            var input = _inputReader.ParseUserInputData(args);

            _logger.Info($"Method: '{input.Action}' | Source file: '{input.SourceFilePath}' | Target file: '{input.TargetFilePath}' \n");

            // new c# features :) 
            ICommand processor = input.Action switch
            {
                UserAction.Compress => new CompressCommand(_streamFactory, _logger),
                UserAction.Decompress => new DecompressCommand(_streamFactory),
                _ => throw new IndexOutOfRangeException()
            };
            processor.StartSync(input);

            _logger.Info("Successfully completed");
        }
    }
}
