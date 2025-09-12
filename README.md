# Haggling

## Core Interfaces

### ICustomer

Represents a customer participating in haggling negotiations.

**Properties:**

-   `string Name { get; init; }` - The customer's name
-   `int Age { get; init; }` - The customer's age
-   `Percentage Patience { get; set; }` - The customer's patience level expressed as a Percentage. This value can be used to determine how willing the customer is to continue haggling before accepting an offer or walking away.

**Methods:**

#### ChooseProduct

Allows the customer to choose a product from the specified vendor.

**Parameters:**

-   `vendor` - The vendor from whom the product is to be chosen. To get the list of products, use the property `IVendor.Products`.

**Returns:**
The `IProduct` selected by the customer.

#### RespondToOffer

Called when another party makes an offer to this customer. Implementations should examine the `offer`, the initiating `vendor` and return a modified counter-offer. To accept the offer, set the `IOffer.Status` to `OfferStatus.Accepted`. To stop the trade, set the `IOffer.Status` to `OfferStatus.Stopped`. To make a counter-offer, set (or keep) the `IOffer.Status` to `OfferStatus.Ongoing` and adjust the `IOffer.Price` accordingly. Also ensure to set the `IOffer.OfferedBy` to `PersonType.Customer`.

**Parameters:**

-   `offer` - The incoming offer to respond to.
-   `vendor` - The vendor who made the offer.

**Returns:**
An `IOffer` representing this customer's response as a counter-offer.

#### AcceptTrade

Finalize and accept the agreed trade represented by `offer`. Implementations should perform any state changes necessary to complete the transaction between this customer and vendor. e.g.: updating inventory, adjusting balances, resetting the patience, etc.

**Parameters:**

-   `offer` - The offer that has been accepted.

#### StopTrade

Stop the current trade negotiation with the vendor. Implementations should reset any state related to the trade.

---

### IVendor

Represents a vendor participating in haggling negotiations.

**Properties:**

-   `string Name { get; init; }` - The vendor's name
-   `int Age { get; init; }` - The vendor's age
-   `Percentage Patience { get; set; }` - The vendor's patience level as a Percentage. This can be used to determine how many counter-offers the vendor will make before accepting or stopping negotiations.
-   `IProduct[] Products { get; init; }` - Array of products offered by this vendor

**Methods:**

#### GetStartingOffer

Create a starting `IOffer` for the specified `product` when interacting with a `customer`. Implementations should set an initial `IOffer.Price` and the `IOffer.OfferedBy` accordingly. The offer's status should be set to `OfferStatus.Ongoing` to indicate that negotiations are in progress.

**Parameters:**

-   `product` - The product for which to create a starting offer.
-   `customer` - The customer the offer is for.

**Returns:**
A new `IOffer` representing the vendor's opening position.

#### RespondToOffer

Called when another party makes an offer to this vendor. Implementations should examine the `offer`, the initiating `customer` and return a modified counter-offer. To accept the offer, set the `IOffer.Status` to `OfferStatus.Accepted`. To stop the trade, set the `IOffer.Status` to `OfferStatus.Stopped`. To make a counter-offer, set (or keep) the `IOffer.Status` to `OfferStatus.Ongoing` and adjust the `IOffer.Price` accordingly. Also ensure to set the `IOffer.OfferedBy` to `PersonType.Vendor`.

**Parameters:**

-   `offer` - The incoming offer to respond to.
-   `customer` - The customer who made the offer.

**Returns:**
An `IOffer` representing this vendor's response as a counter-offer.

#### AcceptTrade

Finalize and accept the agreed trade represented by `offer`. Implementations should perform any state changes necessary to complete the transaction between this customer and vendor. e.g.: updating inventory, adjusting balances, resetting the patience, etc.

**Parameters:**

-   `offer` - The offer that has been accepted.

#### StopTrade

Stop the current trade negotiation with the customer. Implementations should reset any state related to the trade.

---

### IProduct

Represents a product that can be traded.

**Properties:**

-   `string Name { get; init; }` - The product's name
-   `ProductType Type { get; init; }` - The category/type of the product
-   `Percentage Rarity { get; set; }` - A measure of how rare the product is expressed as a Percentage. Implementations may use this to influence starting prices or customer interest.

---

### IOffer

Represents an offer made during negotiations.

**Properties:**

-   `OfferStatus Status { get; set; }` - Current status of the offer
-   `IProduct Product { get; set; }` - The product being offered
-   `decimal Price { get; set; }` - The proposed price
-   `PersonType OfferedBy { get; set; }` - Who made this offer

---

### IDisplay

Handles the presentation/visualization of the haggling process.

**Methods:**

#### ShowProducts

Render the provided list of `IProduct`s for the given `IVendor` and `ICustomer`. Implementations decide how products are presented to the customer (console, GUI, logs, etc.).

**Parameters:**

-   `products` - The products available from the vendor.
-   `vendor` - The vendor offering the products.
-   `customer` - The customer viewing the products.

#### ShowOffer

Display the current `IOffer` in the context of a negotiation between the specified `IVendor` and `ICustomer`. Implementations should present the offer details and any relevant state (e.g.: who made the offer, price, product) to the user or system.

**Parameters:**

-   `offer` - The offer to display.
-   `vendor` - The vendor participating in the negotiation.
-   `customer` - The customer participating in the negotiation.

## Supporting Types

### Percentage

A small value type that represents a percentage constrained between 0 and 100. The type provides implicit conversions to and from `int` for convenience while guaranteeing the value remains clamped.

**Properties:**

-   `int Value { get; set; }` - The integer value of the percentage in the range [0, 100].

**Constructors:**

-   `Percentage(int value)` - Create a percentage from an integer value (automatically clamped to [0, 100])

**Implicit Operators:**

-   `static implicit operator Percentage(int value)` - Create a Percentage from an integer. The value will be clamped into the valid [0, 100] range.
-   `static implicit operator int(Percentage clamped)` - Convert the Percentage back to an int.

### PersonType (Enum)

Identifies who is making an offer:

-   `Customer` - The offer is made by a customer
-   `Vendor` - The offer is made by a vendor

### OfferStatus (Enum)

Represents the current state of an offer:

-   `Accepted` - The offer has been accepted and the trade should complete.
-   `Stopped` - The negotiation was stopped and no trade will occur.
-   `Ongoing` - The negotiation is ongoing; parties may make counter-offers.

### ProductType (Enum)

Categories of products available for trading:

-   `Food`
-   `Electronics`
-   `Clothing`
-   `Furniture`
-   `Toys`
-   `Books`
-   `Tools`
-   `SportsEquipment`
-   `Jewelry`
-   `BeautyProducts`
