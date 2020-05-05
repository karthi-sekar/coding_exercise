import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CareerplanComponent } from './careerplan.component';

describe('CareerplanComponent', () => {
  let component: CareerplanComponent;
  let fixture: ComponentFixture<CareerplanComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CareerplanComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CareerplanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
