﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopApplication.View.Utility;

public static class ImageUtility
{
    public static BitmapSource SetIconColor(BitmapSource image, Color color)
    {
        var wb = new WriteableBitmap(image);
        
        var width = wb.PixelWidth;
        var height = wb.PixelHeight;
        var stride = wb.BackBufferStride;
        var bytesPerPixel = 4;
        
        try
        {
            unsafe
            {
                var pBuff = (byte*)wb.BackBuffer.ToPointer();
                wb.Lock();
                
                for (var row = 0; row < height; row++)
                {
                    for (var col = 0; col < width; col++)
                    {
                        if (pBuff[row * stride + col * bytesPerPixel + 3] == 0) continue;
                        
                        pBuff[row * stride + col * bytesPerPixel + 0] = color.R;
                        pBuff[row * stride + col * bytesPerPixel + 1] = color.G;
                        pBuff[row * stride + col * bytesPerPixel + 2] = color.B;
                    }
                }

                wb.AddDirtyRect(new Int32Rect(0, 0, (int)wb.Width,
                    (int)wb.Height));
            }
        }
        finally
        {
            wb.Unlock();
        }
        
        return wb;
    }
}