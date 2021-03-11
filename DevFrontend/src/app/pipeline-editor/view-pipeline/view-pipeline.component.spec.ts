import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewPipelineComponent } from './view-pipeline.component';

describe('ViewPipelineComponent', () => {
  let component: ViewPipelineComponent;
  let fixture: ComponentFixture<ViewPipelineComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ViewPipelineComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewPipelineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
