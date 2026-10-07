/** Corpo de erro da ApiTechStore (Problem Details, RFC 9457). */
export interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
  /** Código do erro de negócio, por exemplo "Product.DuplicateName". */
  code?: string;
  /** Mensagens por campo, presentes nos erros de validação (HTTP 400). */
  errors?: Record<string, string[]>;
}
