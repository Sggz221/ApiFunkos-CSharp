namespace cSharpApiFunko.Errors;

public abstract class FunkoError(string Mensaje)
{
    public string Mensaje { get; } =  Mensaje;
}

public class NotFound(string Mensaje) : FunkoError(Mensaje);

public class BadRequest(string Mensaje): FunkoError(Mensaje);

public class Validation(string Mensaje): FunkoError(Mensaje);