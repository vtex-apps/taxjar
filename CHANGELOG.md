# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.6.0] - 2022-06-10

### Changed

 - Updated x-api-version to 2022-01-24

## [0.5.1] - 2022-05-24

- (TAXJAR-49) When setting exemption state, changed label for all regions from 'N/A' to 'All'

## [0.5.0] - 2022-05-05

### Added

- (TAXJAR-50) Post marketplace transactions to TaxJar

## [0.4.17] - 2022-03-16

### Fixed

- Fixed response index error

## [0.4.16] - 2022-03-14

### Added

- Log tax request

## [0.4.15] - 2022-02-07

### Fixed

- (TAXJAR-46) Fixed splitting taxes by sku

## [0.4.14] - 2022-02-03

### Fixed

- Allowed null values of the state field in customer exemptions

### Added

- Quality engineering GitHub Actions

## [0.4.13] - 2021-12-07

### Fixed

- Fix customer exemptions loading 

## [0.4.12] - 2021-12-06

### Changed

 - Added query to verify that an email address has an Id

## [0.4.11] - 2021-12-01

### Fixed

 - Do not apply tax to free items

### Changed

 - Restored caching
 - Do not add all tax categories if zero

## [0.4.10] - 2021-11-30

### Fixed

 - Do not skip gifts when calculating discount

## [0.4.9] - 2021-11-30

### Fixed

 - Tax allocation when splitting line items

## [0.4.8] - 2021-11-30

### Fixed

 - Splitting line items

## [0.4.7] - 2021-11-29

### Changed

 - Added logging
 - Disabled caching

## [0.4.6] - 2021-11-29

### Changed

 - Use price instead of listprice for discount
 - Ignore gifts

## [0.4.5] - 2021-11-29

### Changed

 - Added logging

## [0.4.4] - 2021-11-29

### Fixed

- Error calculating discount amount from orderform

## [0.4.3] - 2021-11-29

### Fixed

- Json deserialization error

### Changed

 - Changed log level for Discount Mismatch from Error to Warn

## [0.4.2] - 2021-11-27

### Changed

 - Combine / Split like skus

## [0.4.1] - 2021-11-26

### Changed

 - Only get the discount from the orderform when there is a mismatch in the tax request

## [0.4.0] - 2021-11-26

### Changed

- Load discount amount from orderform

## [0.3.1] - 2021-11-17

### Changed

- Changed cache key format

## [0.3.0] - 2021-11-15

### Added

- Front end validation for customer exemptions
- Better error message when an invalid customer email is used

## [0.2.0] - 2021-11-10

### Added

- Log when the tax hub request item discount amount does not match order discount
- Query to retrive users

## [0.1.12] - 2021-10-29

### Changed

- Check for zero dollar orders.

## [0.1.11] - 2021-10-28

### Changed

- Do not apply adjustment to zero value fields

## [0.1.10] - 2021-10-27

### Fixed

- always return full response

## [0.1.9] - 2021-10-25

### Fixed

- TAXJAR-34 Ensure tax adjustment does not reduce tax below zero

## [0.1.8] - 2021-10-21

### Added

- Handle zero value items
- Error handling

## [0.1.7] - 2021-10-13

### Fixed

- Error when reading `customerList.length`

## [0.1.6] - 2021-10-07

### Fixed

- Error when applying adjustment

## [0.1.5] - 2021-09-29

### Added

- Test Connection and Save Settings feedback Alerts
- App store meta data improvements

## [0.1.4] - 2021-09-27

### Added

- Dropdowns for customer exemption creation
- Can add up to 3 exemption locations

### Fixed

- Customer exemptions not saving properly

## [0.1.3] - 2021-08-20

### Fixed

- Shipping sim on cart page

## [0.1.2] - 2021-08-11

### Fixed

- Split location

## [0.1.1] - 2021-08-10

### Changed

- App store changes
- Missing dock not fatal

## [0.1.0] - 2021-08-05

### Added

- Customer exemption interface in the admin panel

## [0.0.13] - 2021-07-12

### Added

- Filter sales channels

## [0.0.12] - 2021-06-28

### Fixed

- Handle missing Dock address

## [0.0.11] - 2021-06-25

### Added

- Process refunds

## [0.0.10] - 2021-06-24

### Added

- Added screenshots, metadata messages, and an icon

## [0.0.9] - 2021-06-16

### Added

- testing address validation and refunds

## [0.0.8] - 2021-05-25

### Changed

- Changed sign up link to affiliate link
- removed 'amount' field

## [0.0.7] - 2021-05-21

### Changed

- For nexus, only use pickup points tagged 'TaxJar'

## [0.0.6] - 2021-05-19

### Changed

- Default use nexus settings from TaxJar

## [0.0.5] - 2021-05-14

### Added

- Added option to use Nexus settings from TaxJar

## [0.0.4] - 2021-05-11

## [0.0.3] - 2021-05-11

## [0.0.2] - 2021-05-10

### Added

- Admin page

## [0.0.1] - 2021-04-29

### Added

- Initial version
