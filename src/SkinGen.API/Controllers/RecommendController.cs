using Microsoft.AspNetCore.Mvc;
using SkinGen.API.Models;
using SkinGen.API.Services;

namespace SkinGen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendController : ControllerBase
{
    private readonly Recommender _recommender;

    public RecommendController()
    {
        _recommender = new Recommender();
    }

    [HttpPost]
    public ActionResult<RecommendationResponse> GetRecommendations(
        [FromBody] UserQuery query,
        [FromQuery] int n = 10)
    {
        try
        {
            var result = _recommender.GetRecommendations(query, n);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("product-types")]
    public ActionResult<string[]> GetProductTypes()
    {
        return Ok(new[] 
        { 
            "Serum", 
            "General Moisturizer",
            "Day Moisturizer",
            "Night Moisturizer",
            "Face Cleanser", 
            "Toner",
            "Exfoliator",
            "Facial Treatment",
            "Sunscreen",
            "Essence"
        });
    }

    [HttpGet("concerns")]
    public ActionResult<string[]> GetConcerns()
    {
        return Ok(new[] 
        { 
            "hydrating", 
            "anti_aging", 
            "brightening",
            "acne_fighting",
            "redness_reducing",
            "dark_spots",
            "good_for_oily_skin"
        });
    }
}