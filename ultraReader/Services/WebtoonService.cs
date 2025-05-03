using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ultraReader.Data;
using ultraReader.Models.DTOs;
using ultraReader.Models.Entities;

namespace ultraReader.Services
{
    public class WebtoonService : IWebtoonService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDistributedCache _distributedCache;
        private readonly string _webtoonsPath;
        private readonly ILogger<WebtoonService> _logger;
        private const string CacheKeyAllWebtoons = "AllWebtoons";
        private const string CacheKeyWebtoonPrefix = "Webtoon_";
        private const string CacheKeyChapterPrefix = "Chapter_";
        private static readonly TimeSpan DefaultCacheTime = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan LongCacheTime = TimeSpan.FromHours(6);
        private static readonly TimeSpan VeryShortCacheTime = TimeSpan.FromSeconds(30);
        private static readonly HashSet<string> _supportedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };
        private readonly ApplicationDbContext _context;
        private static readonly int MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1);
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public WebtoonService(IWebHostEnvironment webHostEnvironment, IDistributedCache distributedCache, ApplicationDbContext context, ILogger<WebtoonService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _distributedCache = distributedCache;
            _webtoonsPath = Path.Combine(_webHostEnvironment.WebRootPath, "webtoons");
            _context = context;
            _logger = logger;
        }

        public async Task<List<WebtoonInfo>> GetAllWebtoonsAsync()
        {
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(CacheKeyAllWebtoons);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<List<WebtoonInfo>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for AllWebtoons.");
            }

            List<WebtoonInfo> webtoons = new List<WebtoonInfo>();

            if (!Directory.Exists(_webtoonsPath))
            {
                _logger.LogWarning("Webtoons path not found: {WebtoonsPath}", _webtoonsPath);
                return webtoons;
            }

            string[] webtoonDirectories;
            try
            {
                webtoonDirectories = Directory.GetDirectories(_webtoonsPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webtoon directories from path: {WebtoonsPath}", _webtoonsPath);
                return webtoons;
            }

            var concurrentWebtoons = new ConcurrentBag<WebtoonInfo>();

            var processDirectoryBlock = new TransformBlock<string, WebtoonInfo>(
                async (webtoonDirectory) =>
                {
                    string infoFilePath = Path.Combine(webtoonDirectory, "info.json");
                    if (File.Exists(infoFilePath))
                    {
                        try
                        {
                            string json = await File.ReadAllTextAsync(infoFilePath);
                            var webtoonInfo = JsonSerializer.Deserialize<WebtoonInfo>(json, _jsonOptions);

                            if (webtoonInfo != null)
                            {
                                webtoonInfo.FolderName = new DirectoryInfo(webtoonDirectory).Name;
                                return webtoonInfo;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error reading or deserializing info.json from {Directory}", webtoonDirectory);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("info.json not found in {Directory}", webtoonDirectory);
                    }
                    return null;
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                }
            );

            var actionBlock = new ActionBlock<WebtoonInfo>(
                webtoonInfo =>
                {
                    if (webtoonInfo != null)
                    {
                        concurrentWebtoons.Add(webtoonInfo);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism
                }
            );

            processDirectoryBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (string directory in webtoonDirectories)
            {
                await processDirectoryBlock.SendAsync(directory);
            }

            processDirectoryBlock.Complete();
            await actionBlock.Completion;

            webtoons = concurrentWebtoons.ToList();

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(LongCacheTime);
                await _distributedCache.SetStringAsync(CacheKeyAllWebtoons, JsonSerializer.Serialize(webtoons), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for AllWebtoons.");
            }

            return webtoons;
        }

        public async Task<WebtoonDetailDto> GetWebtoonDetailsAsync(string webtoonName)
        {
            string cacheKey = $"{CacheKeyWebtoonPrefix}{webtoonName}";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<WebtoonDetailDto>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for WebtoonDetails with key: {CacheKey}", cacheKey);
            }

            string webtoonPath = Path.Combine(_webtoonsPath, webtoonName);
            if (!Directory.Exists(webtoonPath))
            {
                _logger.LogWarning("Directory not found for webtoon: {WebtoonPath}", webtoonPath);
                return null;
            }

            string infoFilePath = Path.Combine(webtoonPath, "info.json");
            if (!File.Exists(infoFilePath))
            {
                _logger.LogWarning("info.json not found for webtoon: {WebtoonName}", webtoonName);
                return null;
            }

            try
            {
                string json = await File.ReadAllTextAsync(infoFilePath);
                var webtoonInfo = JsonSerializer.Deserialize<WebtoonInfo>(json, _jsonOptions);

                if (webtoonInfo == null)
                {
                    _logger.LogWarning("Deserialized webtoonInfo is null for {WebtoonName}", webtoonName);
                    return null;
                }

                webtoonInfo.FolderName = webtoonName;

                var chapters = Directory.GetDirectories(webtoonPath)
                    .Select(d => new DirectoryInfo(d).Name)
                    .Where(name => !name.Equals("cover", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(name => name, new NaturalStringComparer())
                    .ToList();

                var detailDto = new WebtoonDetailDto
                {
                    WebtoonInfo = webtoonInfo,
                    Chapters = chapters
                };

                try
                {
                    var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DefaultCacheTime);
                    await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(detailDto), cacheOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting cache for WebtoonDetails with key: {CacheKey}", cacheKey);
                }
                return detailDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading or deserializing info.json for webtoon: {WebtoonName}", webtoonName);
                return null;
            }
        }

        public async Task<ReaderViewModel> GetChapterImagesAsync(string webtoonName, string chapterName)
        {
            string cacheKey = $"{CacheKeyChapterPrefix}{webtoonName}_{chapterName}";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<ReaderViewModel>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for ChapterImages with key: {CacheKey}", cacheKey);
            }

            string chapterPath = Path.Combine(_webtoonsPath, webtoonName, chapterName);
            if (!Directory.Exists(chapterPath))
            {
                _logger.LogWarning("Chapter directory not found: {ChapterPath}", chapterPath);
                return null;
            }

            var webtoonDetailDto = await GetWebtoonDetailsAsync(webtoonName);
            if (webtoonDetailDto == null)
            {
                _logger.LogWarning("Webtoon details not found for {WebtoonName}", webtoonName);
                return null;
            }

            List<string> imageFiles;
            try
            {
                var allFiles = Directory.GetFiles(chapterPath);
                
                // Paralelleştirme için Parallel.ForEach kullanarak performansı artır
                var concurrentImageFiles = new ConcurrentBag<string>();
                Parallel.ForEach(allFiles, file => 
                {
                    if (IsImageFile(file))
                    {
                        concurrentImageFiles.Add(Path.GetFileName(file));
                    }
                });
                
                // Natural string comparer ile sırala
                imageFiles = concurrentImageFiles.OrderBy(f => f, new NaturalStringComparer()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image files for chapter: {ChapterName} of webtoon: {WebtoonName}", chapterName, webtoonName);
                return null;
            }

            int currentIndex = webtoonDetailDto.Chapters.IndexOf(chapterName);
            string previousChapter = currentIndex > 0 ? webtoonDetailDto.Chapters[currentIndex - 1] : null;
            string nextChapter = currentIndex < webtoonDetailDto.Chapters.Count - 1 ? webtoonDetailDto.Chapters[currentIndex + 1] : null;

            var viewModel = new ReaderViewModel
            {
                WebtoonInfo = webtoonDetailDto.WebtoonInfo,
                CurrentChapter = chapterName,
                Images = imageFiles,
                PreviousChapter = previousChapter,
                NextChapter = nextChapter
            };

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DefaultCacheTime);
                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(viewModel), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for ChapterImages with key: {CacheKey}", cacheKey);
            }

            return viewModel;
        }

        public async Task<WebtoonInfo> GetWebtoonByIdAsync(string id)
        {
            var webtoons = await GetAllWebtoonsAsync();
            return webtoons.FirstOrDefault(w => w.Id == id);
        }

        public async Task<bool> UpdateWebtoonAsync(WebtoonInfo webtoon)
        {
            if (webtoon == null)
            {
                _logger.LogWarning("Null webtoon passed to UpdateWebtoonAsync.");
                return false;
            }

            string webtoonPath = Path.Combine(_webtoonsPath, webtoon.FolderName);
            if (!Directory.Exists(webtoonPath))
            {
                _logger.LogWarning("Directory not found for webtoon {FolderName}", webtoon.FolderName);
                return false;
            }

            string infoFilePath = Path.Combine(webtoonPath, "info.json");
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(webtoon, options);
                await File.WriteAllTextAsync(infoFilePath, json);

                await _distributedCache.RemoveAsync(CacheKeyAllWebtoons);
                await _distributedCache.RemoveAsync($"{CacheKeyWebtoonPrefix}{webtoon.FolderName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating webtoon {WebtoonId}", webtoon.Id);
                return false;
            }
        }

        public async Task<bool> AddWebtoonAsync(WebtoonInfo webtoon)
        {
            if (webtoon == null || string.IsNullOrEmpty(webtoon.FolderName))
            {
                _logger.LogWarning("Invalid webtoon passed to AddWebtoonAsync.");
                return false;
            }

            string webtoonPath = Path.Combine(_webtoonsPath, webtoon.FolderName);
            try
            {
                if (!Directory.Exists(webtoonPath))
                {
                    Directory.CreateDirectory(webtoonPath);
                }

                string infoFilePath = Path.Combine(webtoonPath, "info.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(webtoon, options);
                await File.WriteAllTextAsync(infoFilePath, json);

                await _distributedCache.RemoveAsync(CacheKeyAllWebtoons);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding webtoon {WebtoonId}", webtoon.Id);
                return false;
            }
        }

        public async Task<bool> DeleteWebtoonAsync(string id)
        {
            var webtoon = await GetWebtoonByIdAsync(id);
            if (webtoon == null)
            {
                _logger.LogWarning("Webtoon with id {WebtoonId} not found for deletion.", id);
                return false;
            }

            string webtoonPath = Path.Combine(_webtoonsPath, webtoon.FolderName);
            if (!Directory.Exists(webtoonPath))
            {
                _logger.LogWarning("Directory not found for deletion of webtoon {FolderName}", webtoon.FolderName);
                return false;
            }

            try
            {
                Directory.Delete(webtoonPath, true);
                await _distributedCache.RemoveAsync(CacheKeyAllWebtoons);
                await _distributedCache.RemoveAsync($"{CacheKeyWebtoonPrefix}{webtoon.FolderName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting webtoon {WebtoonId}", id);
                return false;
            }
        }

        public async Task<WebtoonInfo> GetWebtoonByFolderNameAsync(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                return null;

            // Önce dosya sisteminden kontrol et
            string webtoonPath = Path.Combine(_webtoonsPath, folderName);
            if (!Directory.Exists(webtoonPath))
                return null;

            string infoFilePath = Path.Combine(webtoonPath, "info.json");
            if (!File.Exists(infoFilePath))
                return null;

            try
            {
                string json = await File.ReadAllTextAsync(infoFilePath);
                var webtoonInfo = JsonSerializer.Deserialize<WebtoonInfo>(json, _jsonOptions);
                if (webtoonInfo != null)
                {
                    webtoonInfo.FolderName = folderName;
                    return webtoonInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading info.json from folder {FolderName}", folderName);
            }

            // Yedek olarak veritabanından kontrol et
            try
            {
                return await _context.Webtoons.FirstOrDefaultAsync(w => w.FolderName == folderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying database for folder name {FolderName}", folderName);
                return null;
            }
        }

        private bool IsImageFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return _supportedImageExtensions.Contains(extension);
        }

        public async Task<int> GetTotalWebtoonCountAsync()
        {
            string cacheKey = "TotalWebtoonCount";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return int.Parse(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for TotalWebtoonCount.");
            }

            int count = 0;
            try
            {
                count = await _context.Webtoons.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching webtoon count from database.");
            }

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                await _distributedCache.SetStringAsync(cacheKey, count.ToString(), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for TotalWebtoonCount.");
            }
            return count;
        }

        public async Task<List<WebtoonInfo>> GetLatestWebtoonsAsync(int count)
        {
            string cacheKey = $"LatestWebtoons_{count}";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<List<WebtoonInfo>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for LatestWebtoons with key: {CacheKey}", cacheKey);
            }

            List<WebtoonInfo> latestWebtoons = new List<WebtoonInfo>();
            try
            {
                latestWebtoons = await _context.Webtoons
                    .OrderByDescending(w => w.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest webtoons from database.");
            }

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(latestWebtoons), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for LatestWebtoons with key: {CacheKey}", cacheKey);
            }
            return latestWebtoons;
        }

        public async Task<List<string>> GetAllGenresAsync()
        {
            string cacheKey = "AllGenres";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<List<string>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for AllGenres.");
            }

            var webtoons = await GetAllWebtoonsAsync();
            var genres = webtoons
                .Where(w => w.Genres != null)
                .SelectMany(w => w.Genres)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(genres), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for AllGenres.");
            }
            return genres;
        }

        public async Task<List<string>> GetAllStatusesAsync()
        {
            string cacheKey = "AllStatuses";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<List<string>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for AllStatuses.");
            }

            var webtoons = await GetAllWebtoonsAsync();
            var statuses = webtoons
                .Where(w => !string.IsNullOrEmpty(w.Status))
                .Select(w => w.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(statuses), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for AllStatuses.");
            }
            return statuses;
        }

        public async Task<Dictionary<string, int>> GetWebtoonsByGenreAsync()
        {
            string cacheKey = "WebtoonsByGenre";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, int>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for WebtoonsByGenre.");
            }

            var webtoons = await GetAllWebtoonsAsync();
            var webtoonsByGenre = new Dictionary<string, int>();

            foreach (var webtoon in webtoons.Where(w => w.Genres != null))
            {
                foreach (var genre in webtoon.Genres)
                {
                    if (webtoonsByGenre.ContainsKey(genre))
                    {
                        webtoonsByGenre[genre]++;
                    }
                    else
                    {
                        webtoonsByGenre[genre] = 1;
                    }
                }
            }

            var topGenres = webtoonsByGenre
                .OrderByDescending(g => g.Value)
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Value);

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(topGenres), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for WebtoonsByGenre.");
            }
            return topGenres;
        }

        public async Task<Dictionary<string, int>> GetWebtoonsByStatusAsync()
        {
            string cacheKey = "WebtoonsByStatus";
            try
            {
                string cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, int>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading cache for WebtoonsByStatus.");
            }

            var webtoons = await GetAllWebtoonsAsync();
            var webtoonsByStatus = webtoons
                .Where(w => !string.IsNullOrEmpty(w.Status))
                .GroupBy(w => w.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
                await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(webtoonsByStatus), cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for WebtoonsByStatus.");
            }
            return webtoonsByStatus;
        }

        public Task<bool> CreateWebtoonAsync(WebtoonInfo webtoon)
        {
            return AddWebtoonAsync(webtoon);
        }

        public async Task<bool> AddChapterAsync(Models.Entities.ChapterInfo chapter)
        {
            try
            {
                if (chapter == null || string.IsNullOrEmpty(chapter.WebtoonId) || string.IsNullOrEmpty(chapter.ChapterName))
                {
                    _logger.LogWarning("Geçersiz chapter parametresi ile AddChapterAsync çağrıldı");
                    return false;
                }

                var webtoon = await GetWebtoonByIdAsync(chapter.WebtoonId);
                if (webtoon == null)
                {
                    _logger.LogWarning("Belirtilen webtoon bulunamadı: {WebtoonId}", chapter.WebtoonId);
                    return false;
                }

                string chapterPath = Path.Combine(_webtoonsPath, webtoon.FolderName, chapter.ChapterName);
                
                // Bölüm klasörü yoksa oluştur
                if (!Directory.Exists(chapterPath))
                {
                    Directory.CreateDirectory(chapterPath);
                }

                // Önbellekteki webtoon bilgilerini temizle
                string cacheKey = $"{CacheKeyWebtoonPrefix}{webtoon.FolderName}";
                await _distributedCache.RemoveAsync(cacheKey);
                await _distributedCache.RemoveAsync(CacheKeyAllWebtoons);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bölüm eklenirken hata oluştu: {ChapterName}", chapter.ChapterName);
                return false;
            }
        }

        Task<WebtoonDetailDto> IWebtoonService.GetWebtoonDetailsAsync(string webtoonName)
        {
            return GetWebtoonDetailsAsync(webtoonName);
        }

        public async Task<WebtoonInfo> GetWebtoonDetailsByFolderAsync(string webtoonFolder)
        {
            if (string.IsNullOrEmpty(webtoonFolder))
            {
                _logger.LogWarning("Geçersiz webtoonFolder parametresi ile GetWebtoonDetailsByFolderAsync çağrıldı");
                return null;
            }

            return await GetWebtoonByFolderNameAsync(webtoonFolder);
        }

        public async Task<WebtoonInfo> GetWebtoonDetailsByFolderNameAsync(string webtoonFolder)
        {
            try
            {
                if (string.IsNullOrEmpty(webtoonFolder))
                {
                    _logger.LogWarning("Geçersiz webtoonFolder parametresi ile GetWebtoonDetailsByFolderNameAsync çağrıldı");
                    return null;
                }

                // Webtoon bilgisini getir
                return await GetWebtoonByFolderNameAsync(webtoonFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webtoon details for folder {WebtoonFolder}", webtoonFolder);
                return null;
            }
        }

        public async Task<WebtoonInfo> GetWebtoonByDetailsFolderNameAsync(string webtoonFolder)
        {
            try
            {
                string cacheKey = $"{CacheKeyWebtoonPrefix}{webtoonFolder}";
                
                try
                {
                    var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                    if (!string.IsNullOrEmpty(cachedData))
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<WebtoonInfo>(cachedData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading cache for webtoon {WebtoonFolder}", webtoonFolder);
                }
                
                // Cache'te yoksa disk'ten oku
                return await GetWebtoonByFolderNameAsync(webtoonFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webtoon details by folder name: {WebtoonFolder}", webtoonFolder);
                return null;
            }
        }
        
        public async Task<List<Models.Entities.ChapterInfo>> GetChaptersAsync(string webtoonFolderName)
        {
            try
            {
                var webtoon = await GetWebtoonByFolderNameAsync(webtoonFolderName);
                if (webtoon == null)
                {
                    return new List<Models.Entities.ChapterInfo>();
                }
                
                string webtoonPath = Path.Combine(_webtoonsPath, webtoonFolderName);
                if (!Directory.Exists(webtoonPath))
                {
                    return new List<Models.Entities.ChapterInfo>();
                }
                
                var chapterDirectories = Directory.GetDirectories(webtoonPath)
                    .Where(dir => !Path.GetFileName(dir).StartsWith("."))
                    .ToList();
                
                List<Models.Entities.ChapterInfo> chapters = new List<Models.Entities.ChapterInfo>();
                
                foreach (var chapterDir in chapterDirectories)
                {
                    string chapterName = Path.GetFileName(chapterDir);
                    if (chapterName.Equals("cover", StringComparison.OrdinalIgnoreCase))
                    {
                        continue; // Kapak klasörünü atla
                    }
                    
                    // Bölüm bilgilerini (varsa) bir json dosyasından oku
                    var chapterInfoFile = Path.Combine(chapterDir, "chapter-info.json");
                    Models.Entities.ChapterInfo chapter = new Models.Entities.ChapterInfo
                    {
                        WebtoonId = webtoon.Id,
                        ChapterName = chapterName,
                        SafeChapterName = chapterName,
                        UploadedAt = Directory.GetCreationTime(chapterDir),
                        PublishedAt = Directory.GetCreationTime(chapterDir),
                        IsPublished = true,
                        Views = new Random().Next(10, 1000) // Örnek amaçlı
                    };
                    
                    if (File.Exists(chapterInfoFile))
                    {
                        try
                        {
                            string json = await File.ReadAllTextAsync(chapterInfoFile);
                            var info = System.Text.Json.JsonSerializer.Deserialize<Models.Entities.ChapterInfo>(json);
                            if (info != null)
                            {
                                chapter.ChapterNumber = info.ChapterNumber;
                                chapter.ChapterTitle = info.ChapterTitle;
                                chapter.ChapterDescription = info.ChapterDescription;
                                chapter.IsPublished = info.IsPublished;
                                chapter.Views = info.Views;
                                chapter.PublishedAt = info.PublishedAt;
                                chapter.Order = info.Order;
                                chapter.Title = info.Title ?? info.ChapterTitle;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error reading chapter info for {ChapterName}", chapterName);
                            
                            // Varsayılan değerler
                            int chapterNum = 0;
                            if (int.TryParse(new string(chapterName.Where(char.IsDigit).ToArray()), out chapterNum))
                            {
                                chapter.ChapterNumber = chapterNum;
                            }
                            chapter.Title = $"Bölüm {chapter.ChapterNumber}";
                        }
                    }
                    else
                    {
                        // Bölüm adından numara çıkarma
                        int chapterNum = 0;
                        if (int.TryParse(new string(chapterName.Where(char.IsDigit).ToArray()), out chapterNum))
                        {
                            chapter.ChapterNumber = chapterNum;
                        }
                        chapter.Title = $"Bölüm {chapter.ChapterNumber}";
                    }
                    
                    chapters.Add(chapter);
                }
                
                // Bölümleri sırala
                return chapters.OrderBy(c => c.ChapterNumber).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chapters for webtoon {WebtoonName}", webtoonFolderName);
                return new List<Models.Entities.ChapterInfo>();
            }
        }

        /// <summary>
        /// Belirtilen webtoon için önbelleği yeniler
        /// </summary>
        /// <param name="webtoonFolderName">Webtoon klasör adı</param>
        /// <returns>İşlemin başarı durumu</returns>
        public async Task<bool> RefreshWebtoonCacheAsync(string webtoonFolderName)
        {
            try
            {
                if (string.IsNullOrEmpty(webtoonFolderName))
                {
                    _logger.LogWarning("Null veya boş webtoonFolderName ile RefreshWebtoonCacheAsync çağrıldı.");
                    return false;
                }

                string webtoonPath = Path.Combine(_webtoonsPath, webtoonFolderName);
                if (!Directory.Exists(webtoonPath))
                {
                    _logger.LogWarning("Belirtilen webtoon dizini bulunamadı: {WebtoonPath}", webtoonPath);
                    return false;
                }

                string infoFilePath = Path.Combine(webtoonPath, "info.json");
                if (!File.Exists(infoFilePath))
                {
                    _logger.LogWarning("info.json dosyası bulunamadı: {InfoFilePath}", infoFilePath);
                    return false;
                }

                // Webtoon önbelleğini temizle
                string cacheKey = $"{CacheKeyWebtoonPrefix}{webtoonFolderName}";
                await _distributedCache.RemoveAsync(cacheKey);
                
                // Tüm webtoon listesi önbelleğini de temizle
                await _distributedCache.RemoveAsync(CacheKeyAllWebtoons);
                
                // Webtoon detaylarını yeniden yükle
                await GetWebtoonDetailsAsync(webtoonFolderName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webtoon önbelleğini yenilerken hata oluştu: {WebtoonFolderName}", webtoonFolderName);
                return false;
            }
        }
        
        /// <summary>
        /// Belirtilen bölüm için önbelleği yeniler
        /// </summary>
        /// <param name="webtoonFolderName">Webtoon klasör adı</param>
        /// <param name="chapterName">Bölüm adı</param>
        /// <returns>İşlemin başarı durumu</returns>
        public async Task<bool> RefreshChapterCacheAsync(string webtoonFolderName, string chapterName)
        {
            try
            {
                if (string.IsNullOrEmpty(webtoonFolderName) || string.IsNullOrEmpty(chapterName))
                {
                    _logger.LogWarning("Null veya boş parametrelerle RefreshChapterCacheAsync çağrıldı: WebtoobFolderName={WebtoobFolderName}, ChapterName={ChapterName}", webtoonFolderName, chapterName);
                    return false;
                }

                string chapterPath = Path.Combine(_webtoonsPath, webtoonFolderName, chapterName);
                if (!Directory.Exists(chapterPath))
                {
                    _logger.LogWarning("Belirtilen bölüm dizini bulunamadı: {ChapterPath}", chapterPath);
                    return false;
                }

                // Bölüm önbelleğini temizle
                string cacheKey = $"{CacheKeyChapterPrefix}{webtoonFolderName}_{chapterName}";
                await _distributedCache.RemoveAsync(cacheKey);
                
                // Bölüm bilgilerini yeniden yükle
                await GetChapterImagesAsync(webtoonFolderName, chapterName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bölüm önbelleğini yenilerken hata oluştu: {WebtoonFolderName}/{ChapterName}", webtoonFolderName, chapterName);
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetViewsPerDayAsync(int days = 7)
        {
            var result = new Dictionary<string, int>();
            var today = DateTime.Today;
            
            try
            {
                // Son x gün için tarih oluştur
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    var dateString = date.ToString("yyyy-MM-dd");
                    
                    // PageView tablosunu kullanarak görüntülenme sayısını al
                    int viewsCount = 0;
                    
                    try 
                    {
                        viewsCount = await _context.PageViews
                            .Where(v => v.ViewedAt.Date == date.Date)
                            .CountAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PageViews tablosundan görüntülenme sayısı alınırken hata: {Date}", dateString);
                        
                        // Fallback: Random değerler
                        Random random = new Random();
                        viewsCount = random.Next(50, 500);
                        
                        _logger.LogInformation("Görüntülenme sayısı için {Date} tarihinde örnek değer kullanıldı: {Count}", dateString, viewsCount);
                    }
                    
                    result.Add(dateString, viewsCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görüntülenme istatistikleri alınırken hata oluştu.");
                
                // Hata durumunda varsayılan değerlerle doldur
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    result.Add(date.ToString("yyyy-MM-dd"), 0);
                }
            }
            
            return result;
        }

        public async Task<Dictionary<string, int>> GetWebtoonCategoriesAsync()
        {
            try
            {
                var genres = new Dictionary<string, int>();
                var webtoons = await GetAllWebtoonsAsync();
                
                foreach (var webtoon in webtoons)
                {
                    if (webtoon.Genres != null && webtoon.Genres.Count > 0)
                    {
                        foreach (var genre in webtoon.Genres)
                        {
                            if (!string.IsNullOrEmpty(genre))
                            {
                                string genreTrimmed = genre.Trim();
                                if (genres.ContainsKey(genreTrimmed))
                                    genres[genreTrimmed]++;
                                else
                                    genres[genreTrimmed] = 1;
                            }
                        }
                    }
                }
                
                return genres;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webtoon categories");
                return new Dictionary<string, int>();
            }
        }

        public async Task<int> GetTotalChapterCountAsync()
        {
            try
            {
                int totalChapters = 0;
                var webtoons = await GetAllWebtoonsAsync();
                
                foreach (var webtoon in webtoons)
                {
                    var chapters = await GetChaptersAsync(webtoon.FolderName);
                    totalChapters += chapters.Count;
                }
                
                return totalChapters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total chapter count");
                return 0;
            }
        }

        public async Task<Dictionary<string, object>> GetWebtoonStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, object>();
                
                // Toplam webtoon sayısı
                stats["totalWebtoons"] = await GetTotalWebtoonCountAsync();
                
                // Toplam bölüm sayısı
                stats["totalChapters"] = await GetTotalChapterCountAsync();
                
                // Kategorilere göre webtoon dağılımı
                stats["categories"] = await GetWebtoonCategoriesAsync();
                
                // Son bir haftadaki görüntülenme sayıları
                stats["viewsPerDay"] = await GetViewsPerDayAsync(7);
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webtoon statistics");
                return new Dictionary<string, object>();
            }
        }

        public async Task<List<WebtoonInfo>> GetMostViewedWebtoonsAsync(int count)
        {
            try
            {
                var webtoons = await GetAllWebtoonsAsync();
                
                // Görüntülenme sayısı için random değerler oluşturalım
                // Gerçek uygulamada veritabanından alınması gerekir
                var random = new Random();
                var webtoonViews = new Dictionary<string, int>();
                
                foreach (var webtoon in webtoons)
                {
                    // Örnek veri olarak 100-5000 arası rastgele değerler
                    webtoonViews[webtoon.Id] = random.Next(100, 5000);
                }
                
                // Görüntülenme sayısına göre sırala ve istenilen sayıda dön
                return webtoons
                    .OrderByDescending(w => webtoonViews.GetValueOrDefault(w.Id, 0))
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most viewed webtoons");
                return new List<WebtoonInfo>();
            }
        }
    }

    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lenX = x.Length;
            int lenY = y.Length;
            int posX = 0;
            int posY = 0;

            while (posX < lenX && posY < lenY)
            {
                char charX = x[posX];
                char charY = y[posY];

                if (char.IsDigit(charX) && char.IsDigit(charY))
                {
                    int numStartX = posX;
                    while (posX < lenX && char.IsDigit(x[posX])) posX++;
                    int numX = int.Parse(x.Substring(numStartX, posX - numStartX));

                    int numStartY = posY;
                    while (posY < lenY && char.IsDigit(y[posY])) posY++;
                    int numY = int.Parse(y.Substring(numStartY, posY - numStartY));

                    if (numX != numY)
                    {
                        return numX.CompareTo(numY);
                    }
                }
                else
                {
                    int result = charX.CompareTo(charY);
                    if (result != 0)
                    {
                        return result;
                    }
                    posX++;
                    posY++;
                }
            }

            return lenX.CompareTo(lenY);
        }
    }
}
