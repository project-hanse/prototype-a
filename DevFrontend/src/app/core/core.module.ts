import {CommonModule} from '@angular/common';
import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatCardModule} from '@angular/material/card';
import {MatDividerModule} from '@angular/material/divider';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import {MatListModule} from '@angular/material/list';
import {MatMenuModule} from '@angular/material/menu';
import {MatProgressBarModule} from '@angular/material/progress-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatSelectModule} from '@angular/material/select';
import {MatTabsModule} from '@angular/material/tabs';
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
		FormsModule,
		MatSelectModule,
		MatProgressSpinnerModule,
		MatTabsModule,
		MatMenuModule,
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
		FormsModule,
		MatSelectModule,
		MatProgressSpinnerModule,
		MatTabsModule,
		MatMenuModule,
	]
})
export class CoreModule {
}
