using Microsoft.AspNetCore.Mvc;
using SkinGen.API.Models;
using SkinGen.API.Services;

namespace SkinGen.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendController : ControllerBase
{
    private readonly Recommender _recommender;

    // Correct: ASP.NET injects the singleton Recommender
    public RecommendController(Recommender recommender)
    {
        _recommender = recommender;
    }

    // POST api/recommend
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

    // GET api/recommend/product-types
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

    // GET api/recommend/concerns
    [HttpGet("concerns")]
    public ActionResult<string[]> GetConcerns()
    {
        return Ok(new[]
        {
            "brightening",
            "anti_aging",
            "redness_reducing",
            "good_for_oily_skin",
            "reduces_large_pores",
            "acne_fighting",
            "hydrating",
            "dark_spots",
            "scar_healing",
            "skin_texture"
        });
    }
}
