import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';

@Injectable()
export class DevAuthInterceptor implements HttpInterceptor {

	constructor() {
	}

	intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
		if (!environment.production) {
			return next.handle(request.clone({headers: request.headers.append('Authorization', 'Basic dXNlcm5hbWU6ZmFrZXBhc3N3b3Jk')}));
		}
		return next.handle(request);
	}
}
