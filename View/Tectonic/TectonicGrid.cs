using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace View.Tectonic;

public class TectonicGrid : FrameworkElement
{
    private readonly DrawingVisual _visual = new();

    private readonly List<IDrawableComponent>[] _components = { new() };
    
    public TectonicGrid(int width, int height)
    {
        Focusable = true;
        
        Loaded += AddVisualToTree;
        Unloaded += RemoveVisualFromTree;

        Width = width;
        Height = height;
    }
    
    //DrawingNecessities------------------------------------------------------------------------------------------------
    
    // Provide a required override for the VisualChildrenCount property.
    protected override int VisualChildrenCount => 1;

    // Provide a required override for the GetVisualChild method.
    protected override Visual GetVisualChild(int index)
    {
        return _visual;
    }
    
    private void AddVisualToTree(object sender, RoutedEventArgs e)
    {
        AddVisualChild(_visual);
        AddLogicalChild(_visual);
    }

    private void RemoveVisualFromTree(object sender, RoutedEventArgs e)
    {
        RemoveLogicalChild(_visual);
        RemoveVisualChild(_visual);
    }
    
    //Public------------------------------------------------------------------------------------------------------------
    
    public void Refresh()
    {
        var context = _visual.RenderOpen();

        foreach (var list in _components)
        {
            foreach (var component in list)
            {
                component.Draw(context);
            }
        }
        
        context.Close();
        InvalidateVisual();
    }
}