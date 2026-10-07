import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, shareReplay, throwError } from 'rxjs';

import { environment } from '../../environments/environment';
import { Category } from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/categories`;

  private categories$?: Observable<Category[]>;

  /**
   * Lista as categorias. Como elas não mudam por esta aplicação, a resposta fica em
   * cache e o formulário abre sem nova chamada. Em caso de erro o cache é descartado,
   * para a próxima abertura tentar de novo.
   */
  getAll(): Observable<Category[]> {
    this.categories$ ??= this.http.get<Category[]>(this.url).pipe(
      catchError((error: unknown) => {
        this.categories$ = undefined;
        return throwError(() => error);
      }),
      shareReplay(1),
    );

    return this.categories$;
  }
}
