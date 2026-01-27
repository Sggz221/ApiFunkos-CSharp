using cSharpApiFunko.Models.Dto;

namespace cSharpApiFunko.GraphQl;

public class FunkoSubscription
{
    [Subscribe]
    [Topic("FunkoCreado")]
    [GraphQLDescription("Se dispara cuando un nuevo Funko es creado.")]
    public FunkoResponseDto OnFunkoCreado([EventMessage] FunkoResponseDto funko) 
    {
        return funko;
    }

    [Subscribe]
    [Topic("FunkoActualizado")]
    [GraphQLDescription("Se dispara cuando un Funko existente es actualizado.")]
    public FunkoResponseDto OnFunkoActualizado([EventMessage] FunkoResponseDto funko) 
    {
        return funko;
    }

    [Subscribe]
    [Topic("FunkoEliminado")]
    [GraphQLDescription("Se dispara cuando un Funko es eliminado.")]
    public FunkoResponseDto OnFunkoEliminado([EventMessage] FunkoResponseDto funko) 
    {
        return funko;
    }
}