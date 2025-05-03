using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ultraReader.Controllers;
using ultraReader.Models.DTOs;
using ultraReader.Models.Entities;
using ultraReader.Services;
using Xunit;

namespace ultraReader.Tests
{
    public class ControllerTests
    {
        private readonly Mock<IWebtoonService> _mockWebtoonService;

        public ControllerTests()
        {
            _mockWebtoonService = new Mock<IWebtoonService>();
        }

        [Fact]
        public async Task WebtoonList_Index_ReturnsViewWithWebtoons()
        {
            // Arrange
            var webtoons = new List<WebtoonInfo>
            {
                new WebtoonInfo { Title = "Test Webtoon 1", Author = "Author 1" },
                new WebtoonInfo { Title = "Test Webtoon 2", Author = "Author 2" }
            };

            _mockWebtoonService.Setup(s => s.GetAllWebtoonsAsync()).ReturnsAsync(webtoons);

            var controller = new WebtoonListController(_mockWebtoonService.Object);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<WebtoonInfo>>(result.Model);
            var model = result.Model as IEnumerable<WebtoonInfo>;
            Assert.Equal(2, ((List<WebtoonInfo>)model).Count);
        }

        [Fact]
        public async Task Webtoon_Details_ReturnsViewWithWebtoonDetails()
        {
            // Arrange
            var webtoonName = "test-webtoon";
            var webtoonDetails = new WebtoonDetailDto
            {
                WebtoonInfo = new WebtoonInfo
                {
                    Title = "Test Webtoon",
                    Author = "Test Author",
                    FolderName = webtoonName
                },
                Chapters = new List<string> { "Chapter 1", "Chapter 2" }
            };

            _mockWebtoonService.Setup(s => s.GetWebtoonDetailsAsync(webtoonName)).ReturnsAsync(webtoonDetails);

            var controller = new WebtoonController(_mockWebtoonService.Object);

            // Act
            var result = await controller.Details(webtoonName) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebtoonDetailDto>(result.Model);
            var model = result.Model as WebtoonDetailDto;
            Assert.Equal("Test Webtoon", model.WebtoonInfo.Title);
            Assert.Equal(2, model.Chapters.Count);
        }

        [Fact]
        public async Task Reader_Read_ReturnsViewWithReaderViewModel()
        {
            // Arrange
            var webtoonName = "test-webtoon";
            var chapterName = "Chapter 1";
            var readerViewModel = new ReaderViewModel
            {
                WebtoonInfo = new WebtoonInfo
                {
                    Title = "Test Webtoon",
                    FolderName = webtoonName
                },
                CurrentChapter = chapterName,
                Images = new List<string> { "01.jpg", "02.jpg" }
            };

            _mockWebtoonService.Setup(s => s.GetChapterImagesAsync(webtoonName, chapterName)).ReturnsAsync(readerViewModel);

            var controller = new ReaderController(_mockWebtoonService.Object);

            // Act
            var result = await controller.Read(webtoonName, chapterName) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ReaderViewModel>(result.Model);
            var model = result.Model as ReaderViewModel;
            Assert.Equal(chapterName, model.CurrentChapter);
            Assert.Equal(2, model.Images.Count);
        }

        [Fact]
        public async Task Webtoon_Details_WithEmptyName_RedirectsToWebtoonList()
        {
            // Arrange
            var controller = new WebtoonController(_mockWebtoonService.Object);

            // Act
            var result = await controller.Details("") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("WebtoonList", result.ControllerName);
        }

        [Fact]
        public async Task Reader_Read_WithEmptyName_RedirectsToWebtoonList()
        {
            // Arrange
            var controller = new ReaderController(_mockWebtoonService.Object);

            // Act
            var result = await controller.Read("", "Chapter 1") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("WebtoonList", result.ControllerName);
        }
    }
} 