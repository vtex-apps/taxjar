using System;
using System.Collections.Generic;
using System.Text;

namespace Taxjar.Data
{
    public class TaxjarConstants
    {
        public const string EndPointKey = "taxjar";
        public const string AppName = "taxjar";

        public const string FORWARDED_HEADER = "X-Forwarded-For";
        public const string FORWARDED_HOST = "X-Forwarded-Host";
        public const string APPLICATION_JSON = "application/json";
        public const string HEADER_VTEX_CREDENTIAL = "X-Vtex-Credential";
        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string PROXY_AUTHORIZATION_HEADER_NAME = "Proxy-Authorization";
        public const string USE_HTTPS_HEADER_NAME = "X-Vtex-Use-Https";
        public const string PROXY_TO_HEADER_NAME = "X-Vtex-Proxy-To";
        public const string VTEX_ACCOUNT_HEADER_NAME = "X-Vtex-Account";
        public const string ENVIRONMENT = "vtexcommercestable";
        public const string LOCAL_ENVIRONMENT = "myvtex";
        public const string VTEX_ID_HEADER_NAME = "VtexIdclientAutCookie";
        public const string HEADER_VTEX_WORKSPACE = "X-Vtex-Workspace";
        public const string APP_SETTINGS = "vtex.taxjar";
        public const string ACCEPT = "Accept";
        public const string CONTENT_TYPE = "Content-Type";
        public const string MINICART = "application/vnd.vtex.checkout.minicart.v1+json";
        public const string HTTP_FORWARDED_HEADER = "HTTP_X_FORWARDED_FOR";
        public const string API_VERSION_HEADER = "'x-api-version";
        public const string TAXJAR_API_VERSION = "2020-08-07";

        public const string BUCKET = "TaxJar";
        public const string CACHE_BUCKET = "TaxJarCache";

        public const string PLUGIN = "vtex";

        public const string PICKUP_TAG = "TaxJar";

        public const string ERROR_INDICATOR = "error-detail:";

        public class ExemptionType
        {
            public const string WHOLESALE = "wholesale";
            public const string GOVERNMENT = "government";
            public const string OTHER = "other";
            public const string NON_EXEMPT = "non_exempt";
        }

        public class VtexOrderStatus
        {
            public const string OrderCreated = "order-created";
            public const string OrderCompleted = "order-completed";
            public const string OnOrderCompleted = "on-order-completed";
            public const string PaymentPending = "payment-pending";
            public const string WaitingForOrderAuthorization = "waiting-for-order-authorization";
            public const string ApprovePayment = "approve-payment";
            public const string PaymentApproved = "payment-approved";
            public const string PaymentDenied = "payment-denied";
            public const string RequestCancel = "request-cancel";
            public const string WaitingForSellerDecision = "waiting-for-seller-decision";
            public const string AuthorizeFullfilment = "authorize-fulfillment";
            public const string OrderCreateError = "order-create-error";
            public const string OrderCreationError = "order-creation-error";
            public const string WindowToCancel = "window-to-cancel";
            public const string ReadyForHandling = "ready-for-handling";
            public const string StartHanding = "start-handling";
            public const string Handling = "handling";
            public const string InvoiceAfterCancellationDeny = "invoice-after-cancellation-deny";
            public const string OrderAccepted = "order-accepted";
            public const string Invoice = "invoice";
            public const string Invoiced = "invoiced";
            public const string Replaced = "replaced";
            public const string CancellationRequested = "cancellation-requested";
            public const string Cancel = "cancel";
            public const string Canceled = "canceled";
            public const string Cancelled = "cancelled";
        }

        public class Domain
        {
            public const string Fulfillment = "Fulfillment";
            public const string Marketplace = "Marketplace";
        }

