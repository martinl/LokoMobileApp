using Mapsui.Rendering.Skia.Extensions;
using Mapsui;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Widgets;
using Color = Mapsui.Styles.Color;
using Font = Mapsui.Styles.Font;

namespace loko.Models
{
    public class CustomCalloutStyle : CalloutStyle
    {
        public CustomCalloutStyle()
        {
            // Set custom dimensions
            ArrowWidth = 8;
            ArrowHeight = 8;
            RectRadius = 3;  // Rounded corners
            Padding = new MRect(2, 2, 2, 2);
            Type=CalloutType.Custom;

            // Set colors with transparency
            BackgroundColor = Color.DarkGreen;  // Dark background color with alpha
            Color = Color.Green;  // Green border color
            StrokeWidth = 2;  // Border width

            // Set font properties
            TitleFont = new Font { Size = 10, Bold = false };
            SubtitleFont = new Font { Size = 10, Bold = false };
            TitleFontColor = new Color(255, 255, 255);  // White text
            SubtitleFontColor = new Color(255, 255, 255);  // White text

            // Set alignments
            TitleTextAlignment = Alignment.Left;
            SubtitleTextAlignment = Alignment.Left;

            // Set spacing and max width
            Spacing = 1;
            MaxWidth = 200;

            // Other customizations
            ShadowWidth = 2;  // No shadow
            ArrowAlignment = ArrowAlignment.Bottom;  // Place arrow at the bottom
            ArrowPosition = 0.5f;  // Center the arrow
        }
    }
}
