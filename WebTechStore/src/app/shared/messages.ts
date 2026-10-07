/** Textos exibidos ao usuário, reunidos aqui para facilitar ajustes. */
export const MESSAGES = {
  list: {
    loadError: 'Não foi possível carregar os produtos.',
    empty: 'Nenhum produto cadastrado.',
  },
  create: {
    success: 'Produto cadastrado com sucesso',
    error: 'Não foi possível cadastrar o produto',
  },
  update: {
    success: 'Produto alterado com sucesso',
    error: 'Não foi possível alterar o produto',
  },
  delete: {
    confirm: 'Confirma a exclusão do produto selecionado?',
    success: 'Produto selecionado excluído com sucesso',
    error: 'Não foi possível excluir o produto selecionado',
  },
  categories: {
    loadError: 'Não foi possível carregar as categorias',
  },
  validation: {
    categoryRequired: 'Selecione uma categoria.',
    nameRequired: 'Informe o nome do produto.',
    nameMinLength: 'O nome deve ter pelo menos 3 caracteres.',
    nameMaxLength: 'O nome não pode ultrapassar 50 caracteres.',
    priceRequired: 'Informe o preço do produto.',
    priceZero: 'O preço não pode ser igual a zero.',
    priceNegative: 'O preço deve ser maior que zero.',
    priceDecimals: 'O preço deve ter no máximo duas casas decimais.',
    priceMax: 'O preço não pode ser maior que 99.999.999,99.',
  },
} as const;
