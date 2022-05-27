import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CandidateManagerComponent } from './candidate-manager.component';

describe('CandidateManagerComponent', () => {
  let component: CandidateManagerComponent;
  let fixture: ComponentFixture<CandidateManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CandidateManagerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CandidateManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
