const API_URL = 'http://localhost:5166/api/recommend';

window.addEventListener('DOMContentLoaded', async () => {
    const query = JSON.parse(sessionStorage.getItem('skingenQuery'));
    
    if (!query) {
        window.location.href = 'index.html';
        return;
    }
    
    await fetchRecommendations(query);
});

async function fetchRecommendations(query) {
    const resultsGrid = document.getElementById('resultsGrid');
    const resultsStats = document.getElementById('resultsStats');
    
    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(query)
        });
        
        if (!response.ok) throw new Error('Failed to fetch recommendations');
        
        const data = await response.json();
        
        resultsStats.innerHTML = `<i class="bi bi-check-circle"></i> Found ${data.recommendations.length} products from ${data.totalScreened} screened`;
        
        if (data.recommendations.length === 0) {
            resultsGrid.innerHTML = `
                <div class="no-results">
                    <i class="bi bi-emoji-frown" style="font-size: 3rem; color: var(--text-muted);"></i>
                    <h2>No products found</h2>
                    <p>Try adjusting your filters</p>
                </div>
            `;
            return;
        }
        
        resultsGrid.innerHTML = data.recommendations.map((rec, index) => {
            const product = rec.product;
            const score = (rec.score * 100).toFixed(0);
            const exp = rec.explanation;
            const cardId = `card-${index}`;
            
            const ingredientList = Array.isArray(product.ingredient_list) 
                ? product.ingredient_list 
                : (product.ingredient_list || '').split(',').map(i => i.trim()).filter(i => i);
            
            return `
                <div class="product-card">
                    <div class="product-rank">#${rec.rank}</div>
                    
                    <div class="product-score">${score}%</div>
                    
                    <div class="product-header-info">
                        <div class="product-name">${product.name}</div>
                        <div class="product-meta">
                            <span class="product-brand">
                                <i class="bi bi-star-fill"></i> ${product.brand}
                            </span>
                            <span class="product-type">
                                <i class="bi bi-box-seam"></i> ${formatText(product.type)}
                            </span>
                            ${product.country ? `
                                <span class="product-country">
                                    <i class="bi bi-globe"></i> ${product.country}
                                </span>
                            ` : ''}
                        </div>
                    </div>
                    
                    ${generateCompatibilitySection(exp, query)}
                    
                    ${generateWhyMatches(exp)}
                    
                    <div class="collapsible-section">
                        <button class="collapsible-trigger" onclick="toggleSection('${cardId}-details')">
                            <i class="bi bi-chevron-down"></i> View More Details
                        </button>
                        
                        <div id="${cardId}-details" class="collapsible-content">
                            ${exp.all_claims && exp.all_claims.length > 0 ? `
                                <div class="manufacturer-claims">
                                    <h4><i class="bi bi-card-checklist"></i> Manufacturer Claims</h4>
                                    <ul>
                                        ${exp.all_claims.map(claim => `<li><i class="bi bi-tag"></i> ${formatText(claim)}</li>`).join('')}
                                    </ul>
                                </div>
                            ` : ''}
                            
                            ${generateIngredientBreakdown(exp)}
                            
                            ${ingredientList.length > 0 ? `
                                <div class="ingredients-section">
                                    <h4><i class="bi bi-clipboard-data"></i> Full Ingredient List (${ingredientList.length} ingredients)</h4>
                                    <div class="ingredients-list">
                                        ${ingredientList.join(', ')}
                                    </div>
                                </div>
                            ` : ''}
                        </div>
                    </div>
                </div>
            `;
        }).join('');
        
    } catch (error) {
        resultsGrid.innerHTML = `
            <div class="no-results">
                <i class="bi bi-exclamation-circle" style="font-size: 3rem; color: #ef4444;"></i>
                <h2>Error</h2>
                <p>${error.message}</p>
            </div>
        `;
    }
}

