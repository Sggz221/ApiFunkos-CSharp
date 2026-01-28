using cSharpApiFunko.GraphQl.Inputs;
using cSharpApiFunko.Models.Dto;
using cSharpApiFunko.Services.Funkos;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;

namespace cSharpApiFunko.GraphQl.Mutation;

public class FunkoMutation
{

    [GraphQLDescription("Crea un nuevo Funko.")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<FunkoResponseDto> CrearFunko(
        FunkoCreateInput input,
        [Service] IFunkoService service,
        [Service] ITopicEventSender eventSender)
    {
        var request = new FunkoRequestDto(
            Nombre: input.Nombre,
            Categoria: input.Categoria,
            Precio: input.Precio,
            Image: input.Image
        );
        
        var result = await service.SaveAsync(request);
        
        if (result.IsFailure)
        {
            throw new GraphQLException(result.Error.Mensaje);
        }
        
        await eventSender.SendAsync("FunkoCreado", result.Value);
        return result.Value;
    }

    [GraphQLDescription("Actualiza un Funko.")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<FunkoResponseDto> UpdateFunko(
        FunkoUpdateInput input,
        [Service] IFunkoService service,
        [Service] ITopicEventSender eventSender)
    {
        var request = new FunkoPatchRequestDto
        {
            Nombre = input.Nombre,
            Categoria = input.Categoria,
            Precio = input.Precio,
            Image = input.Image
        };
        
        var result = await service.PatchAsync(input.Id, request);
        
        if (result.IsFailure)
        {
            throw new GraphQLException(result.Error.Mensaje);
        }
        
        await eventSender.SendAsync("FunkoActualizado", result.Value);
        return result.Value;
    }

    [GraphQLDescription("Elimina un Funko.")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<FunkoResponseDto> DeleteFunko(
        long id,
        [Service] IFunkoService service,
        [Service] ITopicEventSender eventSender)
    {
        var result = await service.DeleteAsync(id);
        
        if (result.IsFailure)
        {
            throw new GraphQLException(result.Error.Mensaje);
        }
        
        await eventSender.SendAsync("FunkoEliminado", result.Value);
        return result.Value;
    }
}
