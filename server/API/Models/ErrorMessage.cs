using System.Net;

namespace API.Models;

public class ErrorMessage
{
    public string Message { get; }
    public HttpStatusCode StatusCode { get; }

    public ErrorMessage(string message, HttpStatusCode statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }
}

public static class ErrorMessages
{
    public static class Product
    {
        public static ErrorMessage NotFound(int productId) => 
            new($"Product {productId} not found", HttpStatusCode.NotFound);
        public static ErrorMessage NotFound(string slug) => 
            new($"Product {slug} not found", HttpStatusCode.NotFound);
        public static ErrorMessage NameExists(string name) => 
            new($"Product name {name} already exists", HttpStatusCode.BadRequest);
        public static ErrorMessage SlugExists(string slug) => 
            new($"Product slug {slug} already exists", HttpStatusCode.BadRequest);
        public static readonly ErrorMessage CreationFailed = 
            new("Product could not be created.", HttpStatusCode.InternalServerError);
        public static readonly ErrorMessage DeleteFailed = 
            new("Failed to delete product. Please try again.", HttpStatusCode.InternalServerError);
        public static readonly ErrorMessage Unauthorized = 
            new("You do not have permission to create a product", HttpStatusCode.Unauthorized);
        public static readonly ErrorMessage DemoProductRestricted =
            new("Demo products can only be managed by administrators", HttpStatusCode.Forbidden);
    }
    
    public static class Image
    {
        public static readonly ErrorMessage NotFound = 
            new("Image not found", HttpStatusCode.NotFound);
        public static readonly ErrorMessage AddFailed = 
            new("Failed to add image. Please try again.", HttpStatusCode.InternalServerError);
        public static readonly ErrorMessage SetPrimaryFailed = 
            new("Failed to set primary image. Please try again.", HttpStatusCode.InternalServerError);
        public static readonly ErrorMessage DeleteFailed = 
            new("Failed to delete image. Please try again.", HttpStatusCode.InternalServerError);
        public static readonly ErrorMessage ReorderFailed = 
            new("Failed to reorder images. Please try again.", HttpStatusCode.InternalServerError);
    }
    
    public static class Auth
    {
        public static readonly ErrorMessage InvalidCredentials =
            new("Invalid username or password", HttpStatusCode.Unauthorized);
        public static readonly ErrorMessage InvalidToken =
            new("Invalid token.", HttpStatusCode.Unauthorized);
        public static readonly ErrorMessage EmailExists =
            new("Email already exists.", HttpStatusCode.BadRequest);
        public static readonly ErrorMessage UsernameExists =
            new("Username already exists.", HttpStatusCode.BadRequest);
        public static readonly ErrorMessage RegistrationFailed =
            new("Failed to register user.", HttpStatusCode.InternalServerError);
    }
    
    public static class Metadata
    {
        public static ErrorMessage InvalidSubcategories(string invalidIds) =>
            new($"Invalid subcategoryIds: {invalidIds}", HttpStatusCode.BadRequest);
    }
}