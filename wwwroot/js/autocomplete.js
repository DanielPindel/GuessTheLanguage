document.addEventListener('DOMContentLoaded', () =>
{
    initAutocomplete();
    document.body.addEventListener('htmx:afterSwap', (evt) =>
    {
        if (evt.detail.target.querySelector('#guessInput, #languageDropdown'))
        {
            initAutocomplete();
        }
    });
});

function initAutocomplete()
{
    const input = document.getElementById('guessInput');
    const dropdown = document.getElementById('languageDropdown');
    const form = document.getElementById('guessForm');
    const hiddenIdInput = document.getElementById('selectedLanguageId');

    if (!input || !dropdown || !form)
    {
        console.log('Autocomplete elements not found');
        return;
    }

    input.value = '';
    input.focus();
    dropdown.style.display = 'none';
    let highlightedIndex = -1;
    let visibleItems = [];

    input.addEventListener('input', updateDropdown);
    input.addEventListener('focus', updateDropdown);
    input.addEventListener('keydown', handleKeyDown);
    dropdown.addEventListener('click', handleDropdownClick);
    document.addEventListener('click', closeIfOutside);

    function updateDropdown()
    {
        const searchTerm = input.value.toLowerCase().trim();
        visibleItems = [];
        const exactStartMatches = [];
        const otherMatches = [];

        dropdown.querySelectorAll('.autocomplete-item').forEach((item, index) =>
        {
            const languageName = item.dataset.value.toLowerCase();
            const searchData = item.dataset.search.toLowerCase();

            const matches = searchData.includes(searchTerm);

            if (matches)
            {
                if (languageName.startsWith(searchTerm))
                {
                    exactStartMatches.push(item);
                }
                else
                {
                    otherMatches.push(item);
                }

                item.style.display = '';
                item.classList.remove('highlighted');

                item.onmouseenter = () =>
                {
                    visibleItems.forEach(el => el.classList.remove('highlighted'));
                    item.classList.add('highlighted');
                    highlightedIndex = visibleItems.indexOf(item);
                };
            }
            else
            {
                item.style.display = 'none';
            }
        });

        visibleItems = [...exactStartMatches, ...otherMatches];
        visibleItems.forEach(item => item.parentNode.appendChild(item));

        dropdown.style.display = visibleItems.length ? 'block' : 'none';
        highlightedIndex = -1;
    }

    function handleKeyDown(e)
    {
        const actions = {
            ArrowDown: () => highlightItem(1),
            ArrowUp: () => highlightItem(-1),
            Enter: () =>
            {
                if (highlightedIndex >= 0) selectItem(visibleItems[highlightedIndex]);
                else if (visibleItems.length) selectItem(visibleItems[0]);
                else form.requestSubmit();
            },
            Escape: () => dropdown.style.display = 'none'
        };

        if (actions[e.key])
        {
            e.preventDefault();
            actions[e.key]();
        }
    }

    function highlightItem(step)
    {
        if (!visibleItems.length) return;
        highlightedIndex = (highlightedIndex + step + visibleItems.length) % visibleItems.length;

        visibleItems.forEach((item, i) =>
        {
            item.classList.toggle('highlighted', i === highlightedIndex);
            if (i === highlightedIndex) item.scrollIntoView({ block: 'nearest' });
        });
    }

    function handleDropdownClick(e)
    {
        const item = e.target.closest('.autocomplete-item');
        if (item)
        {
            e.preventDefault();
            selectItem(item);
        }
    }

    function closeIfOutside(e)
    {
        if (!input.contains(e.target) && !dropdown.contains(e.target))
        {
            dropdown.style.display = 'none';
        }
    }

    function selectItem(item)
    {
        input.value = item.dataset.value || item.dataset.display;
        hiddenIdInput.value = item.dataset.id;
        dropdown.style.display = 'none';
        setTimeout(() => form.requestSubmit(), 50);
    }
}
