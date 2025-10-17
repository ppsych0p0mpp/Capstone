using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.Service
{
    public class Currency
    {
        public string Code { get; set; }      // e.g. "USD", "PHP"
        public string Symbol { get; set; }    // e.g. "$", "₱"
        public string CultureCode { get; set; } // e.g. "en-US", "en-PH"
        public static List<Currency> SupportedCurrencies => new()
    {
        new Currency { Code = "PHP", Symbol = "₱", CultureCode = "en-PH" },
        new Currency { Code = "USD", Symbol = "$", CultureCode = "en-US" },
        new Currency { Code = "EUR", Symbol = "€", CultureCode = "fr-FR" },
        new Currency { Code = "GBP", Symbol = "£", CultureCode = "en-GB" },  // British Pound
        new Currency { Code = "JPY", Symbol = "¥", CultureCode = "ja-JP" },  // Japanese Yen
        new Currency { Code = "CNY", Symbol = "¥", CultureCode = "zh-CN" },  // Chinese Yuan
        new Currency { Code = "AUD", Symbol = "A$", CultureCode = "en-AU" }, // Australian Dollar
        new Currency { Code = "CAD", Symbol = "C$", CultureCode = "en-CA" }, // Canadian Dollar
        new Currency { Code = "CHF", Symbol = "CHF", CultureCode = "de-CH" },// Swiss Franc
        new Currency { Code = "NZD", Symbol = "NZ$", CultureCode = "en-NZ" }, // New Zealand Dollar
        new Currency { Code = "SEK", Symbol = "kr", CultureCode = "sv-SE" },  // Swedish Krona
        new Currency { Code = "KRW", Symbol = "₩", CultureCode = "ko-KR" },  // South Korean Won
        new Currency { Code = "INR", Symbol = "₹", CultureCode = "hi-IN" },  // Indian Rupee
        new Currency { Code = "MXN", Symbol = "$", CultureCode = "es-MX" },  // Mexican Peso
        new Currency { Code = "ZAR", Symbol = "R", CultureCode = "en-ZA" }   // South African Rand

       
    };
    }
}
