using SkinGen.API.Models;
using Parquet.Serialization;
using System.Text.Json;

namespace SkinGen.API.Services;

public class Recommender
{
    private List<Product> _products = new();
    private const double PenaltyValue = 0.9;

    public Recommender()
    {
        LoadData();
    }
    private void LoadData()
    {
        var jsonPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "Data", 
            "skingen_data.json"
        );
        
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"ERROR: Data file not found at {jsonPath}");
            return;
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            _products = JsonSerializer.Deserialize<List<Product>>(json) ?? new();
            
            Console.WriteLine($"✓ Loaded {_products.Count} products from JSON");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR loading JSON: {ex.Message}");
        }
    }
    // private void LoadData()
    // {
    //     var parquetPath = Path.Combine(
    //         AppDomain.CurrentDomain.BaseDirectory, 
    //         "Data", 
    //         "skingen_products_enriched_simple.parquet"
    //     );
        
    //     if (!File.Exists(parquetPath))
    //     {
    //         Console.WriteLine($"ERROR: Data file not found at {parquetPath}");
    //         return;
    //     }

    //     try
    //     {
    //         using var stream = File.OpenRead(parquetPath);
    //         _products = ParquetSerializer.DeserializeAsync<Product>(stream).Result.ToList();
            
    //         Console.WriteLine($"✓ Loaded {_products.Count} products from Parquet");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"ERROR loading Parquet: {ex.Message}");
    //     }
    // }

    public RecommendationResponse GetRecommendations(UserQuery query, int n = 10)
    {
        // var candidates = _products.Where(p => p.Type == query.ProductType).ToList();
        var candidates = _products.Where(p => 
        p.Type.Equals(query.ProductType, StringComparison.OrdinalIgnoreCase)).ToList();
        
        int initialCount = candidates.Count;
        Console.WriteLine($"Found {initialCount} {query.ProductType} products");
        
        candidates = ApplyHardFilters(candidates, query);
        Console.WriteLine($"After filtering: {candidates.Count} safe products");
        
        if (candidates.Count == 0)
        {
            return new RecommendationResponse
            {
                Recommendations = new(),
                TotalScreened = initialCount,
                SafeProducts = 0
            };
        }
        
        var scoredProducts = CalculateScores(candidates, query);
        
        var topN = scoredProducts
            .OrderByDescending(p => p.Score)
            .Take(n)
            .Select((p, index) => new Recommendation
            {
                Product = p.Product,
                Score = Math.Round(p.Score, 4),
                Rank = index + 1,
                Explanation = GenerateExplanation(p.Product, query)
            })
            .ToList();

        return new RecommendationResponse
        {
            Recommendations = topN,
            TotalScreened = initialCount,
            SafeProducts = candidates.Count
        };
    }

    private List<Product> ApplyHardFilters(List<Product> candidates, UserQuery query)
    {
        if (query.SkinConditions != null && query.SkinConditions.Any())
        {
            candidates = candidates.Where(p => 
                !p.ConditionConcerns.Any(cc => query.SkinConditions.Contains(cc))
            ).ToList();
        }

        if (query.BlockedCategories != null)
        {
            if (query.BlockedCategories.Contains("fragrance"))
                candidates = candidates.Where(p => !p.HasFragrance).ToList();
            
            if (query.BlockedCategories.Contains("alcohol"))
                candidates = candidates.Where(p => !p.HasDryingAlcohol).ToList();
        }

        if (query.IngredientGroups != null)
        {
            foreach (var group in query.IngredientGroups)
            {
                candidates = group switch
                {
                    "vitamin_c" => candidates.Where(p => p.HasVitaminC).ToList(),
                    "hyaluronic_acid" => candidates.Where(p => p.HasHyaluronicAcid).ToList(),
                    "niacinamide" => candidates.Where(p => p.HasNiacinamide).ToList(),
                    "ceramides" => candidates.Where(p => p.HasCeramides).ToList(),
                    "aha" => candidates.Where(p => p.HasAha).ToList(),
                    "bha" => candidates.Where(p => p.HasBha).ToList(),
                    "retinoids" => candidates.Where(p => p.HasRetinoids).ToList(),
                    "peptides" => candidates.Where(p => p.HasPeptides).ToList(),
                    "antioxidants" => candidates.Where(p => p.HasAntioxidants).ToList(),
                    _ => candidates
                };
            }
        }

        if (query.Allergies != null)
        {
            foreach (var allergen in query.Allergies)
            {
                candidates = candidates.Where(p => 
                    !p.IngredientList.Any(ing => ing.Contains(allergen, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
        }

        var concernRules = new Dictionary<string, string[]>
        {
            { "redness_reducing", new[] { "irritating" } },
            { "acne_fighting", new[] { "acne_trigger", "comedogenic" } }
        };

        var avoidConcerns = new HashSet<string>();
        foreach (var concern in query.Concerns)
        {
            if (concernRules.ContainsKey(concern))
            {
                foreach (var avoid in concernRules[concern])
                    avoidConcerns.Add(avoid);
            }
        }

        if (avoidConcerns.Any())
        {
            candidates = candidates.Where(p => 
                !p.NegativeConcerns.Any(nc => avoidConcerns.Contains(nc))
            ).ToList();
        }

        return candidates;
    }

    private List<(Product Product, double Score)> CalculateScores(List<Product> candidates, UserQuery query)
    {
        var results = new List<(Product, double)>();

        foreach (var product in candidates)
        {
            var score = CalculateCosineSimilarity(product, query);
            score = ApplySoftPenalties(score, product, query);
            results.Add((product, score));
        }

        return results;
    }

    private double CalculateCosineSimilarity(Product product, UserQuery query)
    {
        var userConcerns = new HashSet<string>(query.Concerns);
        var productConcerns = new HashSet<string>(product.PositiveConcerns);
        
        var intersection = userConcerns.Intersect(productConcerns).Count();
        
        if (intersection == 0) return 0;
        
        var userMagnitude = Math.Sqrt(userConcerns.Count);
        var productMagnitude = Math.Sqrt(productConcerns.Count);
        
        return intersection / (userMagnitude * productMagnitude);
    }

    private double ApplySoftPenalties(double score, Product product, UserQuery query)
    {
        if (string.IsNullOrEmpty(query.SkinType)) return score;

        var penaltyRules = new Dictionary<string, string[]>
        {
            { "dry_skin", new[] { "drying" } },
            { "oily_skin", new[] { "may_worsen_oily_skin" } },
            { "sensitive_skin", new[] { "irritating", "drying" } },
            { "combination_skin", new[] { "drying", "may_worsen_oily_skin" } }
        };

        if (!penaltyRules.ContainsKey(query.SkinType)) return score;

        var penalties = penaltyRules[query.SkinType];
        int penaltyCount = product.NegativeConcerns.Count(nc => penalties.Contains(nc));

        return score * Math.Pow(PenaltyValue, penaltyCount);
    }

    private Dictionary<string, object> GenerateExplanation(Product product, UserQuery query)
    {
        var explanation = new Dictionary<string, object>();

        var matchedConcerns = product.PositiveConcerns.Intersect(query.Concerns).ToList();
        explanation["matched_concerns"] = matchedConcerns;
        explanation["all_claims"] = product.PositiveConcerns;
        explanation["warnings"] = product.NegativeConcerns;

        var verifiedIngredients = new Dictionary<string, List<string>>();
        
        var concernToLists = new Dictionary<string, string[]>
        {
            { "hydrating", new[] { "Humectant", "Emollient", "Occlusive" } },
            { "anti_aging", new[] { "Retinoid", "Peptide", "Antioxidant" } },
            { "brightening", new[] { "Exfoliant", "Antioxidant" } },
            { "dark_spots", new[] { "Exfoliant", "Antioxidant" } }
        };

        foreach (var concern in query.Concerns)
        {
            if (concernToLists.ContainsKey(concern))
            {
                var ingredients = new List<string>();
                
                foreach (var listType in concernToLists[concern])
                {
                    var list = listType switch
                    {
                        "Retinoid" => product.RetinoidList,
                        "Peptide" => product.PeptideList,
                        "Antioxidant" => product.AntioxidantList,
                        "Humectant" => product.HumectantList,
                        "Emollient" => product.EmollientList,
                        "Occlusive" => product.OcclusiveList,
                        "Exfoliant" => product.ExfoliantList,
                        _ => new List<string>()
                    };
                    
                    ingredients.AddRange(list.Take(3));
                }
                
                if (ingredients.Any())
                {
                    verifiedIngredients[concern] = ingredients.Distinct().Take(5).ToList();
                }
            }
        }

        explanation["verified_ingredients"] = verifiedIngredients;
        
        explanation["safety_checks"] = new Dictionary<string, bool>
        {
            { "fragrance_free", !product.HasFragrance },
            { "alcohol_free", !product.HasDryingAlcohol },
            { "irritant_free", !product.HasIrritants }
        };

        return explanation;
    }
}