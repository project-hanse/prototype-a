import {ComponentFixture, TestBed} from '@angular/core/testing';

import {ModelsOverviewComponent} from './models-overview.component';

describe('OverviewComponent', () => {
  let component: ModelsOverviewComponent;
  let fixture: ComponentFixture<ModelsOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ModelsOverviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ModelsOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
