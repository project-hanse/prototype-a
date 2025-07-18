import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';

import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {DevAuthInterceptor} from './core/_interceptors/dev-auth.interceptor';
import {CoreModule} from './core/core.module';
import {DevToolsModule} from './dev-tools/dev-tools.module';

@NgModule({
	declarations: [
		AppComponent
	],
	imports: [
		BrowserModule,
		HttpClientModule,
		AppRoutingModule,
		BrowserAnimationsModule,
		CoreModule,
		DevToolsModule
	],
	providers: [
		{
			provide: HTTP_INTERCEPTORS,
			useClass: DevAuthInterceptor,
			multi: true
		}
	],
	bootstrap: [AppComponent]
})
export class AppModule {
}
