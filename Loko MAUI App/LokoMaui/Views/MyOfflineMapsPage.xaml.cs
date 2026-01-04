using loko.Services;

namespace loko.Views;

public partial class MyOfflineMapsPage : ContentPage
{
    public MyOfflineMapsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDownloadedAreas();
    }

    private async Task LoadDownloadedAreas()
    {
        AreasContainer.Children.Clear();
        var downloadAreaManager = new DownloadedAreaManager();
        var areas = await downloadAreaManager.GetAreasAsync();

        foreach (var area in areas)
        {
            var frame = new Frame
            {
                BorderColor = Colors.Gray,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Set the background color of the frame based on the theme
            frame.SetAppThemeColor(VisualElement.BackgroundColorProperty, Colors.White, Color.FromArgb("#1C1C1E"));

            var stackLayout = new StackLayout();

            var nameLabel = new Label
            {
                Text = area.Name,
                FontAttributes = FontAttributes.Bold
            };
            nameLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            var sizeLabel = new Label
            {
                Text = $"Size: {area.DownloadSize:F2} MB"
            };
            sizeLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            var dateLabel = new Label
            {
                Text = $"Downloaded: {area.DownloadDate:g}"
            };
            dateLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            var deleteButton = new Button
            {
                Text = "Delete",
                BackgroundColor = Colors.Red,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.End
            };
            deleteButton.Clicked += async (sender, e) => await DeleteArea(area);

            stackLayout.Children.Add(nameLabel);
            stackLayout.Children.Add(sizeLabel);
            stackLayout.Children.Add(dateLabel);
            stackLayout.Children.Add(deleteButton);

            frame.Content = stackLayout;
            AreasContainer.Children.Add(frame);
        }
    }

    private async Task DeleteArea(DownloadedArea area)
    {
        bool answer = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the area '{area.Name}'?", "Yes", "No");
        if (answer)
        {
            var downloadAreaManager = new DownloadedAreaManager();
            await downloadAreaManager.DeleteAreaAsync(area.Name);
            await LoadDownloadedAreas();
        }
    }
}