using SkinGen.API.Models;
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

    public RecommendationResponse GetRecommendations(UserQuery query, int n = 10)
    {
        var candidates = _products.Where(p => 
            p.Type.Equals(query.ProductType, StringComparison.OrdinalIgnoreCase)
        ).ToList();
        
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
        // Filter out products that trigger skin conditions
        if (query.SkinConditions != null && query.SkinConditions.Any())
        {
            candidates = candidates.Where(p => 
                !p.ConditionConcerns.Any(cc => query.SkinConditions.Contains(cc))
            ).ToList();
        }

        // Filter by blocked categories (fragrance, alcohol)
        if (query.BlockedCategories != null)
        {
            if (query.BlockedCategories.Contains("fragrance"))
                candidates = candidates.Where(p => !p.HasFragrance).ToList();
            
            if (query.BlockedCategories.Contains("alcohol"))
                candidates = candidates.Where(p => !p.HasDryingAlcohol).ToList();
        }

        // Filter by required ingredient groups
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

        // Filter by specific ingredients (user-typed tags)
        if (query.SpecificIngredients != null)
        {
            Console.WriteLine($"Looking for specific ingredients: {string.Join(", ", query.SpecificIngredients)}");
            
            foreach (var ingredient in query.SpecificIngredients)
            {
                var beforeCount = candidates.Count;
                
                candidates = candidates.Where(p => 
                    p.IngredientList.Any(ing => ing.Contains(ingredient, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                Console.WriteLine($"  After filtering for '{ingredient}': {candidates.Count} products (was {beforeCount})");
            }
        }

        // Filter out allergens
        if (query.Allergies != null)
        {
            foreach (var allergen in query.Allergies)
            {
                candidates = candidates.Where(p => 
                    !p.IngredientList.Any(ing => ing.Contains(allergen, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
        }

        // Filter by concern-specific rules
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
        
        // Only show relevant warnings based on skin type
        var warnings = new List<string>();

        if (!string.IsNullOrEmpty(query.SkinType))
        {
            var relevantWarnings = new Dictionary<string, string[]>
            {
                { "dry_skin", new[] { "drying" } },
                { "oily_skin", new[] { "may_worsen_oily_skin" } },
                { "sensitive_skin", new[] { "irritating", "drying" } },
                { "combination_skin", new[] { "drying", "may_worsen_oily_skin" } }
                // normal_skin gets NO warnings from negative_concerns
            };
            
            if (relevantWarnings.ContainsKey(query.SkinType))
            {
                var applicableWarnings = product.NegativeConcerns
                    .Where(nc => relevantWarnings[query.SkinType].Contains(nc))
                    .ToList();
                warnings.AddRange(applicableWarnings);
            }
        }
        else
        {
            // No skin type selected - show all warnings
            warnings.AddRange(product.NegativeConcerns);
        }

        // Add skin condition warnings ONLY if user selected "None" (wants informational warnings)
        // If user selected a specific condition, those products are already filtered out
        if (query.SkinConditions == null || !query.SkinConditions.Any())
        {
            // User selected "None" - show condition warnings as informational
            if (product.ConditionConcerns.Any())
            {
                var conditionWarnings = product.ConditionConcerns
                    .Select(cc => $"may_trigger_{cc}")  // Changed from "may_aggravate" to "may_trigger"
                    .ToList();
                warnings.AddRange(conditionWarnings);
            }
        }

        explanation["warnings"] = warnings;

        // Verified ingredients by concern with MORE ingredients shown (5 instead of 3)
        var verifiedIngredients = new Dictionary<string, Dictionary<string, List<string>>>();
        
        var concernToCategories = new Dictionary<string, string[]>
        {
            { "hydrating", new[] { "humectant", "emollient", "occlusive" } },
            { "anti_aging", new[] { "retinoid", "peptide", "antioxidant" } },
            { "brightening", new[] { "exfoliant", "antioxidant" } },
            { "dark_spots", new[] { "exfoliant", "antioxidant" } },
            { "redness_reducing", new[] { "antioxidant", "plant_extract" } },
            { "acne_fighting", new[] { "exfoliant" } },
            { "good_for_oily_skin", new[] { "exfoliant" } },
            { "reduces_large_pores", new[] { "exfoliant", "antioxidant" } },
            { "scar_healing", new[] { "peptide", "antioxidant" } },
            { "skin_texture", new[] { "exfoliant", "peptide" } }
        };

        // GLOBAL deduplication - track ALL ingredients shown across ALL concerns
        var globalSeenIngredients = new HashSet<string>();
        
        foreach (var concern in query.Concerns)
        {
            if (!concernToCategories.ContainsKey(concern)) continue;
            
            var categoryDict = new Dictionary<string, List<string>>();
            
            foreach (var category in concernToCategories[concern])
            {
                List<string> rawIngredients = category switch
                {
                    "retinoid" => product.RetinoidList,
                    "peptide" => product.PeptideList,
                    "antioxidant" => product.AntioxidantList,
                    "humectant" => product.HumectantList,
                    "emollient" => product.EmollientList,
                    "occlusive" => product.OcclusiveList,
                    "exfoliant" => product.ExfoliantList,
                    "plant_extract" => FilterFragranceComponents(product.PlantExtractList),
                    _ => new List<string>()
                };
                
                // Increased from 3 to 5 ingredients per category
                var uniqueIngredients = rawIngredients
                    .Where(ing => !globalSeenIngredients.Contains(ing))
                    .Take(5)
                    .ToList();
                
                if (uniqueIngredients.Any())
                {
                    foreach (var ing in uniqueIngredients)
                        globalSeenIngredients.Add(ing);
                    
                    var categoryName = char.ToUpper(category[0]) + category.Substring(1);
                    categoryDict[categoryName] = uniqueIngredients;
                }
            }
            
            if (categoryDict.Any())
            {
                verifiedIngredients[concern] = categoryDict;
            }
        }

        explanation["verified_ingredients"] = verifiedIngredients;
        
        // ADD COMPLETE INGREDIENT BREAKDOWN
        var ingredientBreakdown = new Dictionary<string, Dictionary<string, List<string>>>();
        
        // Actives
        var actives = new Dictionary<string, List<string>>();
        if (product.RetinoidList.Any()) actives["Retinoids"] = product.RetinoidList;
        if (product.PeptideList.Any()) actives["Peptides"] = product.PeptideList;
        if (product.AntioxidantList.Any()) actives["Antioxidants"] = product.AntioxidantList;
        if (product.ExfoliantList.Any()) actives["Exfoliants"] = product.ExfoliantList;
        if (actives.Any()) ingredientBreakdown["Actives"] = actives;
        
        // Support
        var support = new Dictionary<string, List<string>>();
        if (product.HumectantList.Any()) support["Humectants"] = product.HumectantList;
        if (product.EmollientList.Any()) support["Emollients"] = product.EmollientList;
        if (product.OcclusiveList.Any()) support["Occlusives"] = product.OcclusiveList;
        if (support.Any()) ingredientBreakdown["Support"] = support;
        
        // Utility
        var utility = new Dictionary<string, List<string>>();
        if (product.TextureEnhancerList.Any()) utility["Texture Enhancers"] = product.TextureEnhancerList;
        if (product.FilmFormingList.Any()) utility["Film Forming"] = product.FilmFormingList;
        if (product.AbsorbentList.Any()) utility["Absorbents"] = product.AbsorbentList;
        if (utility.Any()) ingredientBreakdown["Utility"] = utility;
        
        // Sensory
        var sensory = new Dictionary<string, List<string>>();
        var filteredPlantExtracts = FilterFragranceComponents(product.PlantExtractList);
        if (filteredPlantExtracts.Any()) sensory["Plant Extracts"] = filteredPlantExtracts;
        if (sensory.Any()) ingredientBreakdown["Sensory"] = sensory;
        
        // Risks
        // Risks - deduplicate ingredients that are both fragrance AND irritants
        var risks = new Dictionary<string, List<string>>();
        var allRiskIngredients = new HashSet<string>();

        if (product.FragranceIngredients.Any())
        {
            risks["Fragrance"] = product.FragranceIngredients;
            foreach (var ing in product.FragranceIngredients)
                allRiskIngredients.Add(ing);
        }

        if (product.IrritantIngredients.Any())
        {
            // Only add irritants that aren't already listed as fragrance
            var uniqueIrritants = product.IrritantIngredients
                .Where(ing => !allRiskIngredients.Contains(ing))
                .ToList();
            
            if (uniqueIrritants.Any())
            {
                risks["Irritants"] = uniqueIrritants;
                foreach (var ing in uniqueIrritants)
                    allRiskIngredients.Add(ing);
            }
        }

        if (product.DryingAlcoholIngredients.Any())
        {
            // Only add alcohols that aren't already listed
            var uniqueAlcohols = product.DryingAlcoholIngredients
                .Where(ing => !allRiskIngredients.Contains(ing))
                .ToList();
            
            if (uniqueAlcohols.Any())
                risks["Drying Alcohols"] = uniqueAlcohols;
        }

        if (risks.Any()) ingredientBreakdown["Risks"] = risks;
        explanation["ingredient_breakdown"] = ingredientBreakdown;
        
        explanation["safety_checks"] = new Dictionary<string, bool>
        {
            { "fragrance_free", !product.HasFragrance },
            { "alcohol_free", !product.HasDryingAlcohol },
            { "irritant_free", !product.HasIrritants }
        };

        return explanation;
    }

    private List<string> FilterFragranceComponents(List<string> ingredients)
    {
        // EU 26 allergens (must be labeled separately) + EU banned allergens
        // These should NOT be recommended as beneficial plant extracts
        var fragranceAllergens = new[] { 
            // EU Banned (since 2021)
            "hydroxyisohexyl 3-cyclohexene carboxaldehyde", "lyral",
            "atranol", "chloroatranol",
            
            // EU 26 allergens
            "alpha-isomethyl ionone", "amyl cinnamal", "amylcinnamyl alcohol",
            "anise alcohol", "benzyl alcohol", "benzyl benzoate", "benzyl cinnamate",
            "benzyl salicylate", "butylphenyl methylpropional", "cinnamal",
            "cinnamyl alcohol", "citral", "citronellol", "coumarin", "eugenol",
            "farnesol", "geraniol", "hexyl cinnamal", "hydroxycitronellal",
            "isoeugenol", "limonene", "linalool", "methyl 2-octynoate",
            "evernia furfuracea", "evernia prunastri"  // Tree moss extracts
        };
        
        return ingredients
            .Where(ing => !fragranceAllergens.Any(allergen => 
                ing.Contains(allergen, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}