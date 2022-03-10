import {ComponentFixture, TestBed} from '@angular/core/testing';

import {FileInfoListComponent} from './file-info-list.component';

describe('FileInfoListComponent', () => {
  let component: FileInfoListComponent;
  let fixture: ComponentFixture<FileInfoListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FileInfoListComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FileInfoListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
