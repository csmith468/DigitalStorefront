
export const formatPrice = (price: number, priceType: string) => {
  return priceType + price.toLocaleString();
};