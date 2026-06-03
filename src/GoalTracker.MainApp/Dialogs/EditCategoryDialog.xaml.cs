using GoalTracker.Shared.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace GoalTracker.MainApp.Dialogs;

public sealed partial class EditCategoryDialog : ContentDialog
{
    public Category Category { get; private set; }

    private static readonly string[] Palette =
    [
        "#3D8EF0", "#4CAF50", "#FF6B35", "#9C27B0",
        "#F44336", "#00BCD4", "#FF9800", "#607D8B"
    ];

    private string _selectedColor;
    private readonly List<Ellipse> _colorDots = [];

    public EditCategoryDialog(Category category)
    {
        Category = new Category
        {
            Id    = category.Id,
            Name  = category.Name,
            Color = category.Color,
            Emoji = category.Emoji
        };
        _selectedColor = Category.Color;

        InitializeComponent();

        NameBox.Text  = Category.Name;
        EmojiBox.Text = Category.Emoji;

        BuildPalette();

        PrimaryButtonClick += (_, _) =>
        {
            Category.Name  = NameBox.Text.Trim();
            Category.Emoji = EmojiBox.Text.Trim();
            Category.Color = _selectedColor;
        };
    }

    private void BuildPalette()
    {
        foreach (var hex in Palette)
        {
            var dot = new Ellipse
            {
                Width  = 28,
                Height = 28,
                Fill   = HexBrush(hex)
            };

            if (hex == _selectedColor)
                dot.Stroke = new SolidColorBrush(Colors.White);

            string color = hex;
            dot.PointerPressed += (_, _) => SelectColor(color);

            PalettePanel.Children.Add(dot);
            _colorDots.Add(dot);
        }
    }

    private void SelectColor(string hex)
    {
        _selectedColor = hex;
        for (int i = 0; i < Palette.Length; i++)
        {
            _colorDots[i].Stroke = Palette[i] == hex
                ? new SolidColorBrush(Colors.White)
                : null;
        }
    }

    private static SolidColorBrush HexBrush(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex[..2], 16);
        byte g = Convert.ToByte(hex[2..4], 16);
        byte b = Convert.ToByte(hex[4..6], 16);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
    }
}
