# UI and Database Plan

This document describes the user interface and database section of the Space Shooter project.

## Responsible Member

Member 2 is responsible for the UI, shop, options, about form, SQLite database and documentation.

## Main Forms

- MainMenuForm
- GameForm
- ShopForm
- OptionsForm
- AboutForm

## Database Class

- DatabaseManager

## UI Responsibilities

The UI section includes:

- Main menu
- Play button
- Shop button
- Options button
- About button
- Quit button
- HUD display
- Score display
- Coins display
- Wave display
- Health display

## Shop Responsibilities

The shop section includes:

- Showing total coins
- Showing purchasable items
- Buying items
- Equipping purchased items
- Saving shop data permanently

## SQLite Responsibilities

The database section stores:

- TotalCoins
- HighScore
- PurchasedItems
- EquippedShipSkin
- EquippedBulletStyle
- EquippedBackground

## Documentation Responsibilities

The documentation section includes:

- README file
- Coding rules
- Setup instructions
- Gameplay instructions
- Explanation of SQLite storage

## Design Note

The main storage system of this project is SQLite.
JSON files are not used as the main database of the game.
