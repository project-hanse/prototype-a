import {ComponentFixture, TestBed} from '@angular/core/testing';

import {StatusBarComponent} from './status-bar.component';

describe('BarComponent', () => {
	let component: StatusBarComponent;
	let fixture: ComponentFixture<StatusBarComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [StatusBarComponent]
		})
			.compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(StatusBarComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
