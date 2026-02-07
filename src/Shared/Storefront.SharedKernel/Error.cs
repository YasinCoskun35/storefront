namespace Storefront.SharedKernel;

public record Error(string Code, string Message, string Type)
{
    public static Error NotFound(string code, string message) => 
        new(code, message, "NotFound");

    public static Error Validation(string code, string message) => 
        new(code, message, "Validation");

    public static Error Conflict(string code, string message) => 
        new(code, message, "Conflict");

    public static Error Failure(string code, string message) => 
        new(code, message, "Failure");
}

