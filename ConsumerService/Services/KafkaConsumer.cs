using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using ConsumerService.Data;
using ConsumerService.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsumerService.Services
{
    public class KafkaConsumer : IKafkaConsumer
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "sample-topic";
        private readonly DbContextOptions<PostDbContext> _dbOptions;

        public KafkaConsumer()
        {
            _dbOptions = new DbContextOptionsBuilder<PostDbContext>()
                .UseSqlServer("Server=localhost;Database=PostDb;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "test-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(cancellationToken);
                    var post = JsonSerializer.Deserialize<Post>(result.Message.Value);

                    if (post != null)
                    {
                        using var db = new PostDbContext(_dbOptions);
                        db.Posts.Add(post);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }
}
