namespace SkinGen.API.Models;

public class Recommendation
{
    public Product Product { get; set; } = null!;
    public double Score { get; set; }
    public int Rank { get; set; }
    public Dictionary<string, object> Explanation { get; set; } = new();
}

public class RecommendationResponse
{
    public List<Recommendation> Recommendations { get; set; } = new();
    public int TotalScreened { get; set; }
    public int SafeProducts { get; set; }
}