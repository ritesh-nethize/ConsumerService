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
        private readonly IKafkaConsumer _kafkaConsumer;
        private readonly ILogger<PostConsumerWorker> _logger;

        public PostConsumerWorker(IKafkaConsumer kafkaConsumer, ILogger<PostConsumerWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kafka consumer started.");
            await _kafkaConsumer.StartConsumingAsync(stoppingToken);
        }
    }
}
