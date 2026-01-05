# SkinGen — Content-Based Skincare Recommendation System

SkinGen is a **content-based skincare recommendation system** that provides
relevant and safety-aware product recommendations using **structured product data**
and **ingredient intelligence**.

The system is designed as an **explainable, privacy-first** alternative to
user-tracking recommenders and focuses on transparency rather than black-box predictions.

---

## 1. Project Motivation

The skincare market is highly saturated:
- Products contain 20–50+ ingredients with complex interactions
- Product claims are often vague or marketing-driven
- Users with sensitive skin or conditions face real safety risks
- Most recommendation systems rely on user behavior data

**Goal:**  
Build a recommendation system that:
- works **without user history or ratings**
- is **transparent and explainable**
- explicitly accounts for **safety and skin-type conflicts**
- remains **technically simple and defensible**

---

## 2. Core Design Choices

### 2.1 Content-Based Filtering (Why not Collaborative?)

**Choice:** Content-based recommendation using product features  
**Rejected:** Collaborative filtering

**Why:**
- No reliable user-item interaction data available
- Collaborative methods introduce cold-start problems
- Privacy-first design: no tracking, no accounts
- Product similarity is well-defined via structured metadata

**Result:**  
Products are recommended based on **feature similarity**, not popularity.

---

### 2.2 Product-Level Features over Raw Ingredients

**Choice:** Use **product concern labels** as recommendation features  
**Rejected:** Direct ingredient-level modeling for ranking

**Why:**
- Ingredient coverage is incomplete and noisy
- Many ingredients are fillers or multi-functional
- Product labels already encode domain knowledge
- Ingredient modeling is better suited for explanation than scoring

**Result:**  
A stable and low-dimensional **Feature Profile Matrix** per product.

---

### 2.3 Ingredient Intelligence as Explainability Layer

**Choice:** Separate recommendation logic from ingredient explanation

**Why:**
- Users want to know *why* a product is recommended
- Ingredient lists are difficult to interpret without structure
- Mixing ingredients directly into scoring reduces robustness

**Result:**  
Ingredient intelligence supports **interpretation**, not ranking.

---

## 3. Data Pipeline Overview

### 3.1 Product Dataset

After cleaning and normalization:

- **10,296 skincare products**
- Core product types (serum, cleanser, moisturizer, toner, sunscreen, etc.)
- Full INCI ingredient lists
- Structured concern labels:
  - positive concerns (benefits)
  - negative concerns (side effects)
  - condition concerns (eczema, rosacea)

**Key Insight:**  
43.8% of products combine both *irritating* and *drying* properties, validating
the need for safety-aware recommendation logic.

---

### 3.2 Ingredient Dataset (Paula’s Choice)

**Initial State:**
- Scraped CSV files
- Formatting issues, duplicates, inconsistent fields

**Cleaning & Enrichment:**
- 2,530 unique ingredients
- Structured lists for benefits, categories, info
- Added `functional_group` abstraction

**Functional Groups:**
- Actives
- Support
- Utility
- Sensory
- Risks

**Outcome:**  
Ingredient data became usable for **group-based explanation** and filtering.

---

### 3.3 Ingredient Coverage Improvement

To address incomplete ingredient matching:
- EU allergen lists added
- Pattern matching for plant-based ingredients
- Manual mapping for common compounds

**Coverage Gain:**
- Unique ingredients: ~19% → ~50%
- Per-product coverage: ~77% → ~90%

---

## 4. Recommendation System Implementation

### 4.1 Feature Engineering

Each product is represented by:
- product type
- positive concerns
- negative concerns
- condition concerns
- boolean ingredient flags (e.g. `has_retinoids`, `has_niacinamide`)

**Important Finding:**  
Binary presence features outperform count-based features.

---

### 4.2 Similarity & Ranking

**Similarity Metric:** Cosine similarity  
**Why:**  
- Works well with sparse, high-dimensional binary features
- Scale-independent
- Outperformed Euclidean and Manhattan distance in testing

---

### 4.3 Safety-Aware Scoring Strategy

Instead of strict exclusion:

#### Hard Filters
Applied only for:
- medical contraindications
- explicit user exclusions
- regulatory restrictions

#### Soft Penalties
Applied for:
- skin-type conflicts (drying, irritating)
- condition risks (eczema, rosacea)

Penalties are **cumulative**, ensuring products with multiple conflicts rank lower
without disappearing entirely.

**Design Principle:**  
> Inform the user, don’t decide for them.

---

## 5. Evaluation & Results

Evaluation performed using **synthetic test queries** covering diverse skin profiles.

**Metrics:**
- NDCG@10 ≈ 0.97
- Precision@5 ≈ 1.00
- Recall@5 ≈ 0.96
- Safety Rate ≈ 94%

**Key Findings:**
- Penalty values between 0.8–0.9 balance relevance and safety best
- Count-based weighting adds noise
- Model generalizes well within dataset assumptions

