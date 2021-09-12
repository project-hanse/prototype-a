import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PipelineGraphComponent } from './pipeline-graph.component';

describe('PipelineNodeViewComponent', () => {
  let component: PipelineGraphComponent;
  let fixture: ComponentFixture<PipelineGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PipelineGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PipelineGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
