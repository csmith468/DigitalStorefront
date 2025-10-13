using API.Database;

namespace API.Setup;

public interface ISharedContainer
{
    T DepInj<T>() where T : class;
    IConfiguration Config { get; }
    IDataContextDapper Dapper { get; }
}

public class SharedContainer : ISharedContainer
{
    private readonly IServiceProvider _serviceProvider;

    public SharedContainer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public T DepInj<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }
    public IConfiguration Config => _serviceProvider.GetService<IConfiguration>();
    public IDataContextDapper Dapper => _serviceProvider.GetService<IDataContextDapper>();

}