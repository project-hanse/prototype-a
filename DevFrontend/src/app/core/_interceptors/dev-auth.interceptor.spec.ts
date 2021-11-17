import {TestBed} from '@angular/core/testing';

import {DevAuthInterceptor} from './dev-auth.interceptor';

describe('DevAuthInterceptor', () => {
	beforeEach(() => TestBed.configureTestingModule({
		providers: [
			DevAuthInterceptor
		]
	}));

	it('should be created', () => {
		const interceptor: DevAuthInterceptor = TestBed.inject(DevAuthInterceptor);
		expect(interceptor).toBeTruthy();
	});
});
