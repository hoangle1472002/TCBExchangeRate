const currencySelect = document.getElementById('currency');
const dateInput = document.getElementById('date');
const table = document.getElementById('ratesTable');
const summary = document.getElementById('summary');
const tbody = table.querySelector('tbody');

const apiBase = 'https://localhost:7138/api';

async function loadCurrencies() {
  try {
    const res = await fetch(`${apiBase}/currencies`);
    const json = await res.json();
    const data = json.data;

    data.forEach(currency => {
      const option = document.createElement('option');
      option.value = currency.code;
      option.text = currency.code;
      currencySelect.appendChild(option);
    });
  } catch (err) {
    alert('Failed to load currencies');
  }
}

function formatTime(datetime) {
  const dt = new Date(datetime);
  return dt.toUTCString().split(' ')[4];
}

function formatDateReadable(inputDate) {
  const date = new Date(inputDate);
  return date.toLocaleDateString('en-GB');
}

function formatValue(value) {
  return value === 0 ? '' : value?.toLocaleString() ?? '-';
}

async function fetchExchangeRates() {
  const date = dateInput.value;
  const currencyCode = currencySelect.value;

  if (!date) {
    alert('Please select a date.');
    return;
  }

  const formattedDate = new Date(date).toISOString().split('T')[0];
  const url = `${apiBase}/exchangerate/snapshots?date=${formattedDate}&currencyCode=${currencyCode}`;

  try {
    const res = await fetch(url);
    const json = await res.json();
    const data = json.data;

    tbody.innerHTML = '';

    if (!data.length) {
      table.classList.add('hidden');
      summary.classList.add('hidden');
      alert('No data found for the selected date and currency.');
      return;
    }

    summary.textContent = `Date: ${formatDateReadable(date)}${currencyCode ? ` | Currency: ${currencyCode}` : ''}`;
    summary.classList.remove('hidden');

    data.forEach(rate => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${formatTime(rate.snapshotTime)}</td>
        <td>${formatValue(rate.purchaseCashCheque)}</td>
        <td>${formatValue(rate.purchaseTransfer)}</td>
        <td>${formatValue(rate.sellingCashCheque)}</td>
        <td>${formatValue(rate.sellingTransfer)}</td>
      `;
      tbody.appendChild(row);
    });

    table.classList.remove('hidden');
  } catch (err) {
    alert('Error fetching exchange rates.');
    console.error(err);
  }
}

// Set today's date as default
window.onload = () => {
  const today = new Date().toISOString().split('T')[0];
  dateInput.value = today;
  loadCurrencies();
};
