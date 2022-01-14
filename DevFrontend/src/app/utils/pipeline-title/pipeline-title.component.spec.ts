import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PipelineTitleComponent } from './pipeline-title.component';

describe('PipelineTitleComponent', () => {
  let component: PipelineTitleComponent;
  let fixture: ComponentFixture<PipelineTitleComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PipelineTitleComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PipelineTitleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
