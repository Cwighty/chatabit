using System.Net.Http.Json;
using System.Text;
using Chat.Data;
using Chat.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Chat.IntegrationTests.ControllerTests
{
    public class ChatControllerTests
    {
        public class GetChatMessagesTest : IClassFixture<ChatApiWebApplicationFactory>
        {
            private HttpClient httpClient { get; }
            private readonly ChatApiWebApplicationFactory factory;

            public GetChatMessagesTest(ChatApiWebApplicationFactory factory)
            {
                this.factory = factory;
                httpClient = factory.CreateClient();
            }

            [Fact]
            public async Task GetChatMessagesTestAsync_ShouldContainAMessage()
            {
                var scope = factory.Services.GetService<IServiceScopeFactory>()?.CreateScope();
                if (scope == null)
                {
                    throw new InvalidOperationException("Service scope could not be created.");
                }

                var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                if (dbContext == null)
                {
                    throw new InvalidOperationException("Database context could not be obtained.");
                }

                var chatMessage = new ChatMessage
                {
                    MessageText = "Test message",
                    CreatedAt = DateTime.Now,
                    UserName = "Test user"
                };

                dbContext.ChatMessages.Add(chatMessage);
                await dbContext.SaveChangesAsync();

                var response = await httpClient.GetAsync("/api/Chat/");
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(responseString))
                {
                    throw new InvalidOperationException("Response content is empty or null.");
                }

                var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(responseString);
                if (messages == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the response content.");
                }

                Assert.Single(messages);
            }

            public class PostChatMessageTest : IClassFixture<ChatApiWebApplicationFactory>
            {
                private HttpClient httpClient { get; }
                private readonly ChatApiWebApplicationFactory factory;

                public PostChatMessageTest(ChatApiWebApplicationFactory factory)
                {
                    this.factory = factory;
                    httpClient = factory.CreateClient();
                }

                [Fact]
                public async Task PostChatMessageTestAsync_ShouldContainAMessage()
                {
                    var scope = factory.Services.GetService<IServiceScopeFactory>()?.CreateScope();
                    if (scope == null)
                    {
                        throw new InvalidOperationException("Service scope could not be created.");
                    }

                    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
                    if (dbContext == null)
                    {
                        throw new InvalidOperationException("Database context could not be obtained.");
                    }

                    var chatMessage = new ChatMessage
                    {
                        MessageText = "Test message",
                        CreatedAt = DateTime.Now,
                        UserName = "Test user"
                    };

                    var response = await httpClient.PostAsJsonAsync("/api/Chat/", chatMessage);
                    response.EnsureSuccessStatusCode();

                    var messages = await dbContext.ChatMessages.ToListAsync();
                    Assert.Single(messages);
                }
            }
        }
    }
}
