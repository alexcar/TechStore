import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, ViewChild, inject, output, signal } from '@angular/core';
import { FormBuilder, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelect, MatSelectModule } from '@angular/material/select';

import { ApiProblem } from '../../models/api-problem.model';
import { Category } from '../../models/category.model';
import { Product, ProductInput } from '../../models/product.model';
import { CategoryService } from '../../services/category.service';
import { ProductService } from '../../services/product.service';
import { MESSAGES } from '../../shared/messages';
import { productNameValidator, productPriceValidator } from '../../shared/product-validators';

export interface ProductFormDialogData {
  /** Produto em alteração, ou `null` para incluir um novo. */
  product: Product | null;
}

interface DialogToast {
  type: 'success' | 'error';
  message: string;
}

type FieldName = 'categoryId' | 'name' | 'price';

const TOAST_DURATION_MS = 5000;

/** Pop-up com o formulário de inclusão e de alteração de produto. */
@Component({
  selector: 'app-product-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './product-form-dialog.component.html',
  styleUrl: './product-form-dialog.component.scss',
})
export class ProductFormDialogComponent implements OnInit, OnDestroy {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject<MatDialogRef<ProductFormDialogComponent>>(MatDialogRef);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly data = inject<ProductFormDialogData>(MAT_DIALOG_DATA);

  @ViewChild(FormGroupDirective) private formDirective?: FormGroupDirective;
  @ViewChild(MatSelect) private categorySelect?: MatSelect;

  /** Emitido a cada gravação com sucesso, para a página atualizar o grid. */
  readonly saved = output<Product>();

  /** Produto em alteração. Fica `null` na inclusão. */
  protected readonly product = this.data.product;
  protected readonly isEdit = this.product !== null;
  protected readonly title = this.isEdit ? 'Alterar produto' : 'Incluir produto';

  protected readonly categories = signal<Category[]>([]);
  protected readonly categoriesFailed = signal(false);
  protected readonly saving = signal(false);
  protected readonly toast = signal<DialogToast | null>(null);

  protected readonly form = this.formBuilder.group({
    categoryId: this.formBuilder.control<number | null>(this.product?.categoryId ?? null, Validators.required),
    name: this.formBuilder.nonNullable.control(this.product?.name ?? '', productNameValidator),
    price: this.formBuilder.control<number | null>(this.product?.price ?? null, productPriceValidator),
  });

  private toastTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({
      next: (categories) => this.categories.set(this.orderCategories(categories)),
      error: () => {
        this.categoriesFailed.set(true);
        this.showToast('error', MESSAGES.categories.loadError);
      },
    });
  }

  ngOnDestroy(): void {
    clearTimeout(this.toastTimer);
  }

  protected save(): void {
    if (this.saving()) {
      return;
    }

    // Valida todos os campos ao salvar: os inválidos passam a exibir a mensagem
    // de erro e o formulário não é enviado ao backend.
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const { categoryId, name, price } = this.form.getRawValue();
    const input: ProductInput = {
      categoryId: categoryId!,
      name: name.trim(),
      price: Number(price),
    };

    const request$ = this.product
      ? this.productService.update(this.product.id, input)
      : this.productService.create(input);

    this.saving.set(true);

    request$.subscribe({
      next: (product) => {
        this.saving.set(false);
        this.saved.emit(product);

        if (this.isEdit) {
          // Alteração: os dados permanecem no formulário.
          this.showToast('success', MESSAGES.update.success);
        } else {
          // Inclusão: limpa o formulário para o próximo cadastro.
          this.resetForm();
          this.showToast('success', MESSAGES.create.success);
        }
      },
      error: (error: HttpErrorResponse) => {
        // Os dados permanecem nos campos para o usuário corrigir e tentar de novo.
        this.saving.set(false);
        this.applyServerErrors(error);
        this.showToast('error', this.isEdit ? MESSAGES.update.error : MESSAGES.create.error);
      },
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  /** Campo preenchido corretamente e já visitado: recebe a borda verde. */
  protected isValid(field: FieldName): boolean {
    const control = this.form.controls[field];
    return control.valid && (control.touched || control.dirty);
  }

  /** Mensagem específica da regra violada no campo. */
  protected errorMessage(field: FieldName): string {
    const errors = this.form.controls[field].errors;
    if (!errors) {
      return '';
    }

    // Mensagem devolvida pela API (por exemplo, nome já cadastrado).
    if (typeof errors['server'] === 'string') {
      return errors['server'];
    }

    const validation = MESSAGES.validation;

    switch (field) {
      case 'categoryId':
        return validation.categoryRequired;
      case 'name':
        if (errors['minlength']) return validation.nameMinLength;
        if (errors['maxlength']) return validation.nameMaxLength;
        return validation.nameRequired;
      case 'price':
        if (errors['zero']) return validation.priceZero;
        if (errors['negative']) return validation.priceNegative;
        if (errors['decimals']) return validation.priceDecimals;
        if (errors['max']) return validation.priceMax;
        return validation.priceRequired;
    }
  }

  /**
   * Inclusão: a lista vem em ordem alfabética, depois do item "Selecione".
   * Alteração: a categoria já cadastrada no produto vem em primeiro lugar.
   */
  private orderCategories(categories: Category[]): Category[] {
    const sorted = [...categories].sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'));

    if (!this.product) {
      return sorted;
    }

    const currentId = this.product.categoryId;
    return [...sorted.filter((c) => c.id === currentId), ...sorted.filter((c) => c.id !== currentId)];
  }

  private resetForm(): void {
    const emptyValue = { categoryId: null, name: '', price: null };

    // resetForm() do FormGroupDirective também zera o estado "enviado"; sem isso os
    // campos recém-limpos apareceriam com erro.
    if (this.formDirective) {
      this.formDirective.resetForm(emptyValue);
    } else {
      this.form.reset(emptyValue);
    }

    this.categorySelect?.focus();
  }

  /** Exibe nos campos os erros devolvidos pela API (400 de validação ou 409 de nome repetido). */
  private applyServerErrors(error: HttpErrorResponse): void {
    const problem = error.error as ApiProblem | null;
    if (!problem || typeof problem !== 'object') {
      return;
    }

    if (error.status === 409 && problem.detail) {
      this.setServerError('name', problem.detail);
      return;
    }

    const fieldByApiName: Record<string, FieldName> = {
      categoryid: 'categoryId',
      name: 'name',
      price: 'price',
    };

    for (const [apiField, messages] of Object.entries(problem.errors ?? {})) {
      const field = fieldByApiName[apiField.toLowerCase()];
      if (field && messages.length > 0) {
        this.setServerError(field, messages[0]);
      }
    }
  }

  private setServerError(field: FieldName, message: string): void {
    const control = this.form.controls[field];
    // O erro some sozinho quando o usuário altera o campo, pois o Angular revalida o controle.
    control.setErrors({ server: message });
    control.markAsTouched();
  }

  /** Toast no canto superior direito do pop-up. */
  private showToast(type: DialogToast['type'], message: string): void {
    clearTimeout(this.toastTimer);
    this.toast.set({ type, message });
    this.toastTimer = setTimeout(() => this.toast.set(null), TOAST_DURATION_MS);
  }
}
