using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Chd.Mapping.Bechmark;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class MappingBenchmark
{
    private OrderDto _dto;
    private IMapper _mapper;

    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderDto, OrderEntity>()
                .ForMember(d => d.NetTotal,
                    o => o.MapFrom(s => s.Price * (s.Tax + 100) / 100 - s.Discount))
                .ForMember(d => d.StatusText,
                    o => o.MapFrom(s => s.IsActive ? "Active" : "Passive"));
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        _dto = new OrderDto
        {
            Price = 100,
            Tax = 18,
            Discount = 2,
            IsActive = true
        };

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Benchmark(Baseline = true)]
    public OrderEntity ChdMapping() => _dto;

    [Benchmark]
    public OrderEntity AutoMapper() => _mapper.Map<OrderEntity>(_dto);
}


#region PROGRAM

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<MappingBenchmark>();
    }
}

#endregion

