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
        new Currency { Code = "PHP - Philippine Peso", Symbol = "₱", CultureCode = "en-PH" },
        new Currency { Code = "USD - US Dollar", Symbol = "$", CultureCode = "en-US" },
        new Currency { Code = "EUR - Euro", Symbol = "€", CultureCode = "fr-FR" },
        new Currency { Code = "GBP - British Pound", Symbol = "£", CultureCode = "en-GB" },  // British Pound
        new Currency { Code = "JPY - Japanese Yen", Symbol = "¥", CultureCode = "ja-JP" },  // Japanese Yen
        new Currency { Code = "CNY - Chinese Yuan", Symbol = "¥", CultureCode = "zh-CN" },  // Chinese Yuan
        new Currency { Code = "AUD - Australian Dollar", Symbol = "A$", CultureCode = "en-AU" }, // Australian Dollar
        new Currency { Code = "CAD - Canadian Dollar", Symbol = "C$", CultureCode = "en-CA" }, // Canadian Dollar
        new Currency { Code = "CHF - Swiss Franc", Symbol = "CHF", CultureCode = "de-CH" },// Swiss Franc
        new Currency { Code = "NZD - New Zealand Dollar", Symbol = "NZ$", CultureCode = "en-NZ" }, // New Zealand Dollar
        new Currency { Code = "SEK - Swedish Krona", Symbol = "kr", CultureCode = "sv-SE" },  // Swedish Krona
        new Currency { Code = "KRW - South Korean Won", Symbol = "₩", CultureCode = "ko-KR" },  // South Korean Won
        new Currency { Code = "INR - Indian Rupee", Symbol = "₹", CultureCode = "hi-IN" },  // Indian Rupee
        new Currency { Code = "MXN - Mexican Peso", Symbol = "$", CultureCode = "es-MX" },  // Mexican Peso
        new Currency { Code = "ZAR - South African Rand", Symbol = "R", CultureCode = "en-ZA" }   // South African Rand

       
    };
    }
}
