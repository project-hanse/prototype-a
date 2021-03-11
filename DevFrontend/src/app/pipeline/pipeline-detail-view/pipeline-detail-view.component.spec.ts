import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PipelineDetailViewComponent } from './pipeline-detail-view.component';

describe('PipelineDetailViewComponent', () => {
  let component: PipelineDetailViewComponent;
  let fixture: ComponentFixture<PipelineDetailViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PipelineDetailViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PipelineDetailViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
