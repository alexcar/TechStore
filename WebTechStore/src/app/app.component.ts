import { Component } from '@angular/core';

import { ProductListComponent } from './products/product-list/product-list.component';

/** A aplicação tem uma única página: o grid de produtos. */
@Component({
  selector: 'app-root',
  imports: [ProductListComponent],
  template: '<app-product-list />',
})
export class AppComponent {}
