import {ComponentFixture, TestBed} from '@angular/core/testing';

import {NodeResultPreviewComponent} from './node-result-preview.component';

describe('NodeResultPreviewComponent', () => {
	let component: NodeResultPreviewComponent;
	let fixture: ComponentFixture<NodeResultPreviewComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [NodeResultPreviewComponent]
		})
			.compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(NodeResultPreviewComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
