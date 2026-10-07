import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import { Product } from '../../models/product.model';
import { ProductService } from '../../services/product.service';
import { MESSAGES } from '../../shared/messages';
import {
  ConfirmDeleteDialogComponent,
  ConfirmDeleteDialogData,
  ConfirmDeleteDialogResult,
} from '../confirm-delete-dialog/confirm-delete-dialog.component';
import {
  ProductFormDialogComponent,
  ProductFormDialogData,
} from '../product-form-dialog/product-form-dialog.component';

/** Página principal: grid de produtos com as ações Incluir, Alterar e Excluir. */
@Component({
  selector: 'app-product-list',
  imports: [CurrencyPipe, MatButtonModule, MatProgressBarModule, MatTableModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly messages = MESSAGES;
  protected readonly displayedColumns = ['name', 'categoryName', 'price', 'actions'];

  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadFailed = signal(false);

  /** Linha selecionada pelo usuário (destaque azul claro). */
  protected readonly selectedId = signal<number | null>(null);

  /** Linha aguardando confirmação de exclusão (destaque vermelho claro). */
  protected readonly deletingId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadProducts();
  }

  protected loadProducts(): void {
    this.loading.set(true);
    this.loadFailed.set(false);

    this.productService.getAll().subscribe({
      next: (products) => {
        this.products.set(this.sortByName(products));
        this.loading.set(false);
      },
      error: () => {
        this.loadFailed.set(true);
        this.loading.set(false);
      },
    });
  }

  protected select(product: Product): void {
    this.selectedId.set(product.id);
  }

  protected openCreate(): void {
    this.openForm({ product: null });
  }

  protected openEdit(product: Product): void {
    this.selectedId.set(product.id);
    this.openForm({ product });
  }

  protected confirmDelete(product: Product): void {
    this.selectedId.set(product.id);
    this.deletingId.set(product.id);

    const dialogRef = this.dialog.open<
      ConfirmDeleteDialogComponent,
      ConfirmDeleteDialogData,
      ConfirmDeleteDialogResult
    >(ConfirmDeleteDialogComponent, {
      width: '440px',
      maxWidth: '95vw',
      data: { product },
    });

    dialogRef.afterClosed().subscribe((result) => {
      this.deletingId.set(null);

      if (result === 'deleted') {
        this.selectedId.set(null);
        this.showToast(MESSAGES.delete.success, 'success');
        // Atualiza o grid buscando a lista de novo. Um recarregamento completo do
        // navegador apagaria o toast antes de o usuário conseguir ler a mensagem.
        this.loadProducts();
      } else if (result === 'error') {
        this.showToast(MESSAGES.delete.error, 'error');
      }
    });
  }

  private openForm(data: ProductFormDialogData): void {
    const dialogRef = this.dialog.open<ProductFormDialogComponent, ProductFormDialogData>(
      ProductFormDialogComponent,
      {
        width: '480px',
        maxWidth: '95vw',
        // O pop-up só fecha pelo botão Cancelar, para não perder dados por um clique fora.
        disableClose: true,
        data,
      },
    );

    // O pop-up continua aberto depois de salvar, então o grid é atualizado a cada gravação.
    const subscription = dialogRef.componentInstance.saved.subscribe(() => this.loadProducts());
    dialogRef.afterClosed().subscribe(() => subscription.unsubscribe());
  }

  /** Toast no canto superior direito da página. */
  private showToast(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, undefined, {
      duration: 5000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['toast', `toast-${type}`],
    });
  }

  private sortByName(products: Product[]): Product[] {
    return [...products].sort((a, b) => a.name.localeCompare(b.name, 'pt-BR', { sensitivity: 'base' }));
  }
}
