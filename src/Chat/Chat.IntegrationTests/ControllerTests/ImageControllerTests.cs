using System.Net.Http.Json;
using Chat.Data;
using Chat.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

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
        public class GetChatMessageImagesTest : ImageControllerTests
        {
            public GetChatMessageImagesTest(ImageProcessingWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task GetChatMessagesImagesTestAsync_ShouldContainAnImage()
            {
                var chatMessage = new ChatMessage
                {
                    Id = 1,
                    MessageText = "test",
                    CreatedAt = DateTime.Now,
                    UserName = "testUser",
                };

                DbContext.ChatMessages.Add(chatMessage);
                await DbContext.SaveChangesAsync();

                var chatMessageImage = new ChatMessageImage
                {
                    ChatMessageId = 1,
                    ImageData = "randomData",
                    FileName = "testImage",
                };

                DbContext.ChatMessageImages.Add(chatMessageImage);
                await DbContext.SaveChangesAsync();

                var response = await HttpClient.GetFromJsonAsync<List<ChatMessageImage>>("/api/Image");

                Assert.NotNull(response);
                Assert.True(response.Count > 0);
                Assert.Equal(chatMessageImage.ChatMessageId, response[0].ChatMessageId);
                Assert.Equal(chatMessageImage.ImageData, response[0].ImageData);
            }
        }

        public class PostChatMessageImagesTest : ImageControllerTests
        {
            public PostChatMessageImagesTest(ImageProcessingWebApplicationFactory factory) : base(factory)
            {
            }

            [Fact]
            public async Task PostChatMessagesImagesTestAsync_ShouldContainAnImage()
            {
                var chatMessage = new ChatMessage
                {
                    Id = 1,
                    MessageText = "test",
                    CreatedAt = DateTime.Now,
                    UserName = "testUser",
                };

                DbContext.ChatMessages.Add(chatMessage);
                await DbContext.SaveChangesAsync();

                var images = new List<string>
                {
                    TestImage.ImageData
                };
                var response = await HttpClient.PostAsJsonAsync($"/api/Image/{chatMessage.Id}", images);
                response.EnsureSuccessStatusCode();


                var dbImages = DbContext.ChatMessageImages.ToList();

                dbImages.ForEach(image => Assert.Equal(chatMessage.Id, image.ChatMessageId));
                dbImages.ForEach(image => Assert.Contains(image.ImageData, images));
            }
        }
    }
}
