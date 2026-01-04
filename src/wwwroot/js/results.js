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
        
        resultsStats.textContent = `Found ${data.recommendations.length} products from ${data.totalScreened} screened`;
        
        if (data.recommendations.length === 0) {
            resultsGrid.innerHTML = `
                <div class="no-results">
                    <h2>No products found</h2>
                    <p>Try adjusting your filters</p>
                </div>
            `;
            return;
        }
        
        resultsGrid.innerHTML = data.recommendations.map(rec => {
            const product = rec.product;
            const score = (rec.score * 100).toFixed(0);
            const exp = rec.explanation;
            
            return `
                <div class="product-card">
                    <div class="product-rank">#${rec.rank}</div>
                    <div class="product-score">${score}%</div>
                    <div class="product-name">${product.name}</div>
                    <div class="product-brand">${product.brand} • ${product.type}</div>
                    
                    <div class="product-details">
                        ${exp.matched_concerns && exp.matched_concerns.length > 0 ? `
                            <div class="detail-box">
                                <h4>Matched Concerns</h4>
                                <ul>
                                    ${exp.matched_concerns.map(c => `<li>${formatText(c)}</li>`).join('')}
                                </ul>
                            </div>
                        ` : ''}
                        
                        <div class="detail-box">
                            <h4>Safety Checks</h4>
                            <ul>
                                <li>${exp.safety_checks.fragrance_free ? '✓' : '✗'} Fragrance-free</li>
                                <li>${exp.safety_checks.alcohol_free ? '✓' : '✗'} Alcohol-free</li>
                                <li>${exp.safety_checks.irritant_free ? '✓' : '✗'} Irritant-free</li>
                            </ul>
                        </div>
                    </div>
                    
                    ${exp.warnings && exp.warnings.length > 0 ? `
                        <div class="warning-box">
                            <h4>⚠️ Warnings</h4>
                            <ul>
                                ${exp.warnings.map(w => `<li>${formatText(w)}</li>`).join('')}
                            </ul>
                        </div>
                    ` : ''}
                </div>
            `;
        }).join('');
        
    } catch (error) {
        resultsGrid.innerHTML = `
            <div class="no-results">
                <h2>Error</h2>
                <p>${error.message}</p>
            </div>
        `;
    }
}

function formatText(text) {
    return text.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}