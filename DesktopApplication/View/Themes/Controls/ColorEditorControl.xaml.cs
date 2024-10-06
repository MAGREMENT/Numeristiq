using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Model.Utility;

namespace DesktopApplication.View.Themes.Controls;

public partial class ColorEditorControl
{
    private RGB _color;
    private HSL _hsl;
    private bool _fireEvent;

    public event OnColorChange? ColorChanged;

    public RGB Color
    {
        get => _color;
        set => SetColor(value, true, true, true);
    }
    
    public bool ContinuousUpdate { get; set; }
    
    public ColorEditorControl()
    {
        InitializeComponent();
        InitializeHueSlider();
        UpdateHueCursor();
        UpdateSLMap(true);
        UpdateSLCursor();
        
        _fireEvent = true;
    }

    public void NoColor()
    {
        _fireEvent = false;
        SetColor(new RGB(), false, false, false);
        
        RedValue.SetCurrent(0);
        GreenValue.SetCurrent(0);
        BlueValue.SetCurrent(0);
        HexCode.Text = string.Empty;
        
        HueCursor.Visibility = Visibility.Hidden;
        SLCursor.Visibility = Visibility.Hidden;

        _fireEvent = true;
    }

    private void SetColor(RGB rgb, bool updateColorValues, bool updateHSL, bool updateHexaString)
    {
        _color = rgb;
        _fireEvent = false;

        if (updateColorValues)
        {
            RedValue.SetCurrent(_color.Red);
            GreenValue.SetCurrent(_color.Green);
            BlueValue.SetCurrent(_color.Blue);
        }

        if (updateHexaString)
        {
            HexCode.Text = _color.ToHexString();
        }

        if (updateHSL) SetHSL(rgb.ToHSL(), true, true, true, false);
        

        _fireEvent = true;
    }

    private void SetHSL(HSL hsl, bool updateHueCursor, bool updateSLMap, bool updateSLCursor, bool updateColor)
    {
        _hsl = hsl;
        _fireEvent = false;
        
        if(updateHueCursor) UpdateHueCursor();

        if(updateSLMap) UpdateSLMap(false);
        
        if(updateSLCursor) UpdateSLCursor();
        
        if(updateColor) SetColor(_hsl.ToRGB(), true, false, true);

        _fireEvent = true;
    }

    private void InitializeHueSlider()
    {
        var w = (int)HueSlider.Width;
        var h = (int)HueSlider.Height;
        
        RenderOptions.SetBitmapScalingMode(HueSlider, BitmapScalingMode.HighQuality);
        RenderOptions.SetEdgeMode(HueSlider, EdgeMode.Aliased);
        var bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);
        ((ImageBrush)HueSlider.Background).ImageSource = bitmap;

