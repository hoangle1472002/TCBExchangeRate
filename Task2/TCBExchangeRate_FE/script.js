const currencySelect = document.getElementById('currency');
const dateInput = document.getElementById('date');
const table = document.getElementById('ratesTable');
const summary = document.getElementById('summary');
const tbody = table.querySelector('tbody');

const apiBase = 'https://localhost:7299/api';

const currencies = [
  'AUD', 'CAD', 'CHF', 'CNY', 'EUR', 'GBP', 'HKD', 'JPY', 'KRW', 'NZD', 'SGD', 'THB',
  'USD (1.2)', 'USD (5,10,20)', 'USD (50,100)'
];

function loadCurrencies() {
  currencies.forEach(code => {
    const option = document.createElement('option');
    option.value = code;
    option.text = code;
    currencySelect.appendChild(option);
  });
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
  if (value === 0) return '';
  return value != null ? value.toLocaleString() : '-';
}

async function fetchExchangeRates() {
  const date = dateInput.value;
  const currencyCode = currencySelect.value;
  const loader = document.getElementById('loader');

  if (!date || !currencyCode) {
    alert('Please select both a date and currency.');
    return;
  }

  const formattedDate = new Date(date).toISOString().split('T')[0];
  const url = `${apiBase}/ExchangeRate?currencyCode=${encodeURIComponent(currencyCode)}&date=${formattedDate}`;

  try {
    loader.style.display = 'block';
    table.classList.add('hidden');
    summary.classList.add('hidden');
    tbody.innerHTML = '';

    const res = await fetch(url);
    if (res.status === 204) {
      alert('No data found for the selected date and currency.');
      return;
    }

    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();

    if (!data.length) {
      alert('No data found for the selected date and currency.');
      return;
    }

    summary.textContent = `Date: ${formatDateReadable(date)} | Currency: ${currencyCode}`;
    summary.classList.remove('hidden');

    data.forEach(rate => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${formatTime(rate.inputDate)}</td>
        <td>${formatValue(rate.bidRateTM)}</td>
        <td>${formatValue(rate.bidRateCK)}</td>
        <td>${formatValue(rate.askRateTM)}</td>
        <td>${formatValue(rate.askRate)}</td>
      `;
      tbody.appendChild(row);
    });

    table.classList.remove('hidden');
  } catch (err) {
    alert('Error fetching exchange rates.');
    console.error(err);
  } finally {
    loader.style.display = 'none';
  }
}


window.onload = () => {
  const today = new Date().toISOString().split('T')[0];
  dateInput.value = today;
  loadCurrencies();
};