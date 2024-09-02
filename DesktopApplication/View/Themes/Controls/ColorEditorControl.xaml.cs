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
        set => SetColor(value, true, true);
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

    public void NoColor() //TODO
    {
        _fireEvent = false;
        SetColor(new RGB(), false, false);
        
        RedValue.Text = string.Empty;
        GreenValue.Text = string.Empty;
        BlueValue.Text = string.Empty;
        HueCursor.Visibility = Visibility.Hidden;

        _fireEvent = true;
    }

    private void SetColor(RGB rgb, bool updateColorValues, bool updateHSL)
    {
        _color = rgb;
        _fireEvent = false;

        if (updateColorValues)
        {
            RedValue.Text = _color.Red.ToString();
            GreenValue.Text = _color.Green.ToString();
            BlueValue.Text = _color.Blue.ToString();
        }

        if (updateHSL)
        {
            SetHSL(rgb.ToHSL(), true, true, true, false);
        }

        _fireEvent = true;
    }

    private void SetHSL(HSL hsl, bool updateHueCursor, bool updateSLMap, bool updateSLCursor, bool updateColor)
    {
        _hsl = hsl;
        _fireEvent = false;
        
        if(updateHueCursor) UpdateHueCursor();

        if(updateSLMap) UpdateSLMap(false);
        
        if(updateSLCursor) UpdateSLCursor();
        
        if(updateColor) SetColor(_hsl.ToRGB(), true, false);

        _fireEvent = true;
    }

    private void OnRedChange(object sender, EventArgs e)
    {
        if (!_fireEvent || sender is not TextBox box) return;

        if (box.Text.Length > 3)
        {
            _fireEvent = false;
            box.Text = box.Text[..3];
            _fireEvent = true;
            return;
        }

        byte r;
        if (string.IsNullOrWhiteSpace(box.Text)) r = 0;
        else
        {
            if (int.TryParse(box.Text, out var i)) r = (byte)Math.Max(0, Math.Min(255, i));
            else r = _color.Red;
            
            _fireEvent = false;
            box.Text = r.ToString();
            _fireEvent = true;
        }

        if (r == _color.Red) return;
        
        SetColor(_color.WithRed(r), false, true);
        ColorChanged?.Invoke(Color);
    }
    
    private void OnGreenChange(object sender, EventArgs e)
    {
        if (!_fireEvent || sender is not TextBox box) return;

        if (box.Text.Length > 3)
        {
            _fireEvent = false;
            box.Text = box.Text[..3];
            _fireEvent = true;
            return;
        }

        byte g;
        if (string.IsNullOrWhiteSpace(box.Text)) g = 0;
        else
        {
            if (int.TryParse(box.Text, out var i)) g = (byte)Math.Max(0, Math.Min(255, i));
            else g = _color.Green;
            
            _fireEvent = false;
            box.Text = g.ToString();
            _fireEvent = true;
        }

        if (g == _color.Green) return;

        SetColor(_color.WithGreen(g), false, true);
        ColorChanged?.Invoke(Color);
    }
    
    private void OnBlueChange(object sender, EventArgs e)
    {
        if (!_fireEvent || sender is not TextBox box) return;

        if (box.Text.Length > 3)
        {
            _fireEvent = false;
            box.Text = box.Text[..3];
            _fireEvent = true;
            return;
        }

        byte b;
        if (string.IsNullOrWhiteSpace(box.Text)) b = 0;
        else
        {
            if (int.TryParse(box.Text, out var i)) b = (byte)Math.Max(0, Math.Min(255, i));
            else b = _color.Blue;

            _fireEvent = false;
            box.Text = b.ToString();
            _fireEvent = true;
        }

        if (b == _color.Blue) return;

        SetColor(_color.WithBlue(b), false, true);
        ColorChanged?.Invoke(Color);
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
                        var s = (double)col / w;
                        var temp = 1 - (double)row / h;
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
        var y = 1 - _hsl.Lightness / (1 - 0.5 * _hsl.Saturation);
        var top = (SLWrapper.Height - SLMap.Height) / 2 
            + y * SLMap.Height - SLCursor.Height / 2;
        SLCursor.Margin = new Thickness(left, top, 0, 0);
    }

    private void OnHueChange(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        
        var x = e.MouseDevice.GetPosition(HueSlider).X;
        SetHSL(_hsl.WithHue((int)Math.Round(x / (HueSlider.Width - 1) * 360)),
            true, true, false, true);
        
        if(ContinuousUpdate) ColorChanged?.Invoke(Color);
    }
    
    private void OnSLChange(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        
        var p = e.MouseDevice.GetPosition(SLMap);
        var s = p.X / SLMap.Width;
        var temp = 1 - p.Y / SLMap.Height;
        var l = temp - 0.5 * s * temp;
        SetHSL(new HSL(_hsl.Hue, s, l),
            true, false, true, true);
        if(ContinuousUpdate) ColorChanged?.Invoke(Color);
    }

    private void StopOnUp(object sender, MouseEventArgs e)
    {
        if(!ContinuousUpdate) ColorChanged?.Invoke(Color);
    }
    
    private void StopOnLeave(object sender, MouseEventArgs e)
    {
        if(!ContinuousUpdate && e.LeftButton == MouseButtonState.Pressed) ColorChanged?.Invoke(Color);
    }
}

public delegate void OnColorChange(RGB color);