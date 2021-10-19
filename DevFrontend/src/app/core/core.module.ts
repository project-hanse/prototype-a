import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import {MatDividerModule} from '@angular/material/divider';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatListModule} from '@angular/material/list';
import {MatProgressBarModule} from '@angular/material/progress-bar';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatTooltipModule} from '@angular/material/tooltip';


@NgModule({
	declarations: [],
	imports: [
		CommonModule,
		MatDividerModule,
		MatToolbarModule,
		MatButtonModule,
		MatCardModule,
		MatListModule,
		MatProgressBarModule,
		MatIconModule,
		MatTooltipModule,
		MatInputModule,
		FormsModule
	],
	exports: [
		MatDividerModule,
		MatToolbarModule,
		MatButtonModule,
		MatCardModule,
		MatListModule,
		MatProgressBarModule,
		MatIconModule,
		MatTooltipModule,
		MatInputModule,
		FormsModule
	]
})
export class CoreModule {
}
