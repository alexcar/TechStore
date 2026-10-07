/** Produto devolvido pela ApiTechStore. */
export interface Product {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  price: number;
}

/** Dados enviados à ApiTechStore para incluir ou alterar um produto. */
export interface ProductInput {
  categoryId: number;
  name: string;
  price: number;
}
