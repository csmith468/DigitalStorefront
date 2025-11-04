namespace API.Models.Constants;

public enum RoleEnum
{
    Admin = 1,
    ProductWriter = 2,
    ImageManager = 3
}

public class RoleNames
{
    public const string Admin = nameof(RoleEnum.Admin);
    public const string ProductWriter = nameof(RoleEnum.ProductWriter);
    public const string ImageManager = nameof(RoleEnum.ImageManager);

    public static IEnumerable<(int Id, string Name)> GetAll()
    {
        return Enum.GetValues<RoleEnum>().Select(r => ((int)r, r.ToString()));
    }
}