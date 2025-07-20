using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using RescueScoreManager.Data;

using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.SelectNewCompetition;
public partial class CompetitionDisplayItem : ObservableObject
{
    private readonly IImageService? _imageService;

    [ObservableProperty]
    private BitmapImage? _organizerImage;

    [ObservableProperty]
    private bool _isLoadingImage;

    [ObservableProperty]
    private string _organizerInitial;

    public Competition Competition { get; }

    public CompetitionDisplayItem(Competition competition, IImageService? imageService = null)
    {
        Competition = competition ?? throw new ArgumentNullException(nameof(competition));
        _imageService = imageService;

        // Set the initial letter as fallback
        _organizerInitial = !string.IsNullOrEmpty(competition.Organizer)
            ? competition.Organizer.Trim().Substring(0, 1).ToUpper()
            : "?";

        // Start loading image in background if image service is available
        if (_imageService != null)
        {
            _ = LoadOrganizerImageAsync();
        }
    }

    private async Task LoadOrganizerImageAsync()
    {
        if (_imageService == null) { return; }

        IsLoadingImage = true;
        try
        {
            // First try to get club logo URL (you might need to implement this based on your data structure)
            string? logoUrl = Competition.OrganizerLogoUrl;
            if (!string.IsNullOrEmpty(logoUrl))
            {
                OrganizerImage = await _imageService.GetImageAsync(logoUrl);
                if (OrganizerImage != null) { return; }
            }

            // Then try cap URL
            string? capUrl = Competition.OrganizerCapUrl;
            if (!string.IsNullOrEmpty(capUrl))
            {
                OrganizerImage = await _imageService.GetImageAsync(capUrl);
            }
        }
        catch (Exception)
        {
            // Log if needed, but don't throw - fallback to initial letter
        }
        finally
        {
            IsLoadingImage = false;
        }
    }
}
