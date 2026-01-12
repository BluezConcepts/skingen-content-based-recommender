using System.Text.Json.Serialization;

namespace SkinGen.API.Models;

public class Product
{
    // Basic info (lowercase to match column names in data source)
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
    
    // Lists (stored as arrays in JSON)
    [JsonPropertyName("ingredient_list")]
    public List<string> IngredientList { get; set; } = new();
    
    [JsonPropertyName("positive_concerns")]
    public List<string> PositiveConcerns { get; set; } = new();
    
    [JsonPropertyName("negative_concerns")]
    public List<string> NegativeConcerns { get; set; } = new();
    
    [JsonPropertyName("condition_concerns")]
    public List<string> ConditionConcerns { get; set; } = new();
    
    // Counts (optional - can ignore these)
    [JsonPropertyName("ingredient_count")]
    public int IngredientCount { get; set; }
    
    [JsonPropertyName("positive_count")]
    public int PositiveCount { get; set; }
    
    [JsonPropertyName("negative_count")]
    public int NegativeCount { get; set; }
    
    [JsonPropertyName("condition_count")]
    public int ConditionCount { get; set; }
    
    // Boolean flags
    [JsonPropertyName("has_fragrance")]
    public bool HasFragrance { get; set; }
    
    [JsonPropertyName("has_drying_alcohol")]
    public bool HasDryingAlcohol { get; set; }
    
    [JsonPropertyName("has_irritants")]
    public bool HasIrritants { get; set; }
    
    [JsonPropertyName("has_vitamin_c")]
    public bool HasVitaminC { get; set; }
    
    [JsonPropertyName("has_hyaluronic_acid")]
    public bool HasHyaluronicAcid { get; set; }
    
    [JsonPropertyName("has_niacinamide")]
    public bool HasNiacinamide { get; set; }
    
    [JsonPropertyName("has_ceramides")]
    public bool HasCeramides { get; set; }
    
    [JsonPropertyName("has_aha")]
    public bool HasAha { get; set; }
    
    [JsonPropertyName("has_bha")]
    public bool HasBha { get; set; }
    
    [JsonPropertyName("has_retinoids")]
    public bool HasRetinoids { get; set; }
    
    [JsonPropertyName("has_peptides")]
    public bool HasPeptides { get; set; }
    
    [JsonPropertyName("has_antioxidants")]
    public bool HasAntioxidants { get; set; }
    
    // Ingredient category lists (stored as JSON strings in parquet, arrays in JSON export)
    [JsonPropertyName("retinoid_list")]
    public List<string> RetinoidList { get; set; } = new();
    
    [JsonPropertyName("peptide_list")]
    public List<string> PeptideList { get; set; } = new();
    
    [JsonPropertyName("antioxidant_list")]
    public List<string> AntioxidantList { get; set; } = new();
    
    [JsonPropertyName("humectant_list")]
    public List<string> HumectantList { get; set; } = new();
    
    [JsonPropertyName("emollient_list")]
    public List<string> EmollientList { get; set; } = new();
    
    [JsonPropertyName("occlusive_list")]
    public List<string> OcclusiveList { get; set; } = new();
    
    [JsonPropertyName("exfoliant_list")]
    public List<string> ExfoliantList { get; set; } = new();
    
    [JsonPropertyName("plant_extract_list")]
    public List<string> PlantExtractList { get; set; } = new();
    
    [JsonPropertyName("absorbent_list")]
    public List<string> AbsorbentList { get; set; } = new();
    
    [JsonPropertyName("film_forming_list")]
    public List<string> FilmFormingList { get; set; } = new();
    
    [JsonPropertyName("texture_enhancer_list")]
    public List<string> TextureEnhancerList { get; set; } = new();
    
    [JsonPropertyName("fragrance_ingredients")]
    public List<string> FragranceIngredients { get; set; } = new();
    
    [JsonPropertyName("irritant_ingredients")]
    public List<string> IrritantIngredients { get; set; } = new();
    
    [JsonPropertyName("drying_alcohol_ingredients")]
    public List<string> DryingAlcoholIngredients { get; set; } = new();
}