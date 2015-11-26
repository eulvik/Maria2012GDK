using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TPG.GeoFramework.Symbols.Contracts.Providers;
using TPG.GeoFramework.Symbols.Contracts.Storage;

namespace TestMapApp2_0
{
    class BitmapSymbolProvider : IRasterSymbolProvider
    {
        public RasterSymbol GetSymbol(string symbolCode, double scale, SymbolColorScheme symbolColorScheme, bool dropShadow,
            Color? dropShadowColor, SymbolColorOverride? symbolColorOverride, SortedDictionary<string, string> customParameters)
        {
            RasterSymbol symbol = new RasterSymbol();
            string path = @"http://www.teleplanglobe.no/files/media/productlogo/0001/01/thumb_5_productlogo_medium.png";
            // symbolCode is a full path to the image to use as a symbol
            BitmapSource bm = new BitmapImage(new Uri(path, UriKind.Absolute));

            if (Math.Abs(scale - 1.0) > 1e-5)
            {
                var tbm = new TransformedBitmap(bm, new
                ScaleTransform(scale, scale));
                bm = tbm;
            }

            symbol.Data = bm;
            symbol.Width = bm.PixelWidth;
            symbol.Height = bm.PixelHeight;
            symbol.SelectWidth = bm.PixelWidth;
            symbol.SelectHeight = bm.PixelHeight;
            return symbol;

        }

        public string GetProviderType()
        {
            return "eSmart";
        }

        public List<string> GetAvailableSymbols()
        {
            return new List<string>();
        }

        public double BaseSymbolScaling { get { return 1.0; } }
    }
}
