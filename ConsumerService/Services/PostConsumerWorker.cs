using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ConsumerService.Data;
using ConsumerService.Models;

namespace ConsumerService.Services
{
    public class PostConsumerWorker : BackgroundService
    {
        private readonly ILogger<PostConsumerWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PostConsumerWorker(ILogger<PostConsumerWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "post-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("sample-topic");

            _logger.LogInformation("Kafka consumer started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);

                    var post = JsonSerializer.Deserialize<Post>(result.Message.Value);

                    if (post != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<PostDbContext>();

                        db.Posts.Add(post);
                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Post saved: {Title}", post.title);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            }
        }
    }
}
