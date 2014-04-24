using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TPG.GeoFramework.Symbols.Contracts.Providers;
using TPG.GeoFramework.Symbols.Contracts.Storage;
using Color = System.Windows.Media.Color;

namespace TestMapApp
{
    public class BitmapFileSymbolProvider : IRasterSymbolProvider
    {
        public RasterSymbol GetSymbol(string symbolCode, double scale, SymbolColorScheme symbolColorScheme, bool dropShadow,
            Color? dropShadowColor, SymbolColorOverride? symbolColorOverride, SortedDictionary<string, string> customParameters)
        {
            RasterSymbol symbol = new RasterSymbol();

            // symbolCode is a full path to the image to use as a symbol
            BitmapSource bm = new BitmapImage(new Uri(symbolCode, UriKind.Relative));

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

        public double BaseSymbolScaling { get { return 1.0; } }
    }
}