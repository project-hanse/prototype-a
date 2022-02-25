import {ComponentFixture, TestBed} from '@angular/core/testing';

import {PipelineExportComponent} from './pipeline-export.component';

describe('PipelineExportComponent', () => {
	let component: PipelineExportComponent;
	let fixture: ComponentFixture<PipelineExportComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [PipelineExportComponent]
		})
			.compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(PipelineExportComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
