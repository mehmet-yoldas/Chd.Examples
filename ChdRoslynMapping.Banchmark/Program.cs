using System;
using System.Linq;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Chd.Mapping.Abstractions;

namespace ChdRoslynMappingTest
{
    //Mark your DTO with [MapTo] to enable simple mapping and expression mapping
    [MapTo(typeof(OrderEntity))]
    public partial class OrderDto
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        // Chd mapping expression-based calculated property only this line is needed,
        // no need to write any mapping code and configuration!
        [MapProperty("Price * (Tax + 100) / 100 - Discount")]
        public decimal NetTotal { get; set; }
    }

    public partial class OrderEntity
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MappingBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class MappingBenchmark
    {

        private OrderDto[] _orders;
        private IMapper _mapper;

        public MappingBenchmark()
        {
            #region Samle data generation
            _orders = Enumerable.Range(0, 10000)
            .Select(i => new OrderDto
            {
                Price = i,
                Tax = 20,
                Discount = 5
            })
            .ToArray();
            #endregion

            #region Configure AutoMapper 
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrderDto, OrderEntity>()
                   .ForMember(
                       d => d.NetTotal,
                       opt => opt.MapFrom(
                           s => s.Price * (s.Tax + 100) / 100 - s.Discount));
            });

            _mapper = config.CreateMapper();
            #endregion
        }

        #region Single mapping benchmark
        [Benchmark]
        public OrderEntity AutoMapper_Single()
            => _mapper.Map<OrderEntity>(_orders[0]);

        [Benchmark]
        public OrderEntity ChdMapping_Single()
            => (OrderEntity)_orders[0];
        #endregion

        #region Batch mapping benchmark

        [Benchmark]
        public OrderEntity[] AutoMapper_Batch()
        {
            var result = new OrderEntity[_orders.Length];

            for (int i = 0; i < _orders.Length; i++)
                result[i] = _mapper.Map<OrderEntity>(_orders[i]);

            return result;
        }

        [Benchmark]
        public OrderEntity[] ChdMapping_Batch()
        {
            var result = new OrderEntity[_orders.Length];

            for (int i = 0; i < _orders.Length; i++)
                result[i] = (OrderEntity)_orders[i];

            return result;
        }
        #endregion
    }
}