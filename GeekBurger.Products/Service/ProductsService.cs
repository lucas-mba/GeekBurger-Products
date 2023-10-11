using AutoMapper;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Azure.Messaging.ServiceBus.Administration;
using GeekBurger.Products.Repository;
using GeekBurger.Products.Contract;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using GeekBurger.Products.Model;

namespace GeekBurger.Products.Service
{
    public class ProductsService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private IMapper _mapper;
        private readonly List<ServiceBusMessage> _messages;
        private Task _lastTask;
        //private readonly IServiceBusNamespace _namespace;
        private CancellationTokenSource _cancelMessages;
        private IServiceProvider _serviceProvider { get; }
        private ServiceBusConfiguration _serviceBusConfiguration { get; }
        private IProductsRepository _productsRepository;

        public ProductsService(IMapper mapper,
            IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _configuration = configuration;
            _messages = new List<ServiceBusMessage>();

            _cancelMessages = new CancellationTokenSource();
            _serviceProvider = serviceProvider;

            _serviceBusConfiguration = _configuration.GetSection("serviceBus").Get<ServiceBusConfiguration>();
            
        }

        public async Task EnsureQueueIsCreated(string queue)
        {
            var adminClient = new ServiceBusAdministrationClient(_serviceBusConfiguration.ConnectionString);
            if (!await adminClient.QueueExistsAsync(queue))
            {
                var queueOptions = new CreateQueueOptions(queue)
                {
                    DefaultMessageTimeToLive = TimeSpan.FromHours(1),
                    LockDuration = TimeSpan.FromSeconds(30),
                    RequiresSession = true
                };
                await adminClient.CreateQueueAsync(queueOptions);
            }
        }

        private Task Processor_ProcessErrorAsync(
            ProcessErrorEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private async Task Processor_ProcessMessageAsync(
            ProcessSessionMessageEventArgs arg)
        {
            var productsList = GetProductsByStoreName(arg.Message.Body.ToString());
            await SendReplyMessage(productsList, arg.SessionId);
            await arg.CompleteMessageAsync(arg.Message);
        }

        private async Task SendReplyMessage(string msg, string sessionId)
        {
            var sender = new ServiceBusClient(_serviceBusConfiguration.ConnectionString).CreateSender(
                _serviceBusConfiguration.ProductsPubQueue);
            var requestMsg = new ServiceBusMessage(msg);
            requestMsg.SessionId = sessionId;
            await sender.SendMessageAsync(requestMsg);
        }

        public string GetProductsByStoreName(string storeName)
        {
            using var scope = _serviceProvider.CreateScope();
            var productRepository =
                scope.ServiceProvider
                    .GetRequiredService<IProductsRepository>();
            var productsByStore = productRepository.GetProductsByStoreName(storeName).ToList();

            if (productsByStore.Count <= 0)
                return "Nenhum dado encontrado";

            var productsToGet = _mapper.Map<IEnumerable<ProductToGet>>(productsByStore);

            return JsonConvert.SerializeObject(productsToGet);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var options = new ServiceBusSessionProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentSessions = 100,
                PrefetchCount = 100
            };

            EnsureQueueIsCreated(_serviceBusConfiguration.ProductsPubQueue).Wait();
            EnsureQueueIsCreated(_serviceBusConfiguration.ProductsSubQueue).Wait();

            var processor = new ServiceBusClient(_serviceBusConfiguration.ConnectionString)
                .CreateSessionProcessor(
                    _serviceBusConfiguration.ProductsSubQueue, options);
            processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

            processor.StartProcessingAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelMessages.Cancel();

            return Task.CompletedTask;
        }
    }
}