        public static Dictionary<string, string> CountryCodesMapping = new Dictionary<string, string>()
        {
           { "AFG", "AF" },    // Afghanistan
           { "ALB", "AL" },    // Albania
           { "ARE", "AE" },    // U.A.E.
           { "ARG", "AR" },    // Argentina
           { "ARM", "AM" },    // Armenia
           { "AUS", "AU" },    // Australia
           { "AUT", "AT" },    // Austria
           { "AZE", "AZ" },    // Azerbaijan
           { "BEL", "BE" },    // Belgium
           { "BGD", "BD" },    // Bangladesh
           { "BGR", "BG" },    // Bulgaria
           { "BHR", "BH" },    // Bahrain
           { "BIH", "BA" },    // Bosnia and Herzegovina
           { "BLR", "BY" },    // Belarus
           { "BLZ", "BZ" },    // Belize
           { "BOL", "BO" },    // Bolivia
           { "BRA", "BR" },    // Brazil
           { "BRN", "BN" },    // Brunei Darussalam
           { "CAN", "CA" },    // Canada
           { "CHE", "CH" },    // Switzerland
           { "CHL", "CL" },    // Chile
           { "CHN", "CN" },    // People's Republic of China
           { "COL", "CO" },    // Colombia
           { "CRI", "CR" },    // Costa Rica
           { "CZE", "CZ" },    // Czech Republic
           { "DEU", "DE" },    // Germany
           { "DNK", "DK" },    // Denmark
           { "DOM", "DO" },    // Dominican Republic
           { "DZA", "DZ" },    // Algeria
           { "ECU", "EC" },    // Ecuador
           { "EGY", "EG" },    // Egypt
           { "ESP", "ES" },    // Spain
           { "EST", "EE" },    // Estonia
           { "ETH", "ET" },    // Ethiopia
           { "FIN", "FI" },    // Finland
           { "FRA", "FR" },    // France
           { "FRO", "FO" },    // Faroe Islands
           { "GBR", "GB" },    // United Kingdom
           { "GEO", "GE" },    // Georgia
           { "GRC", "GR" },    // Greece
           { "GRL", "GL" },    // Greenland
           { "GTM", "GT" },    // Guatemala
           { "HKG", "HK" },    // Hong Kong S.A.R.
           { "HND", "HN" },    // Honduras
           { "HRV", "HR" },    // Croatia
           { "HUN", "HU" },    // Hungary
           { "IDN", "ID" },    // Indonesia
           { "IND", "IN" },    // India
           { "IRL", "IE" },    // Ireland
           { "IRN", "IR" },    // Iran
           { "IRQ", "IQ" },    // Iraq
           { "ISL", "IS" },    // Iceland
           { "ISR", "IL" },    // Israel
           { "ITA", "IT" },    // Italy
           { "JAM", "JM" },    // Jamaica
           { "JOR", "JO" },    // Jordan
           { "JPN", "JP" },    // Japan
           { "KAZ", "KZ" },    // Kazakhstan
           { "KEN", "KE" },    // Kenya
           { "KGZ", "KG" },    // Kyrgyzstan
           { "KHM", "KH" },    // Cambodia
           { "KOR", "KR" },    // Korea
           { "KWT", "KW" },    // Kuwait
           { "LAO", "LA" },    // Lao P.D.R.
           { "LBN", "LB" },    // Lebanon
           { "LBY", "LY" },    // Libya
           { "LIE", "LI" },    // Liechtenstein
           { "LKA", "LK" },    // Sri Lanka
           { "LTU", "LT" },    // Lithuania
           { "LUX", "LU" },    // Luxembourg
           { "LVA", "LV" },    // Latvia
           { "MAC", "MO" },    // Macao S.A.R.
           { "MAR", "MA" },    // Morocco
           { "MCO", "MC" },    // Principality of Monaco
           { "MDV", "MV" },    // Maldives
           { "MEX", "MX" },    // Mexico
           { "MKD", "MK" },    // Macedonia (FYROM)
           { "MLT", "MT" },    // Malta
           { "MNE", "ME" },    // Montenegro
           { "MNG", "MN" },    // Mongolia
           { "MYS", "MY" },    // Malaysia
           { "NGA", "NG" },    // Nigeria
           { "NIC", "NI" },    // Nicaragua
           { "NLD", "NL" },    // Netherlands
           { "NOR", "NO" },    // Norway
           { "NPL", "NP" },    // Nepal
           { "NZL", "NZ" },    // New Zealand
           { "OMN", "OM" },    // Oman
           { "PAK", "PK" },    // Islamic Republic of Pakistan
           { "PAN", "PA" },    // Panama
           { "PER", "PE" },    // Peru
           { "PHL", "PH" },    // Republic of the Philippines
           { "POL", "PL" },    // Poland
           { "PRI", "PR" },    // Puerto Rico
           { "PRT", "PT" },    // Portugal
           { "PRY", "PY" },    // Paraguay
           { "QAT", "QA" },    // Qatar
           { "ROU", "RO" },    // Romania
           { "RUS", "RU" },    // Russia
           { "RWA", "RW" },    // Rwanda
           { "SAU", "SA" },    // Saudi Arabia
           { "SCG", "CS" },    // Serbia and Montenegro (Former)
           { "SEN", "SN" },    // Senegal
           { "SGP", "SG" },    // Singapore
           { "SLV", "SV" },    // El Salvador
           { "SRB", "RS" },    // Serbia
           { "SVK", "SK" },    // Slovakia
           { "SVN", "SI" },    // Slovenia
           { "SWE", "SE" },    // Sweden
           { "SYR", "SY" },    // Syria
           { "TAJ", "TJ" },    // Tajikistan
           { "THA", "TH" },    // Thailand
           { "TKM", "TM" },    // Turkmenistan
           { "TTO", "TT" },    // Trinidad and Tobago
           { "TUN", "TN" },    // Tunisia
           { "TUR", "TR" },    // Turkey
           { "TWN", "TW" },    // Taiwan
           { "UKR", "UA" },    // Ukraine
           { "URY", "UY" },    // Uruguay
           { "USA", "US" },    // United States
           { "UZB", "UZ" },    // Uzbekistan
           { "VEN", "VE" },    // Bolivarian Republic of Venezuela
           { "VNM", "VN" },    // Vietnam
           { "YEM", "YE" },    // Yemen
           { "ZAF", "ZA" },    // South Africa
           { "ZWE", "ZW" },    // Zimbabwe
        };
    }
}
