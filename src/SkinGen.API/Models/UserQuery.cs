namespace SkinGen.API.Models;

public class UserQuery
{
    public string ProductType { get; set; } = string.Empty;
    public List<string> Concerns { get; set; } = new();
    public string? SkinType { get; set; }
    public List<string>? SkinConditions { get; set; }
    public List<string>? IngredientGroups { get; set; }
    public List<string>? SpecificIngredients { get; set; }
    public List<string>? BlockedCategories { get; set; }
    public List<string>? Allergies { get; set; }
}