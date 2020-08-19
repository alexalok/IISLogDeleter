using System;

namespace IISLogDeleter.Models
{
    public class AppSettings
    {
        /// <summary>
        /// An interval between two consequent deletions.
        /// </summary>
        public TimeSpan DeletionInterval { get; private set; }

        /// <summary>
        /// Full path to the logs folder.
        /// </summary>
        public string LogsFolder { get; private set; }

        /// <summary>
        /// Only delete files that were last modified more than the given period ago.
        /// For example, when this setting is set to one day, only files that
        /// were last modified more than a day ago are deleted.
        /// </summary>
        public TimeSpan DeleteFilesOlderThan { get; private set; }
    }
}