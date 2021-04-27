📢 Use this project, [contribute](https://github.com/vtex-apps/ship-station) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).

<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->

[![All Contributors](https://img.shields.io/badge/all_contributors-0-orange.svg?style=flat-square)](#contributors-)

<!-- ALL-CONTRIBUTORS-BADGE:END -->

# TaxJar

The TaxJar app uses the TaxJar API to calculate taxes.  Optionally, invoiced orders will be recorded in TaxJar for reporting.

## Configuration

### Step 1 - Create API Token in TaxJar

- [Sign up for TaxJar](https://app.taxjar.com/api_sign_up) 
- [How do I get a TaxJar sales tax API token?](https://support.taxjar.com/article/160-how-do-i-get-a-taxjar-sales-tax-api-token)

### Step 2 - Install the TaxJar app

Using your terminal, log in to the desired VTEX account and run the following command:
`vtex install vtex.taxjar`

### Step 3 - Defining the app settings

1. In the VTEX admin, under Account Settings, choose Apps, then My Apps
2. On the TaxJar app, choose Settings
3. Enter the API Token.
4. Choose optional settings
- **Production** - The entered API Token is a Live token.
- **Enable Tax Calculations** - Request tax calculations.
- **Enable Transaction Posting** - Record invoiced orders in TaxJar
5. Save Settings

### Notes

- Warehouse locations are used to determine nexus addresses.
- Product tax codes are entered in Catalog -> Products and SKUs -> Tax Code.
- An order transaction is created in TaxJar when an order is invoiced.

## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<!-- markdownlint-enable -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
