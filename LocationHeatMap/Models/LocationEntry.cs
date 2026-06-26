// =============================================================================
// LocationEntry.cs
// Model representing a single recorded GPS location entry stored in SQLite.
// Author: Student — CS Mobile Development
// Follows: Microsoft C# Coding Conventions
// =============================================================================

using SQLite;

namespace LocationHeatMap.Models
{
    /// <summary>
    /// Represents one saved GPS coordinate captured from the device.
    /// Each row in the SQLite "LocationEntries" table maps to one instance.
    /// </summary>
    [Table("LocationEntries")]
    public class LocationEntry
    {
        // ── Primary key ──────────────────────────────────────────────────────
        /// <summary>Auto-incremented unique identifier for each location record.</summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ── Spatial data ─────────────────────────────────────────────────────
        /// <summary>WGS-84 latitude in decimal degrees (−90 to +90).</summary>
        [NotNull]
        public double Latitude { get; set; }

        /// <summary>WGS-84 longitude in decimal degrees (−180 to +180).</summary>
        [NotNull]
        public double Longitude { get; set; }

        /// <summary>
        /// Horizontal accuracy radius in metres reported by the OS.
        /// Lower is better; values above ~50 m are considered low quality.
        /// </summary>
        public double AccuracyMeters { get; set; }

        // ── Temporal data ────────────────────────────────────────────────────
        /// <summary>UTC timestamp when the location was captured.</summary>
        [NotNull]
        public DateTime RecordedAtUtc { get; set; }

        // ── Convenience ──────────────────────────────────────────────────────
        /// <summary>
        /// Returns a human-readable string of this entry, useful for debugging.
        /// </summary>
        public override string ToString() =>
            $"[{Id}] ({Latitude:F6}, {Longitude:F6}) ±{AccuracyMeters:F1}m @ {RecordedAtUtc:u}";
    }
}
