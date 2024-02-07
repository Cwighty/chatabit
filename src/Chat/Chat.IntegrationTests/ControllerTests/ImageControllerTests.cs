using System.Net;
using System.Net.Http.Json;
using Chat.Data;
using Chat.Data.Entities;
using Chat.Features.Chat;
using Chat.ImageProcessing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Chat.IntegrationTests.ControllerTests
{
    public class ImageControllerTests : IClassFixture<ImageProcessingWebApplicationFactory>
    {
        public HttpClient HttpClient { get; }
        public ChatDbContext DbContext { get; }

        public ImageProcessingWebApplicationFactory Factory { get; }

        public ImageControllerTests(ImageProcessingWebApplicationFactory factory)
        {
            Factory = factory;
            HttpClient = factory.CreateClient(); var scope = Factory.Services.GetService<IServiceScopeFactory>()?.CreateScope();
            if (scope == null)
            {
                throw new InvalidOperationException("Service scope could not be created.");
            }

            var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
            if (dbContext == null)
            {
                throw new InvalidOperationException("Database context could not be obtained.");
            }
            DbContext = dbContext;
        }
        public class GetImageFileTest : ImageControllerTests
        {
            public GetImageFileTest(ImageProcessingWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task GetImageFile_NotFoundInCacheAndDisk_ReturnsNotFound()
            {
                var imageId = Guid.NewGuid();
                
                var response = await HttpClient.GetAsync($"/api/Image/file/{imageId}");

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        public class UploadImageForMessageTest : ImageControllerTests
        {
            public UploadImageForMessageTest(ImageProcessingWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task UploadImageForMessage_ImagesUploadedSuccessfully_ReturnsOk()
            {
                var uploadRequest = new List<UploadChatImageRequest>
                {
                    new UploadChatImageRequest
                    {
                        Id = Guid.NewGuid(),
                        ImageData = TestImage.ImageData 
                    }
                };

                var response = await HttpClient.PostAsJsonAsync("/api/Image", uploadRequest);

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
