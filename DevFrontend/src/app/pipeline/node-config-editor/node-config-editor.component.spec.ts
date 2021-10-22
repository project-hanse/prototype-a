import {ComponentFixture, TestBed} from '@angular/core/testing';

import {NodeConfigEditorComponent} from './node-config-editor.component';

describe('NodeConfigEditorComponent', () => {
	let component: NodeConfigEditorComponent;
	let fixture: ComponentFixture<NodeConfigEditorComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [NodeConfigEditorComponent]
		})
			.compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(NodeConfigEditorComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