function generateCompatibilitySection(exp, query) {
    const hasWarnings = exp.warnings && exp.warnings.length > 0;
    const safetyChecks = exp.safety_checks || {};
    
    // Determine overall compatibility
    let compatibilityStatus = 'compatible';
    let compatibilityMessage = '';
    
    if (query.skinType) {
        if (query.skinType === 'normal_skin') {
            compatibilityStatus = hasWarnings ? 'warning' : 'compatible';
            compatibilityMessage = hasWarnings 
                ? 'Generally suitable, but note considerations below'
                : 'Generally suitable for Normal Skin';
        } else {
            compatibilityStatus = hasWarnings ? 'warning' : 'compatible';
            compatibilityMessage = hasWarnings
                ? `May have concerns for ${formatText(query.skinType)} - see below`
                : `Suitable for ${formatText(query.skinType)}`;
        }
    } else {
        compatibilityStatus = hasWarnings ? 'warning' : 'compatible';
        compatibilityMessage = hasWarnings 
            ? 'Some considerations to note'
            : 'No compatibility issues detected';
    }
    
    return `
        <div class="compatibility-section">
            <h4><i class="bi bi-shield-check"></i> Compatibility & Safety</h4>
            
            <div class="compatibility-status ${compatibilityStatus}">
                <i class="bi bi-${compatibilityStatus === 'compatible' ? 'check-circle-fill' : 'exclamation-triangle-fill'}"></i>
                <span>${compatibilityMessage}</span>
            </div>
            
            <div class="safety-grid">
                <div class="safety-item ${safetyChecks.fragrance_free ? 'safe' : 'unsafe'}">
                    <i class="bi bi-${safetyChecks.fragrance_free ? 'check-circle-fill' : 'x-circle-fill'}"></i>
                    <span>Fragrance-free</span>
                </div>
                <div class="safety-item ${safetyChecks.alcohol_free ? 'safe' : 'unsafe'}">
                    <i class="bi bi-${safetyChecks.alcohol_free ? 'check-circle-fill' : 'x-circle-fill'}"></i>
                    <span>Alcohol-free</span>
                </div>
                <div class="safety-item ${safetyChecks.irritant_free ? 'safe' : 'unsafe'}">
                    <i class="bi bi-${safetyChecks.irritant_free ? 'check-circle-fill' : 'x-circle-fill'}"></i>
                    <span>Irritant-free</span>
                </div>
            </div>
            
            ${hasWarnings ? `
                <div class="warnings-list">
                    <h5><i class="bi bi-info-circle"></i> Things to Consider:</h5>
                    <ul>
                        ${exp.warnings.map(w => `<li>${formatText(w)}</li>`).join('')}
                    </ul>
                </div>
            ` : ''}
        </div>
    `;
}

function generateWhyMatches(exp) {
    const verifiedIngredients = exp.verified_ingredients || {};
    
    if (Object.keys(verifiedIngredients).length === 0) {
        return '';
    }
    
    let html = `
        <div class="why-matches">
            <h4><i class="bi bi-lightbulb"></i> Why This Product Matches</h4>
    `;
    
    for (const [concern, categories] of Object.entries(verifiedIngredients)) {
        if (!categories || Object.keys(categories).length === 0) continue;
        
        html += `
            <div class="concern-match">
                <div class="concern-name"><i class="bi bi-arrow-right-circle"></i> ${formatText(concern)}</div>
        `;
        
        for (const [category, ingredients] of Object.entries(categories)) {
            if (!ingredients || ingredients.length === 0) continue;
            
            html += `
                <div class="ingredient-category">
                    <span class="category-label">${category}:</span>
                    <span class="ingredient-items">${ingredients.join(', ')}</span>
                </div>
            `;
        }
        
        html += `</div>`;
    }
    
    html += `</div>`;
    
    return html;
}

function generateIngredientBreakdown(exp) {
    const breakdown = exp.ingredient_breakdown || {};
    
    if (Object.keys(breakdown).length === 0) {
        return '';
    }
    
    let html = `
        <div class="ingredient-breakdown">
            <h4><i class="bi bi-layers"></i> Complete Ingredient Breakdown</h4>
    `;
    
    for (const [group, categories] of Object.entries(breakdown)) {
        html += `<div class="breakdown-group">`;
        html += `<h5>${group}</h5>`;
        
        for (const [category, ingredients] of Object.entries(categories)) {
            if (!ingredients || ingredients.length === 0) continue;
            
            html += `
                <div class="breakdown-category">
                    <span class="breakdown-label">${category}:</span>
                    <span class="breakdown-items">${ingredients.join(', ')}</span>
                </div>
            `;
        }
        
        html += `</div>`;
    }
    
    html += `</div>`;
    
    return html;
}

function toggleSection(id) {
    const section = document.getElementById(id);
    const trigger = section.previousElementSibling;
    const icon = trigger.querySelector('i');
    
    if (section.classList.contains('expanded')) {
        section.classList.remove('expanded');
        icon.classList.remove('bi-chevron-up');
        icon.classList.add('bi-chevron-down');
        trigger.innerHTML = '<i class="bi bi-chevron-down"></i> View More Details';
    } else {
        section.classList.add('expanded');
        icon.classList.remove('bi-chevron-down');
        icon.classList.add('bi-chevron-up');
        trigger.innerHTML = '<i class="bi bi-chevron-up"></i> Hide Details';
    }
}

function formatText(text) {
    return text.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}