        try
        {
            bitmap.Lock();

            unsafe
            {
                var ptr = bitmap.BackBuffer;

                for (int col = 0; col < w; col++)
                {
                    var hue = (double)col / w * 360;
                    var hsl = new HSL((int)hue, 0.5, 0.5);
                    var rgb = hsl.ToRGB();
                    var colorData = (rgb.Red << 16) | (rgb.Green << 8) | rgb.Blue; 

                    for (int row = 0; row < h; row++)
                    {
                        var index = ptr + row * bitmap.BackBufferStride + col * 4;
                        *(int*)index = colorData;
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, w, h));
            }
        }
        finally
        {
            bitmap.Unlock();
        }
    }

    private void UpdateSLMap(bool initialize)
    {
        var w = (int)SLMap.Width;
        var h = (int)SLMap.Height;
        WriteableBitmap bitmap;
        if (initialize)
        {
            RenderOptions.SetBitmapScalingMode(SLMap, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(SLMap, EdgeMode.Aliased);
            bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);
            ((ImageBrush)SLMap.Background).ImageSource = bitmap;
        }
        else bitmap = (WriteableBitmap)((ImageBrush)SLMap.Background).ImageSource;
        
        try
        {
            bitmap.Lock();

            unsafe
            {
                var ptr = bitmap.BackBuffer;

                for (int col = 0; col < w; col++)
                {
                    for (int row = 0; row < h; row++)
                    {
                        var s = (double)col / (w - 1);
                        var temp = 1 - (double)row / (h - 1);
                        var l = temp - 0.5 * s * temp;
                        var hsl = new HSL(_hsl.Hue, s, l);
                        var rgb = hsl.ToRGB();
                        var colorData = (rgb.Red << 16) | (rgb.Green << 8) | rgb.Blue;  
                        
                        var index = ptr + row * bitmap.BackBufferStride + col * 4;
                        *(int*)index = colorData;
                    }
                }

                bitmap.AddDirtyRect(new Int32Rect(0, 0, w, h));
            }
        }
        finally
        {
            bitmap.Unlock();
        }
    }

    private void UpdateHueCursor()
    {
        HueCursor.Visibility = Visibility.Visible;
        var left = (HueWrapper.Width - HueSlider.Width) / 2 
            + _hsl.Hue / 360.0 * HueSlider.Width - HueCursor.Width / 2;
        HueCursor.Margin = new Thickness(left, 0, 0, 0);
    }
    
    private void UpdateSLCursor()
    {
        SLCursor.Visibility = Visibility.Visible;
        var left = (SLWrapper.Width - SLMap.Width) / 2 
            + _hsl.Saturation * SLMap.Width - SLCursor.Width / 2;
        var y = Math.Max(0, 1 - _hsl.Lightness / (1 - 0.5 * _hsl.Saturation));
        var top = (SLWrapper.Height - SLMap.Height) / 2 
            + y * SLMap.Height - SLCursor.Height / 2;
        SLCursor.Margin = new Thickness(left, top, 0, 0);
    }
    
    private void OnRedChange(int r)
    {
        if (!_fireEvent || r == _color.Red) return;
        
        SetColor(_color.WithRed((byte)r), false, true, true);
        ColorChanged?.Invoke(Color);
    }
    
    private void OnGreenChange(int g)
    {
        if (!_fireEvent || g == _color.Green) return;

        SetColor(_color.WithGreen((byte)g), false, true, true);
        ColorChanged?.Invoke(Color);
    }
    
    private void OnBlueChange(int b)
    {
        if (!_fireEvent || b == _color.Blue) return;

        SetColor(_color.WithBlue((byte)b), false, true, true);
        ColorChanged?.Invoke(Color);
    }

    private void OnHueChange(object sender, MouseEventArgs e)
    {
        if (!_fireEvent || e.LeftButton != MouseButtonState.Pressed) return;
        
        var x = e.MouseDevice.GetPosition(HueSlider).X;
        SetHSL(_hsl.WithHue((int)Math.Round(x / (HueSlider.Width - 1) * 360)),
            true, true, false, true);
        
        if(ContinuousUpdate && _fireEvent) ColorChanged?.Invoke(Color);
    }
    
    private void OnSLChange(object sender, MouseEventArgs e)
    {
        if (!_fireEvent || e.LeftButton != MouseButtonState.Pressed) return;
        
        var p = e.MouseDevice.GetPosition(SLMap);
        var s = p.X / (int)SLMap.Width;
        var temp = 1 - p.Y / (int)SLMap.Height;
        var l = temp - 0.5 * s * temp;
        SetHSL(new HSL(_hsl.Hue, s, l),
            true, false, true, true);
        if(ContinuousUpdate) ColorChanged?.Invoke(Color);
    }

    private void StopOnUp(object sender, MouseEventArgs e)
    {
        if(!ContinuousUpdate && _fireEvent) ColorChanged?.Invoke(Color);
    }
    
    private void StopOnLeave(object sender, MouseEventArgs e)
    {
        if(!ContinuousUpdate && e.LeftButton == MouseButtonState.Pressed && _fireEvent) ColorChanged?.Invoke(Color);
    }

    private void OnHexCodeChanged(object sender, TextChangedEventArgs e)
    {
        if(!_fireEvent) return;

        var s = HexCode.Text;
        var alphabet = HexadecimalAlphabet.Instance;
        if (s.Length != 7 || !alphabet.Contains(s[1..]))
        {
            _fireEvent = false;
            HexCode.Text = _color.ToHexString();
            _fireEvent = true;
            return;
        }

        SetColor(RGB.FromHexString(s), true, true, false);
        ColorChanged?.Invoke(Color);
    }
}

public delegate void OnColorChange(RGB color);