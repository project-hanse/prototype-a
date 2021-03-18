import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatDividerModule} from '@angular/material/divider';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';


@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    MatDividerModule,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule
  ],
  exports: [
    MatDividerModule,
    MatToolbarModule,
    MatButtonModule,
    MatCardModule
  ]
})
export class CoreModule {
}
