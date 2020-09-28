using System.IO;

namespace Veeam.Gzipper.Core.Configuration.Types
{
    /// <summary>
    /// Represents user input data
    /// </summary>
    public class UserInputData
    {
        /// <summary>
        /// Gets user action
        /// </summary>
        public UserAction Action { get; }

        /// <summary>
        /// Gets source file path
        /// </summary>
        public string SourceFilePath { get; }

        /// <summary>
        /// Gets target file path
        /// </summary>
        public string TargetFilePath { get; }

        /// <summary>
        /// Creates a new instance of <see cref="UserInputData"/>
        /// </summary>
        /// <param name="action">User action</param>
        /// <param name="sourceFilePath">Source path</param>
        /// <param name="targetFilePath">Target path</param>
        public UserInputData(UserAction action, string sourceFilePath, string targetFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
                throw new InvalidDataException(nameof(sourceFilePath));
            if (string.IsNullOrEmpty(targetFilePath))
                throw new InvalidDataException(nameof(targetFilePath));

            Action = action;
            SourceFilePath = sourceFilePath;
            TargetFilePath = targetFilePath;
        }
    }
}
