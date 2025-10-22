export interface ProductType {
    productTypeId: number;
    typeName: string;
    typeCode: string;
    description: string | null;
}

export interface PriceType {
    priceTypeId: number;
    priceTypeName: string;
    priceTypeCode: string;
    icon: string;
}