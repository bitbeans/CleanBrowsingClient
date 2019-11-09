using System;

namespace CleanBrowsingClient.Models
{
    /// <summary>
	///     Outer remote update class.
	/// </summary>
	public class RemoteUpdate
    {
        /// <summary>
        ///     Indicates if the requested update can be done.
        /// </summary>
        public bool CanUpdate { get; set; }

        /// <summary>
        ///     The update data.
        /// </summary>
        public Update Update { get; set; }
    }

    /// <summary>
	///     The update data.
	/// </summary>
	public class Update
    {
        /// <summary>
        ///     The available version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        ///     The date of this release.
        /// </summary>
        public DateTime Release { get; set; }

        /// <summary>
        ///     The installer object.
        /// </summary>
        public Installer Installer { get; set; }
    }

    /// <summary>
	///     The installer information.
	/// </summary>
	public class Installer
    {
        /// <summary>
        ///     Name of the installer file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Uri to download the installer file.
        /// </summary>
        public Uri Uri { get; set; }
    }
}
