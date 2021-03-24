import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PipelineExecutionLogComponent } from './pipeline-execution-log.component';

describe('PipelineExecutionLogComponent', () => {
  let component: PipelineExecutionLogComponent;
  let fixture: ComponentFixture<PipelineExecutionLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PipelineExecutionLogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PipelineExecutionLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