---

## 6. Explainability & User Transparency

Every recommendation includes:
- visible ingredient groups
- clear warnings for sensitivities
- full INCI list (never hidden)

**Design Philosophy:**
> Ingredient data is always shown in full.  
> Interpretations support informed decisions — they do not replace them.

---

## 7. System Architecture

**Backend:**
- C# (.NET)
- In-memory product loading
- Single recommendation endpoint

**Frontend:**
- Vanilla JavaScript + HTML/CSS
- Query → results flow
- Collapsible ingredient sections
- Emphasis on clarity over visual noise

**Deployment:**
- Azure App Service (student tier)
- Always-on configuration for responsiveness

---

## 8. Limitations

- **Reliance on product-level concern labels**:  
  Recommendations depend on manufacturer-provided concern labels, which may be
  incomplete or inconsistent.

- **No ingredient concentration data**:  
  Ingredient presence is known, but not concentration or formulation strength,
  limiting fine-grained ingredient-level reasoning. However most product names do have their *core ingredient* in the name. (eg. Azelaic Acid 10%)

- **No real user feedback loop**:  
  The system does not learn from user interactions, preferences, or outcomes.

- **No collaborative filtering component**:  
  Recommendations are based solely on product similarity, not collective user
  behavior.

- **Deterministic recommendation output**:  
  Identical user inputs will always produce the same Top-N results, as ranking is
  based purely on similarity scores. This ensures reproducibility but may reduce
  perceived variety when users repeat the same queries.

These limitations are intentional trade-offs made to preserve transparency,
interpretability, and a controlled project scope.

---

## 9. Future Work

### 9.1 Ingredient–Claim Correlation & Brand Integrity Analysis (Business Case)

A natural extension of the current system is to **correlate product-level claims
with ingredient-level evidence**.

**Idea:**
- Compare claimed concerns (e.g. “anti-aging”, “brightening”) with the presence of
  supporting ingredient groups (retinoids, peptides, antioxidants, etc.)
- Identify products or brands where claims are weakly supported by ingredients

**Potential Applications:**
- Brand-level “claim integrity” scoring
- Detection of over-promising or under-delivering products
- Consumer-facing transparency tools
- B2B auditing insights for retailers or regulators

This transforms SkinGen from a recommender into an **analytical framework for
marketing vs formulation alignment**.

---

### 9.2 Brand-Specific Filtering & Comparison

Future versions could allow:
- Filtering by brand
- Finding alternatives to a specific product or brand
- Side-by-side comparison of products within the same brand

**Use Cases:**
- “Find products similar to Brand X but fragrance-free”
- “Compare two brands on ingredient quality for sensitive skin”
- Brand portfolio analysis for strengths and weaknesses

---

### 9.3 Regional & Regulatory Filtering (EU / US / Asian Markets)

Products can be filtered or annotated based on **region or country of origin**.

**Motivation:**
- Regulatory differences (EU vs US vs Asia)
- Ingredient availability and restrictions
- Regional formulation philosophies (e.g. K-beauty vs EU pharmacy brands)

**Examples:**
- Filter for EU-compliant products only
- Compare claim accuracy by region
- Highlight ingredients banned or restricted in certain markets

This would enable **region-aware recommendations** and comparative market insights.

---

### 9.4 Advanced Personalization Controls

- Concern importance weighting (primary vs secondary concerns)
- Ingredient inclusion/exclusion rules
- Sensitivity profiles beyond basic skin types

---

### 9.5 Controlled Randomization & Diversity Injection

To improve perceived variety without sacrificing relevance or safety, future
versions could introduce **controlled randomness** in the final ranking stage.

**Possible Approaches:**
- Sample from the top-K highest scoring products instead of always returning
  the top-N
- Randomize ordering within score bands (e.g. all products scoring ≥ 0.9)
- Apply diversity constraints to avoid near-identical formulations

**Design Goal:**
Maintain relevance and safety guarantees while avoiding repetitive results for
identical user inputs.

This would be implemented strictly at the presentation layer, ensuring that
core scoring logic remains deterministic and explainable.

---

### 9.6 Hybrid Recommendation Models

- Combine content-based filtering with user feedback
- Introduce collaborative signals where available
- Maintain explainability as a core constraint

---

## 10. Conclusion

SkinGen demonstrates that **transparent, safety-aware skincare recommendations**
are achievable using structured product data and thoughtful system design.

By separating recommendation logic from ingredient explainability and prioritizing
user autonomy over rigid filtering, the system offers a practical and defensible
alternative to opaque, behavior-driven recommenders.

---

## Medical Disclaimer

Important: SkinGen is an educational tool, not medical advice. Users with skin conditions should consult dermatologists. Ingredient analysis is based on general skincare science and may not account for individual sensitivities.

---

## Author

Elias  
Bachelor — Applied Data Intelligence - Course : Artifical Intelligence & Machine Learning & Forecasting.
Academic Year: 2024–2025
