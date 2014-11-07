using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TPG.GeoFramework.Symbols.Contracts.Providers;
using TPG.GeoFramework.Symbols.Contracts.Storage;
using Color = System.Windows.Media.Color;

namespace TestMapApp
{
    public class BitmapFileSymbolProvider : IRasterSymbolProvider
    {
        private string _providerId;

        public BitmapFileSymbolProvider(string providerId)
        {
            _providerId = providerId;
        }
        public RasterSymbol GetSymbol(string symbolCode, double scale, SymbolColorScheme symbolColorScheme, bool dropShadow,
            Color? dropShadowColor, SymbolColorOverride? symbolColorOverride, SortedDictionary<string, string> customParameters)
        {
            try
            {
                System.Console.WriteLine("{0} : {1}", _providerId, symbolCode);
                string url = string.Format(@"http://mariamapserver.teleplan.no:9015/symbolservice/getsymbol/{0}/{2}/{1}.png", _providerId, symbolCode, scale.ToString(CultureInfo.InvariantCulture));
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                PngBitmapDecoder decoder = new PngBitmapDecoder(resp.GetResponseStream(), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapSource bitmapSource = decoder.Frames[0];
                return new RasterSymbol
                {
                    CenterX = 0.5,
                    CenterY = 0.5,
                    Data = bitmapSource,
                    Height = bitmapSource.PixelHeight,
                    Width = bitmapSource.PixelWidth,
                };
            }
            catch (Exception e)
            {
                return new RasterSymbol();
            }
        }

        public string GetProviderType()
        {
            return _providerId;
        }

        public List<string> GetAvailableSymbols()
        {
            return new List<string>();
        }

        public double BaseSymbolScaling { get { return 1.0; } }
    }
}