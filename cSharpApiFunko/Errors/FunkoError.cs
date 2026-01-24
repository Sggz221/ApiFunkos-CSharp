namespace cSharpApiFunko.Errors;

public abstract class FunkoError(string Mensaje)
{
    public string Mensaje { get; } =  Mensaje;
}

public class NotFoundError(string Mensaje) : FunkoError(Mensaje);

public class ConflictError(string Mensaje): FunkoError(Mensaje);

public class ValidationError(string Mensaje): FunkoError(Mensaje);

public class StorageError(string Mensaje): FunkoError(Mensaje);
