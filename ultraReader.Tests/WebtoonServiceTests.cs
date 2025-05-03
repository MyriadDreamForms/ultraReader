using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ultraReader.Models.Entities;
using ultraReader.Services;
using Xunit;

namespace ultraReader.Tests
{
    public class WebtoonServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnv;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<ICacheEntry> _mockCacheEntry;

        public WebtoonServiceTests()
        {
            _mockWebHostEnv = new Mock<IWebHostEnvironment>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockCacheEntry = new Mock<ICacheEntry>();

            _mockMemoryCache
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(_mockCacheEntry.Object);
        }

        [Fact]
        public async Task GetAllWebtoonsAsync_CacheHit_ReturnsFromCache()
        {
            // Arrange
            var cachedWebtoons = new List<WebtoonInfo>
            {
                new WebtoonInfo { Title = "Test Webtoon", Author = "Test Author" }
            };

            _mockMemoryCache
                .Setup(m => m.TryGetValue(It.IsAny<string>(), out cachedWebtoons))
                .Returns(true);

            var service = new WebtoonService(_mockWebHostEnv.Object, _mockMemoryCache.Object);

            // Act
            var result = await service.GetAllWebtoonsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Webtoon", result[0].Title);
            Assert.Equal("Test Author", result[0].Author);
        }

        [Fact]
        public async Task GetAllWebtoonsAsync_DirectoryNotExists_ReturnsEmptyList()
        {
            // Arrange
            var notFoundWebtoonsPath = "not_found_path";
            _mockWebHostEnv.Setup(m => m.WebRootPath).Returns(notFoundWebtoonsPath);

            var service = new WebtoonService(_mockWebHostEnv.Object, _mockMemoryCache.Object);

            // Act
            var result = await service.GetAllWebtoonsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWebtoonDetailsAsync_CacheHit_ReturnsFromCache()
        {
            // Arrange
            var webtoonName = "test-webtoon";
            var cachedDetails = new ultraReader.Models.DTOs.WebtoonDetailDto
            {
                WebtoonInfo = new WebtoonInfo { Title = "Test Webtoon", Author = "Test Author" },
                Chapters = new List<string> { "Chapter 1", "Chapter 2" }
            };

            _mockMemoryCache
                .Setup(m => m.TryGetValue(It.Is<string>(s => s.Contains(webtoonName)), out cachedDetails))
                .Returns(true);

            var service = new WebtoonService(_mockWebHostEnv.Object, _mockMemoryCache.Object);

            // Act
            var result = await service.GetWebtoonDetailsAsync(webtoonName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Webtoon", result.WebtoonInfo.Title);
            Assert.Equal(2, result.Chapters.Count);
        }

        [Fact]
        public async Task GetChapterImagesAsync_CacheHit_ReturnsFromCache()
        {
            // Arrange
            var webtoonName = "test-webtoon";
            var chapterName = "Chapter 1";
            var cachedViewModel = new ultraReader.Models.DTOs.ReaderViewModel
            {
                WebtoonInfo = new WebtoonInfo { Title = "Test Webtoon" },
                CurrentChapter = chapterName,
                Images = new List<string> { "01.jpg", "02.jpg" }
            };

            _mockMemoryCache
                .Setup(m => m.TryGetValue(It.Is<string>(s => s.Contains(webtoonName) && s.Contains(chapterName)), out cachedViewModel))
                .Returns(true);

            var service = new WebtoonService(_mockWebHostEnv.Object, _mockMemoryCache.Object);

            // Act
            var result = await service.GetChapterImagesAsync(webtoonName, chapterName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(chapterName, result.CurrentChapter);
            Assert.Equal(2, result.Images.Count);
        }
    }
} 