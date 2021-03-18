import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PipelineNodeViewComponent } from './pipeline-node-view.component';

describe('PipelineNodeViewComponent', () => {
  let component: PipelineNodeViewComponent;
  let fixture: ComponentFixture<PipelineNodeViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PipelineNodeViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PipelineNodeViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
