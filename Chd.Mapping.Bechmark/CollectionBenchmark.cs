using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
// using AutoMapper.Extensions.Microsoft.DependencyInjection; // not used to avoid ambiguity
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Chd.Mapping.Bechmark;
using System.Collections.Generic;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class CollectionBenchmark
{
    private List<OrderDto> _dtos = new List<OrderDto>();
    private IMapper _mapper = null!;

    private const int Count = 20;

    [GlobalSetup]
    public void Setup()
    {
        _dtos = new List<OrderDto>(Count);
        for (int i = 0; i < Count; i++)
        {
            _dtos.Add(new OrderDto
            {
                Price = 100,
                Tax = 18,
                Discount = 2,
                IsActive = true,
                Name = "Mehmet",
                Surname = "Yoldas"
            });
        }

        var services = new ServiceCollection();
        services.AddAutoMapper(cfg => cfg.CreateMap<OrderDto, OrderEntity>()
                .ForMember(d => d.NetTotal,
                    o => o.MapFrom(s => s.Price * (s.Tax + 100) / 100 - s.Discount))
                .ForMember(d => d.StatusText,
                    o => o.MapFrom(s => s.IsActive ? "Active" : "Passive")));

        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();
    }

    // 🚀 CHD – Source Generated
    [Benchmark(Baseline = true)]
    public List<OrderEntity> ChdCollectionMap()
    {
        var result = new List<OrderEntity>(Count);
        for (int i = 0; i < Count; i++)
            result.Add((OrderEntity)_dtos[i]);

        return result;
    }

    // 🐌 AutoMapper – Runtime Mapping
    [Benchmark]
    public List<OrderEntity> AutoMapperCollectionMap()
    {
        var result = new List<OrderEntity>(Count);
        for (int i = 0; i < Count; i++)
            result.Add(_mapper.Map<OrderEntity>(_dtos[i]));

        return result;
    }
}
