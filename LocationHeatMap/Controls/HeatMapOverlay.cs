// =============================================================================
// HeatMapOverlay.cs
// Custom GraphicsView drawable that renders a kernel-density heat map
// on top of the .NET MAUI Map control.
// Each recorded point is drawn as a radial gradient; overlapping gradients
// blend additively to create the characteristic "hot" clusters.
// Author: Student — CS Mobile Development
// =============================================================================

using LocationHeatMap.Models;
using Microsoft.Maui.Graphics;

namespace LocationHeatMap.Controls
{
    /// <summary>
    /// IDrawable implementation for the heat map canvas.
    /// Assign this to a <see cref="GraphicsView.Drawable"/> property.
    /// Call <see cref="UpdatePoints"/> whenever the underlying data changes.
    /// </summary>
    public class HeatMapOverlay : IDrawable
    {
        // ── Rendering configuration ───────────────────────────────────────────

        /// <summary>Radius (in display-independent pixels) of each heat blob.</summary>
        private const float BlobRadius = 40f;

        /// <summary>Alpha ceiling of the hottest cluster (0-255).</summary>
        private const int MaxAlpha = 200;

        // ── State ─────────────────────────────────────────────────────────────

        /// <summary>Screen-space (x, y) positions of every data point.</summary>
        private List<PointF> _screenPoints = new();

        /// <summary>Bounding box of all recorded GPS coordinates.</summary>
        private MapBounds? _bounds;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Converts GPS entries to screen-space coordinates and schedules a redraw.
        /// Must be called on the UI thread.
        /// </summary>
        /// <param name="entries">All location records to render.</param>
        /// <param name="canvasWidth">Current pixel width of the host GraphicsView.</param>
        /// <param name="canvasHeight">Current pixel height of the host GraphicsView.</param>
        public void UpdatePoints(
            IEnumerable<LocationEntry> entries,
            float canvasWidth,
            float canvasHeight)
        {
            var list = entries.ToList();
            if (list.Count == 0)
            {
                _screenPoints.Clear();
                return;
            }

            // Calculate the geographic bounding box.
            double minLat = list.Min(e => e.Latitude);
            double maxLat = list.Max(e => e.Latitude);
            double minLon = list.Min(e => e.Longitude);
            double maxLon = list.Max(e => e.Longitude);

            // Add 10 % padding so edge points aren't clipped.
            double latPad = (maxLat - minLat) * 0.10;
            double lonPad = (maxLon - minLon) * 0.10;

            _bounds = new MapBounds(
                minLat - latPad, maxLat + latPad,
                minLon - lonPad, maxLon + lonPad);

            // Map each GPS coordinate to a pixel position.
            _screenPoints = list.Select(e => ToScreen(e, canvasWidth, canvasHeight)).ToList();
        }

        // ── IDrawable ─────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the MAUI graphics engine on every frame.
        /// Draws radial gradients for each recorded point.
        /// </summary>
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_screenPoints.Count == 0)
                return;

            canvas.SaveState();
            canvas.Alpha = 0.8f; // Overall overlay transparency

            foreach (var pt in _screenPoints)
            {
                DrawBlob(canvas, pt.X, pt.Y);
            }

            canvas.RestoreState();
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Draws a single radial gradient "blob" centred at (cx, cy).
        /// Colour transitions: red (hot centre) → yellow → transparent (cool edge).
        /// </summary>
        private static void DrawBlob(ICanvas canvas, float cx, float cy)
        {
            // Use concentric filled circles at decreasing opacity to simulate
            // a radial gradient (MAUI's RadialGradientBrush is not available on
            // GraphicsView; this approach works cross-platform).
            const int steps = 8;
            for (int i = steps; i >= 1; i--)
            {
                float fraction = (float)i / steps;           // 1 = edge, ~0 = centre
                float radius   = BlobRadius * fraction;
                int   alpha    = (int)(MaxAlpha * (1f - fraction + 0.1f));

                // Interpolate colour: edge = blue, mid = yellow, centre = red
                Color blobColor = InterpolateHeatColor(1f - fraction);

                canvas.FillColor = blobColor.WithAlpha(alpha / 255f);
                canvas.FillCircle(cx, cy, radius);
            }
        }

        /// <summary>
        /// Maps a normalised heat value (0 = cold, 1 = hot) to a colour on the
        /// classic blue → cyan → green → yellow → red heat-map spectrum.
        /// </summary>
        private static Color InterpolateHeatColor(float heat)
        {
            // Four-stop gradient using linear interpolation between stops.
            return heat switch
            {
                <= 0.25f => LerpColor(Colors.Blue,   Colors.Cyan,   heat / 0.25f),
                <= 0.50f => LerpColor(Colors.Cyan,   Colors.Green,  (heat - 0.25f) / 0.25f),
                <= 0.75f => LerpColor(Colors.Green,  Colors.Yellow, (heat - 0.50f) / 0.25f),
                _        => LerpColor(Colors.Yellow, Colors.Red,    (heat - 0.75f) / 0.25f),
            };
        }

        /// <summary>Linearly interpolates between two colours.</summary>
        private static Color LerpColor(Color a, Color b, float t) =>
            new(
                a.Red   + (b.Red   - a.Red)   * t,
                a.Green + (b.Green - a.Green) * t,
                a.Blue  + (b.Blue  - a.Blue)  * t);

        /// <summary>
        /// Projects a GPS coordinate onto the canvas using a simple equirectangular
        /// projection (accurate enough for city-scale heat maps).
        /// </summary>
        private PointF ToScreen(LocationEntry entry, float width, float height)
        {
            if (_bounds is null)
                return new PointF(0, 0);

            float x = (float)((entry.Longitude - _bounds.MinLon) /
                               (_bounds.MaxLon  - _bounds.MinLon)) * width;

            // Latitude increases upward but screen Y increases downward — flip it.
            float y = (float)(1.0 - (entry.Latitude - _bounds.MinLat) /
                                    (_bounds.MaxLat  - _bounds.MinLat)) * height;

            return new PointF(x, y);
        }

        // ── Inner types ───────────────────────────────────────────────────────

        /// <summary>Holds the geographic bounding box of the data set.</summary>
        private sealed record MapBounds(
            double MinLat, double MaxLat,
            double MinLon, double MaxLon);
    }
}
