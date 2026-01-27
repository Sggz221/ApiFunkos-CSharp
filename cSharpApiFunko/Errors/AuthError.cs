namespace cSharpApiFunko.Errors;

public abstract record AuthError(string Message);

public record AuthConflictError(string Message): AuthError(Message);
public record UnauthorizedError(string Message): AuthError(Message);
