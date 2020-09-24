﻿using System;
using Veeam.Gzipper.Core.Abstractions;

namespace Veeam.Gzipper.Cmd.Concrete
{
    /// <summary>
    /// <see cref="Console"/> implementation of <see cref="IInputOutput"/> interface
    /// </summary>
    public class ConsoleInputOutput : IInputOutput
    {
        /// <inheritdoc cref="IInputOutput.ReadLine"/>
        public string ReadLine(string message = "")
        {
            if(!string.IsNullOrEmpty(message)) Console.Write(message);
            return Console.ReadLine();
        }

        /// <inheritdoc cref="IInputOutput.Write"/>
        public void Write(string message)
        {
            Console.Write(message);
        }
    }
}
