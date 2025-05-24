window.redirectToCheckout = async function (sessionId, publishableKey) {
    const stripe = Stripe(publishableKey);
    await stripe.redirectToCheckout({ sessionId });
};