import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const NAME_MIN_LENGTH = 3;
export const NAME_MAX_LENGTH = 50;

/** Maior valor aceito pela coluna DECIMAL(10,2) do banco. */
export const PRICE_MAX = 99_999_999.99;

/**
 * Regras do nome: obrigatório, de 3 a 50 caracteres.
 * Espaços no início e no fim não contam, porque a API os remove antes de gravar.
 */
export const productNameValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = String(control.value ?? '').trim();

  if (value.length === 0) {
    return { required: true };
  }

  if (value.length < NAME_MIN_LENGTH) {
    return { minlength: true };
  }

  if (value.length > NAME_MAX_LENGTH) {
    return { maxlength: true };
  }

  return null;
};

/** Regras do preço: obrigatório, diferente de zero, positivo, com até duas casas decimais. */
export const productPriceValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const raw: unknown = control.value;

  if (raw === null || raw === undefined || raw === '') {
    return { required: true };
  }

  const value = Number(raw);

  if (Number.isNaN(value)) {
    return { required: true };
  }

  if (value === 0) {
    return { zero: true };
  }

  if (value < 0) {
    return { negative: true };
  }

  if (value > PRICE_MAX) {
    return { max: true };
  }

  // Compara em centavos para não esbarrar na imprecisão de ponto flutuante.
  const cents = value * 100;
  if (Math.abs(cents - Math.round(cents)) > 1e-6) {
    return { decimals: true };
  }

  return null;
};
