import {ComponentFixture, TestBed} from '@angular/core/testing';

import {PipelineListViewComponent} from './pipeline-list-view.component';

describe('PipelineListViewComponent', () => {
	let component: PipelineListViewComponent;
	let fixture: ComponentFixture<PipelineListViewComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [PipelineListViewComponent]
		})
			.compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(PipelineListViewComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
