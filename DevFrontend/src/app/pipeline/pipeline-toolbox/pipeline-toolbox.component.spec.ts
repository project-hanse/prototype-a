import {ComponentFixture, TestBed} from '@angular/core/testing';

import {PipelineToolboxComponent} from './pipeline-toolbox.component';

describe('PipelineToolboxComponent', () => {
	let component: PipelineToolboxComponent;
	let fixture: ComponentFixture<PipelineToolboxComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [PipelineToolboxComponent]
		})
			.compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(PipelineToolboxComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
