using ConsumerService.Data;
using ConsumerService.Models;
using ConsumerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConsumerService.UnitTest
{
    public class PostConsumerWorkerTests
    {
        private readonly Post _testPost;
        private readonly PostDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<ILogger<PostConsumerWorker>> _loggerMock;

        public PostConsumerWorkerTests()
        {
            // Setup test PostData
            _testPost = new Post
            {
                id = 1,
                userId = 10,
                title = "Test Title",
                body = "Test Body"
            };

            // Use InMemory DB
            var options = new DbContextOptionsBuilder<PostDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique for isolation
                .Options;

            _dbContext = new PostDbContext(options);

            // Setup mock logger
            _loggerMock = new Mock<ILogger<PostConsumerWorker>>();

            // Setup scoped service provider
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(
                new ServiceCollection()
                    .AddSingleton(_dbContext)
                    .BuildServiceProvider()
            );

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                               .Returns(serviceScopeFactoryMock.Object);

            _serviceProvider = serviceProviderMock.Object;
        }

        [Fact]
        public async Task SavePostData_AddsToDatabase()
        {
            // Act: simulate saving PostData
            using (var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scopedDb = scope.ServiceProvider.GetRequiredService<PostDbContext>();
                scopedDb.Posts.Add(_testPost);
                await scopedDb.SaveChangesAsync();
            }

            // Assert: ensure it's saved
            var saved = _dbContext.Posts.FirstOrDefault(p => p.id == _testPost.id);
            Assert.NotNull(saved);
            Assert.Equal(_testPost.title, saved.title);
        }
    }
}
