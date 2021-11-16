import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilesOverviewComponent } from './files-overview.component';

describe('FilesOverviewComponent', () => {
  let component: FilesOverviewComponent;
  let fixture: ComponentFixture<FilesOverviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FilesOverviewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FilesOverviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
