// =============================================================================
// DatabaseService.cs
// Handles all SQLite read/write operations for location data.
// Uses the sqlite-net-pcl library which ships with MAUI.
// Author: Student — CS Mobile Development
// =============================================================================

using SQLite;
using LocationHeatMap.Models;

namespace LocationHeatMap.Services
{
    /// <summary>
    /// Singleton-style service that manages the local SQLite database.
    /// Call <see cref="InitAsync"/> once at app startup before any other method.
    /// </summary>
    public class DatabaseService
    {
        // ── Fields ────────────────────────────────────────────────────────────
        private SQLiteAsyncConnection? _connection;

        /// <summary>Absolute path to the SQLite file on the device.</summary>
        private static string DbPath =>
            Path.Combine(FileSystem.AppDataDirectory, "locations.db3");

        // ── Initialisation ────────────────────────────────────────────────────

        /// <summary>
        /// Opens (or creates) the database and ensures the schema is up to date.
        /// Must be awaited before calling <see cref="SaveLocationAsync"/> or
        /// <see cref="GetAllLocationsAsync"/>.
        /// </summary>
        public async Task InitAsync()
        {
            if (_connection is not null)
                return; // Already initialised — nothing to do

            _connection = new SQLiteAsyncConnection(DbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            // CreateTableAsync is idempotent: safe to call on every launch.
            await _connection.CreateTableAsync<LocationEntry>();
        }

        // ── Write ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Persists a single <see cref="LocationEntry"/> to the database.
        /// </summary>
        /// <param name="entry">The entry to insert.</param>
        /// <returns>Number of rows inserted (should always be 1).</returns>
        public async Task<int> SaveLocationAsync(LocationEntry entry)
        {
            EnsureInitialised();
            return await _connection!.InsertAsync(entry);
        }

        // ── Read ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves every stored location, ordered oldest-first.
        /// </summary>
        public async Task<List<LocationEntry>> GetAllLocationsAsync()
        {
            EnsureInitialised();
            return await _connection!
                .Table<LocationEntry>()
                .OrderBy(e => e.RecordedAtUtc)
                .ToListAsync();
        }

        /// <summary>
        /// Returns locations recorded within the last <paramref name="hours"/> hours.
        /// Useful for limiting the heat-map to a recent session.
        /// </summary>
        public async Task<List<LocationEntry>> GetRecentLocationsAsync(int hours = 24)
        {
            EnsureInitialised();
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            return await _connection!
                .Table<LocationEntry>()
                .Where(e => e.RecordedAtUtc >= cutoff)
                .OrderBy(e => e.RecordedAtUtc)
                .ToListAsync();
        }

        // ── Delete ────────────────────────────────────────────────────────────

        /// <summary>Removes all stored location records — use carefully!</summary>
        public async Task ClearAllAsync()
        {
            EnsureInitialised();
            await _connection!.DeleteAllAsync<LocationEntry>();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Throws if <see cref="InitAsync"/> has not been called yet.
        /// Guards against accidental use before the DB is ready.
        /// </summary>
        private void EnsureInitialised()
        {
            if (_connection is null)
                throw new InvalidOperationException(
                    "DatabaseService.InitAsync() must be awaited before use.");
        }
    }
}
