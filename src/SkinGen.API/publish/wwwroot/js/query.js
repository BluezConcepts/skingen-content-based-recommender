const API_URL =
  window.location.hostname === 'localhost'
    ? 'http://localhost:5166/api/recommend'
    : 'https://skingen-elias-hwahbbhwbbhca9a9.francecentral-01.azurewebsites.net/api/recommend';

// Store tags
let specificIngredientsTags = [];
let allergiesTags = [];

document.addEventListener('DOMContentLoaded', () => {
    // Setup tag inputs
    setupTagInput('specificIngredients', 'specificIngredientsTags', specificIngredientsTags);
    setupTagInput('allergies', 'allergiesTags', allergiesTags);
});

function setupTagInput(inputId, containerId, tagsArray) {
    const input = document.getElementById(inputId);
    const container = document.getElementById(containerId);
    
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            
            const value = input.value.trim();
            if (value && !tagsArray.includes(value)) {
                // Add to array
                tagsArray.push(value);
                
                // Create tag element
                const tag = document.createElement('div');
                tag.className = 'tag';
                tag.innerHTML = `
                    <span>${value}</span>
                    <span class="tag-remove" onclick="removeTag('${inputId}', '${containerId}', '${value}')">×</span>
                `;
                container.appendChild(tag);
                
                // Clear input
                input.value = '';
            }
        }
    });
}

function removeTag(inputId, containerId, value) {
    // Determine which array to use
    const tagsArray = inputId === 'specificIngredients' ? specificIngredientsTags : allergiesTags;
    
    // Remove from array
    const index = tagsArray.indexOf(value);
    if (index > -1) {
        tagsArray.splice(index, 1);
    }
    
    // Re-render tags
    renderTags(containerId, tagsArray, inputId);
}

function renderTags(containerId, tagsArray, inputId) {
    const container = document.getElementById(containerId);
    container.innerHTML = '';
    
    tagsArray.forEach(value => {
        const tag = document.createElement('div');
        tag.className = 'tag';
        tag.innerHTML = `
            <span>${value}</span>
            <span class="tag-remove" onclick="removeTag('${inputId}', '${containerId}', '${value}')">×</span>
        `;
        container.appendChild(tag);
    });
}

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
    
    // Use the tags arrays instead of parsing input
    const specificIngredients = specificIngredientsTags.length > 0 ? specificIngredientsTags : null;
    
    const blockedCategories = getCheckedValues('#blockedGroup input[type="checkbox"]:checked');
    
    // Use the tags arrays instead of parsing input
    const allergies = allergiesTags.length > 0 ? allergiesTags : null;
    
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