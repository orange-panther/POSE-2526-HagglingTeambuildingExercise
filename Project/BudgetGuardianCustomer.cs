namespace haggling_interfaces;

public class BudgetGuardianCustomer : Customer
{
    private readonly int MinBargainRounds = Random.Shared.Next(3, 7);
    private const decimal GuardianCloseEnough = 0.02m;

    protected override OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        if (Patience == 0) return OfferDecision.Decline;

        bool likes = Likes.Contains(offer.Product.Type);
        bool mustHave = MustHaves != null && MustHaves.Contains(offer.Product.Type);
        bool dislikes = Dislikes.Contains(offer.Product.Type);

        if (dislikes || offer.Price > Budget || Patience < LowPatienceThreshold)
            return OfferDecision.Decline;

        if (CounterOffersMade < MinBargainRounds)
            return OfferDecision.Counter;

        if (LastCustomerOffer != null)
        {
            var gap = Math.Abs(offer.Price - LastCustomerOffer.Price);
            var rel = LastCustomerOffer.Price > 0 ? (gap / LastCustomerOffer.Price) : 1.0m;
            if (rel <= GuardianCloseEnough && offer.Price <= LastCustomerOffer.Price)
                return OfferDecision.Accept;
        }

        const decimal MustHaveTight = 0.65m;
        const decimal LikeTight = 0.55m;
        if (mustHave && offer.Price <= Budget * MustHaveTight) return OfferDecision.Accept;
        if (likes && offer.Price <= Budget * LikeTight) return OfferDecision.Accept;

        return OfferDecision.Counter;
    }

    protected override IOffer CreateOffer(IProduct product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (LastVendorOffer == null) throw new InvalidOperationException();

        var offer = new CustomerOffer
        {
            Product = product,
            OfferedBy = PersonType.Customer,
            Status = OfferStatus.Ongoing
        };

        decimal vendor = LastVendorOffer.Price;

        if (LastCustomerOffer == null)
        {
            decimal firstCut = 0.60m;
            if (MustHaves != null && MustHaves.Contains(product.Type)) firstCut = 0.75m;
            else if (Likes.Contains(product.Type)) firstCut = 0.70m;

            decimal baseCushion = (MustHaves != null && MustHaves.Contains(product.Type)) ? 0.05m : 0.10m;
            int rounds = 1;
            decimal shrinkByRounds = 1m - 0.12m * (rounds - 1);
            if (shrinkByRounds < 0.2m) shrinkByRounds = 0.2m;
            decimal patienceScale = 0.7m + 0.3m * ((decimal)Patience / 100m);
            decimal cushion = baseCushion * shrinkByRounds * patienceScale;
            if (cushion < 0.01m) cushion = 0.01m;

            decimal cap = R2(vendor * (1m - cushion));

            decimal first = R2(vendor * firstCut);
            if (first > cap) first = cap;

            decimal minOffer = MinMeaningfulOfferFor(vendor);
            first = Math.Min(first, Budget);
            first = Math.Max(first, minOffer);

            offer.Price = R2(first);
            CounterOffersMade++;
            return offer;
        }

        decimal prev = LastCustomerOffer.Price;

        decimal elasticityFactor = (decimal)Elasticity / 100m;
        decimal patienceFactor = 1 - (decimal)Patience / 100m;
        decimal stepFactor = 0.20m + 0.20m * elasticityFactor + 0.15m * patienceFactor;
        if (stepFactor < 0.20m) stepFactor = 0.20m;
        if (stepFactor > 0.60m) stepFactor = 0.60m;

        decimal baseCushion2 = (MustHaves != null && MustHaves.Contains(product.Type)) ? 0.05m : 0.10m;
        int rounds2 = Math.Max(1, CounterOffersMade + 1);
        decimal shrinkByRounds2 = 1m - 0.12m * (rounds2 - 1);
        if (shrinkByRounds2 < 0.2m) shrinkByRounds2 = 0.2m;
        decimal patienceScale2 = 0.7m + 0.3m * ((decimal)Patience / 100m);
        decimal cushion2 = baseCushion2 * shrinkByRounds2 * patienceScale2;
        if (cushion2 < 0.01m) cushion2 = 0.01m;

        decimal target = R2(vendor * (1m - cushion2));

        decimal gap = target - prev;
        decimal next = prev + gap * stepFactor;

        if (next < prev) next = prev;
        if (next > target) next = target;
        if (next > Budget) next = Budget;

        decimal minOffer2 = MinMeaningfulOfferFor(vendor);
        if (next < minOffer2) next = minOffer2;

        offer.Price = R2(next);
        CounterOffersMade++;
        return offer;
    }

    protected override void UpdatePatience(IOffer newVendorOffer)
    {
        base.UpdatePatience(newVendorOffer);
        int recover = (int)Math.Round((100 - (int)Patience) * 0.05);
        Patience = Math.Clamp((int)Patience + recover, 0, 100);
    }
}
