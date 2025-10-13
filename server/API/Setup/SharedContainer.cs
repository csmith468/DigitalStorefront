using API.Database;
using AutoMapper;

namespace API.Setup;

public interface ISharedContainer
{
    T? DepInj<T>() where T : class;
    IConfiguration Config { get; }
    IDataContextDapper Dapper { get; }
    IMapper Mapper { get; }
}

public class SharedContainer(IServiceProvider serviceProvider) : ISharedContainer
{
    public T? DepInj<T>() where T : class
    {
        return serviceProvider.GetService<T>();
    }
    public IConfiguration Config => serviceProvider.GetService<IConfiguration>()!;
    public IDataContextDapper Dapper => serviceProvider.GetService<IDataContextDapper>()!;
    public IMapper Mapper => serviceProvider.GetService<IMapper>()!;
}