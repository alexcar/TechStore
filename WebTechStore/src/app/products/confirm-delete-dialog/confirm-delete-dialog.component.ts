import { Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import { Product } from '../../models/product.model';
import { ProductService } from '../../services/product.service';
import { MESSAGES } from '../../shared/messages';

export interface ConfirmDeleteDialogData {
  product: Product;
}

/**
 * Resultado do pop-up: 'deleted' quando a API excluiu o produto, 'error' quando a
 * exclusão falhou. Fica indefinido quando o usuário respondeu "Não" ou fechou o pop-up.
 */
export type ConfirmDeleteDialogResult = 'deleted' | 'error' | undefined;

/** Pop-up de confirmação da exclusão. Ao responder "Sim", chama a API e só então fecha. */
@Component({
  selector: 'app-confirm-delete-dialog',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './confirm-delete-dialog.component.html',
  styleUrl: './confirm-delete-dialog.component.scss',
})
export class ConfirmDeleteDialogComponent {
  private readonly dialogRef =
    inject<MatDialogRef<ConfirmDeleteDialogComponent, ConfirmDeleteDialogResult>>(MatDialogRef);
  private readonly productService = inject(ProductService);

  protected readonly data = inject<ConfirmDeleteDialogData>(MAT_DIALOG_DATA);
  protected readonly messages = MESSAGES;
  protected readonly deleting = signal(false);

  protected confirm(): void {
    if (this.deleting()) {
      return;
    }

    this.deleting.set(true);
    // Enquanto a exclusão está em andamento o pop-up não pode ser fechado por Esc ou clique fora.
    this.dialogRef.disableClose = true;

    this.productService.delete(this.data.product.id).subscribe({
      next: () => this.dialogRef.close('deleted'),
      error: () => this.dialogRef.close('error'),
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
