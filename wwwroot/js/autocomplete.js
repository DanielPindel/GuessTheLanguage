function initAutocomplete()
{
    const input = document.getElementById('guessInput');
    const dropdown = document.getElementById('languageDropdown');
    const hiddenInput = document.getElementById('selectedLanguageId');
    const items = document.querySelectorAll('.autocomplete-item');

    if (!input || !dropdown) return;

    input.addEventListener('input', function ()
    {
        const value = this.value.toLowerCase();
        let hasMatches = false;

        items.forEach(item =>
        {
            const text = item.getAttribute('data-value').toLowerCase();
            if (text.includes(value))
            {
                item.style.display = '';
                hasMatches = true;
            } else
            {
                item.style.display = 'none';
            }
        });

        dropdown.style.display = hasMatches ? 'block' : 'none';
    });

    input.addEventListener('focus', function ()
    {
        if (dropdown.querySelectorAll('.autocomplete-item:not([style*="display: none"])').length > 0)
        {
            dropdown.style.display = 'block';
        }
    });

    document.addEventListener('click', function (e)
    {
        if (!input.contains(e.target) && !dropdown.contains(e.target))
        {
            dropdown.style.display = 'none';
        }
    });

    items.forEach(item =>
    {
        item.addEventListener('click', function ()
        {
            input.value = this.getAttribute('data-value');
            hiddenInput.value = this.getAttribute('data-id');
            dropdown.style.display = 'none';
        });
    });

    input.addEventListener('keydown', function (e)
    {
        if (e.key === 'Enter' && dropdown.style.display === 'block')
        {
            e.preventDefault();
            const visibleItems = dropdown.querySelectorAll('.autocomplete-item:not([style*="display: none"])');
            if (visibleItems.length > 0)
            {
                visibleItems[0].click();
            }
        }
    });

    // Add form validation to ensure a language is selected
    const form = document.getElementById('guessForm');
    if (form)
    {
        form.addEventListener('submit', function (e)
        {
            if (!hiddenInput.value)
            {
                e.preventDefault();
                alert('Please select a language from the dropdown');
                input.focus();
            }
        });
    }
}

// Initialize autocomplete when the page loads
document.addEventListener('DOMContentLoaded', initAutocomplete);

// Reinitialize autocomplete when HTMX loads new content
document.addEventListener('htmx:afterSwap', function ()
{
    initAutocomplete();
});