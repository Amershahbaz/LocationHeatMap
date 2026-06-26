using System.Collections.Specialized;
using System.Text;
using LocationHeatMap.Controls;
using LocationHeatMap.Models;
using LocationHeatMap.ViewModels;

namespace LocationHeatMap.Views
{
    public partial class MainView : ContentPage
    {
        private readonly MainViewModel _vm;
        private readonly HeatMapOverlay _overlay;

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            BindingContext = _vm;
            _overlay = new HeatMapOverlay();
            HeatMapCanvas.Drawable = _overlay;
            MapWebView.Source = GetMapHtml();
            _vm.Locations.CollectionChanged += OnLocationsChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _vm.InitialiseAsync();
                RefreshHeatMap();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"OnAppearing: {ex.Message}");
            }
        }

        private void OnLocationsChanged(
            object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshHeatMap();
        }

        private void RefreshHeatMap()
        {
            try
            {
                _overlay.UpdatePoints(
                    _vm.Locations,
                    (float)HeatMapCanvas.Width,
                    (float)HeatMapCanvas.Height);
                HeatMapCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"RefreshHeatMap: {ex.Message}");
            }
        }

        private static HtmlWebViewSource GetMapHtml()
        {
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html><html><head>");
            html.Append("<meta name='viewport' " +
                "content='width=device-width,initial-scale=1.0'>");
            html.Append("<link rel='stylesheet' " +
                "href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>");
            html.Append("<style>" +
                "body{margin:0}" +
                "#map{width:100vw;height:100vh}" +
                "</style>");
            html.Append("</head><body>");
            html.Append("<div id='map'></div>");
            html.Append("<script src=" +
                "'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'>" +
                "</script>");
            html.Append("<script>");
            html.Append("var map=L.map('map')" +
                ".setView([37.3318,-122.0312],14);");
            html.Append("L.tileLayer(" +
                "'https://tile.openstreetmap.org/{z}/{x}/{y}.png'," +
                "{maxZoom:19," +
                "attribution:'© OpenStreetMap'}" +
                ").addTo(map);");
            html.Append("</script>");
            html.Append("</body></html>");
            return new HtmlWebViewSource { Html = html.ToString() };
        }
    }
}