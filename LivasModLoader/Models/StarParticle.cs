using System.Windows.Media;

namespace LivasModLoader.Models;

public class StarParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Size { get; set; }
    public double Opacity { get; set; }
    public Brush Brush { get; set; } = Brushes.White;
}
