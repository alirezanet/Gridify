# Dependency Injection

Gridify offers a powerful feature that enables you to streamline data mapping and configurations in your application by integrating with the Dependency Injection (DI) container. By registering your mapping profiles with DI, you can achieve cleaner, more maintainable code and improved separation of concerns. This section provides an overview of how to register your GridifyMapper configurations with DI.

## Register GridifyMapper with DI

Registering Gridify mapping with DI is a straightforward process. You'll define mapping profiles for your models and then register them in the DI container. Follow these steps to get started:

## 1. Define Mapping Profiles

Create mapping profiles by inheriting from `GridifyMapper<T>`, where `T` represents the type you want to map. Configure your mappings within these profile classes.

Example:

``` csharp
public class WeatherForecastGridifyMapper : GridifyMapper<WeatherForecast>
{
    public WeatherForecastGridifyMapper()
    {
        // Define your mappings here
        AddMap("summary", q => q.Summary);
        AddMap("temp", q => q.TemperatureC);

        // optionally you can customize the configuration for each mapper
        Configuration.CaseSensitive = false;
        Configuration.AllowNullSearch = true;
        Configuration.IgnoreNotMappedFields = true;

    }
}
```

## 2. Register Mapping Profiles

Utilize the `AddGridifyMappers` extension method available on the IServiceCollection to scan your assembly and register all mapping profiles.

Example:

``` csharp
using Gridify; // Make sure to include the necessary namespace
// ...

public void ConfigureServices(IServiceCollection services)
{
    // Other service registrations

    services.AddGridifyMappers(typeof(Program).Assembly);
}
```

## 3. Inject and Use Mappers

Once you've registered the mapping profiles, you can inject the corresponding `IGridifyMapper<T>` interfaces into your services or controllers.

Example:

:::: code-group

```csharp [Extensions] :line-numbers
public class WeatherForecastController : ControllerBase
{
    private readonly IGridifyMapper<WeatherForecast> _mapper;

    public WeatherForecastController(IGridifyMapper<WeatherForecast> mapper)
    {
        _mapper = mapper;
    }

   [HttpGet(Name = "GetWeatherForecast")]
    public Paging<WeatherForecast> Get([FromQuery] GridifyQuery query)
    {
        IQueryable<WeatherForecast> result = GetWeatherForecasts();

        // You can pass the mapper to the GridifyExtension
        return result.Gridify(query, _mapper);
    }
}
```

``` csharp [QueryBuilder] :line-numbers
public class WeatherForecastController : ControllerBase
{
    private readonly IGridifyMapper<WeatherForecast> _mapper;

    public WeatherForecastController(IGridifyMapper<WeatherForecast> mapper)
    {
        _mapper = mapper;
    }

  [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get([FromQuery] GridifyQuery query)
    {
        var result = GetWeatherForecasts();

        var queryBuilder = new QueryBuilder<WeatherForecast>()
            .UseCustomMapper(_mapper)
            .AddQuery(query);

        return queryBuilder.Build(result);
    }
}
```
