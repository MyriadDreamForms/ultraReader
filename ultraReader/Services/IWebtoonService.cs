using System.Collections.Generic;
using System.Threading.Tasks;
using ultraReader.Models.DTOs;
using ultraReader.Models.Entities;

namespace ultraReader.Services
{
    public interface IWebtoonService
    {
        Task<List<WebtoonInfo>> GetAllWebtoonsAsync();
        Task<WebtoonDetailDto> GetWebtoonDetailsAsync(string webtoonName);
        Task<ReaderViewModel> GetChapterImagesAsync(string webtoonName, string chapterName);
        Task<WebtoonInfo> GetWebtoonByIdAsync(string id);
        Task<bool> UpdateWebtoonAsync(WebtoonInfo webtoon);
        Task<bool> AddWebtoonAsync(WebtoonInfo webtoon);
        Task<bool> DeleteWebtoonAsync(string id);
        Task<WebtoonInfo> GetWebtoonByFolderNameAsync(string folderName);
        Task<int> GetTotalWebtoonCountAsync();
        Task<List<WebtoonInfo>> GetLatestWebtoonsAsync(int count);
        Task<List<string>> GetAllGenresAsync();
        Task<List<string>> GetAllStatusesAsync();
        
        // Grafik istatistikleri için metodlar
        Task<Dictionary<string, int>> GetWebtoonsByGenreAsync();
        Task<Dictionary<string, int>> GetWebtoonsByStatusAsync();
        Task<Dictionary<string, int>> GetWebtoonCategoriesAsync();
        Task<int> GetTotalChapterCountAsync();
        Task<List<Models.Entities.ChapterInfo>> GetChaptersAsync(string webtoonFolderName);
        Task<bool> AddChapterAsync(Models.Entities.ChapterInfo chapter);
        Task<Dictionary<string, int>> GetViewsPerDayAsync(int days = 7);
        Task<Dictionary<string, object>> GetWebtoonStatisticsAsync();
        Task<List<WebtoonInfo>> GetMostViewedWebtoonsAsync(int count);
        Task<bool> CreateWebtoonAsync(WebtoonInfo webtoon);
        
        // Bölüm/webtoon önbelleği yenileme için metodlar
        Task<bool> RefreshWebtoonCacheAsync(string webtoonFolderName);
        Task<bool> RefreshChapterCacheAsync(string webtoonFolderName, string chapterName);
        
        // Detayların dosyadan ya da klasörden çekilmesine yönelik alternatif metotlar
        Task<WebtoonInfo> GetWebtoonDetailsByFolderAsync(string webtoonFolder);
        Task<WebtoonInfo> GetWebtoonDetailsByFolderNameAsync(string webtoonFolder);
        Task<WebtoonInfo> GetWebtoonByDetailsFolderNameAsync(string webtoonFolder);
    }
}
