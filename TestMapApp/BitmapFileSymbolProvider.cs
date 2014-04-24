using System;
using System.Collections.Generic;
using System.Drawing;
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
            var rs = new RasterSymbol();
            BitmapSource bitmapImage = new BitmapImage(new Uri(symbolCode, UriKind.RelativeOrAbsolute));

            var tbm = new TransformedBitmap(bitmapImage, new ScaleTransform(50.0 / bitmapImage.Width, 50.0 / bitmapImage.Height));
            bitmapImage = tbm;

            rs.Data = bitmapImage;
            rs.Width = bitmapImage.PixelWidth;
            rs.SelectWidth = bitmapImage.PixelWidth;
            rs.Height = bitmapImage.PixelHeight;
            rs.SelectHeight = bitmapImage.PixelHeight;
            rs.CenterX = bitmapImage.PixelWidth * 0.5;
            rs.CenterY = bitmapImage.PixelHeight * 0.5;

            return rs;
        }

        public double BaseSymbolScaling { get  {return 1.0; } }
    }
}