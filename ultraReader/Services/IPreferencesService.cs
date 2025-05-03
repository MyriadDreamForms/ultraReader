using System.Threading.Tasks;
using ultraReader.Models.Entities;
using ultraReader.Models.DTOs;
using ultraReader.Models.Enums;

namespace ultraReader.Services
{
    public interface IPreferencesService
    {
        Task<UserPreferences> GetUserPreferencesAsync(string userId);
        Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences);
        string ConvertReadingModeToDirection(ReadingMode mode);
        ReadingMode ConvertDirectionToReadingMode(string direction);
        Task<ReadingMode> GetUserReadingModeAsync(string userId);
        Task UpdateUserPreferencesAsync(UserPreferences preferences);
    }
} 