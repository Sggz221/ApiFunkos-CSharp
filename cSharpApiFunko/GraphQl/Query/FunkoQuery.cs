using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Services.Funkos;

namespace cSharpApiFunko.GraphQl.Query;

public class FunkoQuery
{   
    private readonly ILogger<FunkoQuery> _log;

    public FunkoQuery(ILogger<FunkoQuery> log)
    {
        _log = log;
    }
    [GraphQLName("funkos")]
    [GraphQLDescription("Obtiene la lista de Funkos con filtrado y paginacion")]
    public async Task<IEnumerable<FunkoResponseDto>> GetFunkos(
        [Service] IFunkoService service,
        string? nombre,
        string? categoria,
        double? maxPrecio,
        int page = 1,
        int size = 10,
        string sortBy = "id",
        string direction = "asc")
    {
        _log.LogDebug("GRAPHQL: Obteniendo la lista de funkos...");
        var filter = new FilterDto(
            Nombre: nombre,
            Categoria: categoria,
            MaxPrecio: maxPrecio,
            Page: page,
            Size: size,
            SortBy: sortBy,
            Direction: direction);
        var result = await service.GetAllAsync(filter);
        return result.Value.Items;
    }

    [GraphQLDescription("Obtiene un Funko por su ID")]
    public async Task<object> GetById(long id, [Service] IFunkoService service)
    {
        _log.LogDebug($"GRAPHQL: Obteniendo Funko por ID: {id}");
        var result = await service.GetByIdAsync(id);
        return result.IsSuccess ? result.Value : result.Error;
    }
}