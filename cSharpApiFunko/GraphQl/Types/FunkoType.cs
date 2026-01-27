using cSharpApiFunko.Models;

namespace cSharpApiFunko.GraphQl.Types;

public class FunkoType: ObjectType<Funko>
{
    protected override void Configure(IObjectTypeDescriptor<Funko> descriptor)
    {
        descriptor.Description("Representa una figura Funkod de la tienda");
        
        descriptor.Field(f => f.Id).Description("Identificador unico de un Funko");
        descriptor.Field(f => f.Nombre).Description("Nombre del Funko");
        descriptor.Field(f => f.Precio).Description("Precio del Funko en euros");
        descriptor.Field(f => f.Categoria).Description("Categoria a la que pertenece el Funko");
        descriptor.Field(f => f.Image).Description("URL de la imagen del Funko");
    }
}