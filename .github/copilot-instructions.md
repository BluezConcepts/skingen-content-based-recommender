# GitHub Copilot / Agent Instructions — skinGen Content-Based Recommender

## Purpose ✅
This repository contains the data and notebooks for a content-based skincare product recommender (data cleaning and EDA). Focus of immediate work: data cleaning, ingredient parsing, product categorization, and preparing feature representations for content-based similarity.

## Quick start ⚡
- Open and run the notebook: `notebooks/01_product_data_eda_and_cleaning.ipynb` (Jupyter / VSCode Notebook) to explore current data-cleaning steps.
- Raw CSVs live under `data/raw/` (key files: `datasheet.csv`, `COSING_Ingredients-Fragrance Inventory_v2.csv`, and `paulas_choice_*.csv`).
- Typical imports used in notebooks: `pandas`, `numpy`, `matplotlib`.

Example (PowerShell):
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install pandas numpy matplotlib jupyterlab
jupyter lab
```

Note: There is no `requirements.txt` in the repo currently — if you add one, update this doc.

## Key patterns & conventions 🔧
- Column renaming is done explicitly and preserved in working copies:
  - `ingridients` -> `inci_raw`
  - `afterUse` -> `claimed_concerns_raw`
  - See: `notebooks/01_product_data_eda_and_cleaning.ipynb` (renaming cell).

- Keep a copy before mutating dataframes (common pattern):
```python
df = raw_df.copy()
```

- Product-type filtering uses two constant lists defined in the notebook:
  - `SKINCARE_TYPES` — broad list used to filter the dataset initially.
  - `CORE_SKINCARE_TYPES` — narrow list used for core-recommendation product types.

Example filtering flow (notebook):
```python
df = df[df['type'].isin(SKINCARE_TYPES)].copy()
# later
df = df[df['type'].isin(CORE_SKINCARE_TYPES)].copy()
```

- Exploratory diagnostics: `df.shape`, `value_counts()`, and a `column_overview` DataFrame are used frequently to reason about missingness and cardinality.

## Data notes & expectations 🗂️
- `data/raw/datasheet.csv` is the primary dataset for products used in EDA & cleaning.
- Ingredients data may need normalization using `COSING_Ingredients-...` for standard names.
- Text columns often contain raw human-entered values (e.g., `type`, ingredient lists); expect inconsistent casing/typos and run normalization before matching.

## Development workflows & debugging 🐞
- Use the notebook interactively and add small test cells that assert expectations (e.g., check unique counts, ensure renamed columns exist).
- Useful checks to add when changing cleaning code:
  - assert `"inci_raw" in df.columns`
  - `assert df.shape[0] > 0` after filtering
  - `df['type'].value_counts().head()` to check expected categories

## What to look for when making changes 🧭
- Avoid changing column names used elsewhere without updating the notebook references.
- Preserve the two-stage filtering logic (`SKINCARE_TYPES` then `CORE_SKINCARE_TYPES`) unless intentionally broadening scope — document the reasoning in the notebook.
- If adding new dependencies, add `requirements.txt` and update this doc.

## Integration & external dependencies 🔗
- No external services or CI scripts were found. Work is currently local and notebook-driven.
- Future ML pipelines would likely ingest the cleaned CSV output produced from the notebooks.

## Examples of inline patterns to reuse ✍️
- Use uppercase for list-of-choices constants, e.g., `SKINCARE_TYPES`, `CORE_SKINCARE_TYPES`.
- Use `pd.set_option("display.max_columns", None)` and `pd.set_option("display.max_colwidth", 200)` for exploratory cells.

---

If anything above is unclear or you want additional automation (e.g., a `make` file, `requirements.txt`, or a script to export cleaned CSVs), tell me which direction you prefer and I will iterate. ✨

Please review and point out any missing facts or other files I should reference.