const API_URL = 'http://localhost:5166/api/recommend';

document.getElementById('queryForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const query = buildQuery();
    
    if (!validateQuery(query)) return;
    
    // Store query in sessionStorage
    sessionStorage.setItem('skingenQuery', JSON.stringify(query));
    
    // Navigate to results page
    window.location.href = 'results.html';
});

function buildQuery() {
    const productType = document.getElementById('productType').value;
    const concerns = getCheckedValues('#concernsGroup input[type="checkbox"]:checked');
    
    const skinTypeRadio = document.querySelector('input[name="skinType"]:checked');
    const skinType = skinTypeRadio && skinTypeRadio.value ? skinTypeRadio.value : null;
    
    const skinConditionRadio = document.querySelector('input[name="skinCondition"]:checked');
    const skinCondition = skinConditionRadio && skinConditionRadio.value ? skinConditionRadio.value : null;
    const skinConditions = skinCondition ? [skinCondition] : null;
    
    const ingredientGroups = getCheckedValues('#ingredientsGroup input[type="checkbox"]:checked');
    
    const specificIngredientsInput = document.getElementById('specificIngredients').value;
    const specificIngredients = specificIngredientsInput 
        ? specificIngredientsInput.split(',').map(s => s.trim()).filter(s => s) 
        : null;
    
    const blockedCategories = getCheckedValues('#blockedGroup input[type="checkbox"]:checked');
    
    const allergiesInput = document.getElementById('allergies').value;
    const allergies = allergiesInput 
        ? allergiesInput.split(',').map(s => s.trim()).filter(s => s) 
        : null;
    
    return {
        productType,
        concerns,
        skinType,
        skinConditions,
        ingredientGroups: ingredientGroups.length > 0 ? ingredientGroups : null,
        specificIngredients,
        blockedCategories: blockedCategories.length > 0 ? blockedCategories : null,
        allergies
    };
}

function getCheckedValues(selector) {
    return Array.from(document.querySelectorAll(selector)).map(el => el.value);
}

function validateQuery(query) {
    if (!query.productType) {
        alert('Please select a product type');
        return false;
    }
    
    if (query.concerns.length === 0) {
        alert('Please select at least one skin concern');
        return false;
    }
    
    return true;
}